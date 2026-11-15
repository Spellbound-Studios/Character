// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    [DefaultExecutionOrder(-100)]
    public sealed class TransformReferenceFrameSource : ReferenceFrameSource {
        private Vector3 _previousPosition;
        private Quaternion _previousRotation = Quaternion.identity;
        private Vector3 _linearVelocity;
        private Vector3 _angularVelocity;

        public override Pose WorldPose => new(transform.position, transform.rotation);

        public override Vector3 AngularVelocity => _angularVelocity;

        public override Vector3 VelocityAt(Vector3 worldPoint) =>
                _linearVelocity + Vector3.Cross(_angularVelocity, worldPoint - transform.position);

        protected override void OnEnable() {
            transform.GetPositionAndRotation(out _previousPosition, out _previousRotation);
            _linearVelocity = Vector3.zero;
            _angularVelocity = Vector3.zero;
            base.OnEnable();
        }

        private void FixedUpdate() {
            transform.GetPositionAndRotation(out var position, out var rotation);
            var stepSeconds = Time.fixedDeltaTime;

            if (stepSeconds > 0f) {
                _linearVelocity = (position - _previousPosition) / stepSeconds;
                _angularVelocity = AngularVelocityBetween(_previousRotation, rotation, stepSeconds);
            }

            _previousPosition = position;
            _previousRotation = rotation;
        }

        private static Vector3 AngularVelocityBetween(Quaternion from, Quaternion to, float stepSeconds) {
            (to * Quaternion.Inverse(from)).ToAngleAxis(out var degrees, out var axis);

            if (degrees > 180f)
                degrees -= 360f;

            if (Mathf.Abs(degrees) <= Mathf.Epsilon || !float.IsFinite(axis.x))
                return Vector3.zero;

            return axis * (degrees * Mathf.Deg2Rad / stepSeconds);
        }
    }
}
