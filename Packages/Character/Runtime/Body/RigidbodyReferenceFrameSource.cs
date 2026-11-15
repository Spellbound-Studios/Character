// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyReferenceFrameSource : ReferenceFrameSource {
        private Rigidbody _body;
        private Vector3 _movedLinearVelocity;
        private Vector3 _movedAngularVelocity;
        private double _movedAtFixedTime = double.NaN;

        private Rigidbody Body {
            get {
                if (_body == null)
                    _body = GetComponent<Rigidbody>();

                return _body;
            }
        }

        private bool MovedThisStep => _movedAtFixedTime == Time.fixedTimeAsDouble;

        public override Pose WorldPose => new(Body.position, Body.rotation);

        public override Vector3 AngularVelocity => MovedThisStep ? _movedAngularVelocity : Body.angularVelocity;

        public override Vector3 VelocityAt(Vector3 worldPoint) {
            if (!MovedThisStep)
                return Body.GetPointVelocity(worldPoint);

            return _movedLinearVelocity + Vector3.Cross(_movedAngularVelocity, worldPoint - Body.position);
        }

        public void Move(Pose target) {
            var stepSeconds = Time.fixedDeltaTime;

            if (stepSeconds > 0f) {
                _movedLinearVelocity = (target.position - Body.position) / stepSeconds;
                _movedAngularVelocity = AngularVelocityBetween(Body.rotation, target.rotation, stepSeconds);
                _movedAtFixedTime = Time.fixedTimeAsDouble;
            }

            Body.MovePosition(target.position);
            Body.MoveRotation(target.rotation);
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
