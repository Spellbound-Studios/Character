// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public static class SlopeMath {
        public static float GetSlopeAngle(Vector3 normal, Vector3 up) => Vector3.Angle(normal, up);

        public static Vector3 GetSlopeDirection(Vector3 normal, Vector3 up) =>
                Vector3.ProjectOnPlane(-up, normal).normalized;

        public static Vector3 GetSlopeAdjustedDirection(Vector3 horizontalDirection, Vector3 normal, Vector3 up) {
            var magnitude = horizontalDirection.magnitude;

            if (magnitude < 1e-4f)
                return Vector3.zero;

            var alongSlope = Vector3.ProjectOnPlane(horizontalDirection, normal).normalized;

            return Vector3.ProjectOnPlane(alongSlope, up) * magnitude;
        }

        public static float GetSlopeReachFactor(float slopeAngle, float maxAngle, float maximumReachFactor = 4f) {
            var clamped = Mathf.Clamp(slopeAngle, 0f, maxAngle);

            return Mathf.Clamp(1f / Mathf.Cos(clamped * Mathf.Deg2Rad), 1f, maximumReachFactor);
        }

        public static float GetSlopeAccelerationMagnitude(float gravityMagnitude, float slopeAngle) =>
                gravityMagnitude * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);

        public static Vector3 CancelIntoSurface(Vector3 vector, Vector3 surfaceNormal) =>
                vector - Mathf.Min(Vector3.Dot(vector, surfaceNormal), 0f) * surfaceNormal;

        public static Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal, Vector3 up) {
            var directionRight = Vector3.Cross(direction, up);

            return directionRight.sqrMagnitude > 1e-8f
                    ? Vector3.Cross(surfaceNormal, directionRight).normalized
                    : Vector3.ProjectOnPlane(direction, surfaceNormal).normalized;
        }

        public static Vector3 ProjectOntoCrease(Vector3 vector, Vector3 normalA, Vector3 normalB) {
            var crease = Vector3.Cross(normalA, normalB);

            if (crease.sqrMagnitude < 1e-6f)
                return CancelIntoSurface(vector, normalB);

            crease.Normalize();

            return Vector3.Dot(vector, crease) * crease;
        }
    }
}
