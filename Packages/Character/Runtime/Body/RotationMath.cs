// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public static class RotationMath {
        public static Quaternion RotateTowardsDirection(
            Quaternion currentRotation, Vector3 targetDirection, Vector3 upDirection, float rotationSpeed,
            float deltaTime) {
            if (targetDirection.sqrMagnitude < 1e-6f)
                return currentRotation;

            var targetRotation = Quaternion.LookRotation(targetDirection, upDirection);
            var maxRotationStep = rotationSpeed * deltaTime;

            return Quaternion.RotateTowards(currentRotation, targetRotation, maxRotationStep);
        }

        public static float GetRotationAngleDifference(
            Quaternion currentRotation, Vector3 targetDirection, Vector3 upDirection) {
            if (targetDirection.sqrMagnitude < 1e-6f)
                return 0f;

            var targetRotation = Quaternion.LookRotation(targetDirection, upDirection);

            return Quaternion.Angle(currentRotation, targetRotation);
        }

        public static void HandleCharacterRotation(
            Rigidbody rigidbody, Vector3 planarUp, float turnSpeed, float rotationFallOffAngle, float deltaTime) =>
                HandleCharacterRotation(rigidbody, Vector3.ProjectOnPlane(rigidbody.linearVelocity, planarUp),
                    planarUp, turnSpeed, rotationFallOffAngle, deltaTime);

        public static void HandleCharacterRotation(
            Rigidbody rigidbody, Vector3 direction, Vector3 planarUp, float turnSpeed, float rotationFallOffAngle,
            float deltaTime) {
            var planarDirection = Vector3.ProjectOnPlane(direction, planarUp);

            if (planarDirection.sqrMagnitude < 1e-6f)
                return;

            var desiredDirection = planarDirection.normalized;
            var targetRotation = Quaternion.LookRotation(desiredDirection, planarUp);
            var angleDifference = Quaternion.Angle(rigidbody.rotation, targetRotation);
            var speedFactor = Mathf.InverseLerp(0f, rotationFallOffAngle, angleDifference);

            var maxStepDegrees = turnSpeed * speedFactor * deltaTime;

            var nextRotation = Quaternion.RotateTowards(rigidbody.rotation, targetRotation, maxStepDegrees);

            rigidbody.MoveRotation(nextRotation);
        }
    }
}
