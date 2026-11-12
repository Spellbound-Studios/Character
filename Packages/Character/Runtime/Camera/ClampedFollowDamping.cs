// Copyright 2026 Spellbound Studio Inc.

using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller {
    public class ClampedFollowDamping : CinemachineExtension {
        [SerializeField, Range(0f, 30f),
         Tooltip("How quickly the camera eases toward the follow target when it moves. Higher = tighter follow; " +
                 "0 = rigid, no ease.")]
        private float damping = 6f;

        [SerializeField, Range(0f, 3f),
         Tooltip("Hard cap in meters on how far the camera may trail the follow target in any direction, so fast " +
                 "acceleration never leaves the character behind. 0 = no cap.")]
        private float maxLag = 0.35f;

        private Vector3 _trackedPosition;
        private Vector3 _displacement;
        private Quaternion _bodyOrientation;
        private bool _hasTrackedPosition;

        public float Damping {
            get => damping;
            set => damping = value;
        }

        public float MaxLag {
            get => maxLag;
            set => maxLag = value;
        }

        protected override void OnEnable() {
            base.OnEnable();
            _hasTrackedPosition = false;
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {
            if (stage == CinemachineCore.Stage.Body)
                ApplyPositionLag(vcam, ref state, deltaTime);
            else if (stage == CinemachineCore.Stage.Aim)
                CancelLagRotation(ref state);
        }

        private void ApplyPositionLag(CinemachineVirtualCameraBase vcam, ref CameraState state, float deltaTime) {
            _displacement = Vector3.zero;
            _bodyOrientation = state.RawOrientation;

            var target = vcam.Follow;

            if (target == null)
                return;

            var targetPosition = target.position;

            if (deltaTime < 0f || !_hasTrackedPosition || damping <= 0f) {
                _trackedPosition = targetPosition;
                _hasTrackedPosition = true;

                return;
            }

            var blend = 1f - Mathf.Exp(-damping * deltaTime);
            _trackedPosition = Vector3.Lerp(_trackedPosition, targetPosition, blend);

            var displacement = _trackedPosition - targetPosition;

            if (maxLag > 0f && displacement.sqrMagnitude > maxLag * maxLag) {
                displacement = displacement.normalized * maxLag;
                _trackedPosition = targetPosition + displacement;
            }

            state.RawPosition += displacement;
            _displacement = displacement;
        }

        private void CancelLagRotation(ref CameraState state) {
            if (_displacement.sqrMagnitude < 1e-8f || !state.HasLookAt())
                return;

            if (Quaternion.Angle(_bodyOrientation, state.RawOrientation) < 1e-3f)
                return;

            var actualDirection = state.ReferenceLookAt - state.GetCorrectedPosition();
            var desiredDirection = actualDirection + _displacement;

            if (actualDirection.sqrMagnitude < 1e-6f || desiredDirection.sqrMagnitude < 1e-6f)
                return;

            var correction = Quaternion.FromToRotation(actualDirection, desiredDirection);
            var forward = correction * (state.RawOrientation * Vector3.forward);
            state.RawOrientation = Quaternion.LookRotation(forward, state.ReferenceUp);
        }
    }
}
