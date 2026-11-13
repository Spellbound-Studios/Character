// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public static class VelocityMath {
        public static Vector3 GetHorizontalVelocity(Rigidbody rigidbody) {
            var velocity = rigidbody.linearVelocity;
            velocity.y = 0;

            return velocity;
        }

        public static float GetHorizontalSpeed(Rigidbody rigidbody, Vector3 projectionPlane) =>
                Vector3.ProjectOnPlane(rigidbody.linearVelocity, projectionPlane).magnitude;

        public static float GetVerticalSpeed(Rigidbody rigidbody, Vector3 upDirection) =>
                Vector3.Dot(rigidbody.linearVelocity, upDirection);

        public static Vector3 GetPlanarVelocity(Rigidbody rigidbody, Vector3 planeNormal) =>
                Vector3.ProjectOnPlane(rigidbody.linearVelocity, planeNormal);

        public static float GetCurrentSpeed(Rigidbody rigidbody) => rigidbody.linearVelocity.magnitude;

        public static Vector3 GetInputDirectionRelativeToCamera(
            Vector2 inputDirection, Transform referenceTransform, Vector3 upDirection) {
            if (referenceTransform == null)
                return Vector3.zero;

            var rightProjected = Vector3.ProjectOnPlane(
                referenceTransform.right, upDirection).normalized;

            var forwardProjected = Vector3.ProjectOnPlane(
                referenceTransform.forward, upDirection).normalized;

            var direction = rightProjected * inputDirection.x + forwardProjected * inputDirection.y;

            return direction.magnitude > 1f
                    ? direction.normalized
                    : direction;
        }

        public static Vector2 NormalizeInputDirection(Vector2 inputDirection) =>
                inputDirection.magnitude > 1f
                        ? inputDirection.normalized
                        : inputDirection;

        public static void ApplyHorizontalMovementForce(
            Rigidbody rigidbody, Vector3 targetVelocity, float forceMultiplier, ForceMode forceMode) {
            var currentHorizontalVelocity = GetHorizontalVelocity(rigidbody);
            var velocityDifference = targetVelocity - currentHorizontalVelocity;
            var force = velocityDifference * forceMultiplier;

            rigidbody.AddForce(force, forceMode);
        }
    }
}
