// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// This static class is meant to contain optional helpers that a user might want to use as a reference or directly
    /// call. Please note: you cannot override a static method, and therefore, if you're attempting to create state
    /// variants that override a specific behavior, then you will need to create a protected abstract or virtual method
    /// in the base state and either implement or wrap one of these methods.
    /// </summary>
    public static class ControllerHelper {
        /// <summary>
        /// Returns the horizontal velocity of a given rigidbody.
        /// </summary>
        public static Vector3 GetHorizontalVelocity(Rigidbody rb) {
            var velocity = rb.linearVelocity;
            velocity.y = 0;

            return velocity;
        }

        public static float GetHorizontalSpeed(Rigidbody rb, Vector3 projectionPlane) =>
                Vector3.ProjectOnPlane(rb.linearVelocity, projectionPlane).magnitude;

        /// <summary>
        /// Returns the vertical speed relative to the specified up direction.
        /// </summary>
        public static float GetVerticalSpeed(Rigidbody rb, Vector3 upDirection) =>
                Vector3.Dot(rb.linearVelocity, upDirection);

        /// <summary>
        /// Returns the planar velocity projected onto the specified plane.
        /// </summary>
        public static Vector3 GetPlanarVelocity(Rigidbody rb, Vector3 planeNormal) =>
                Vector3.ProjectOnPlane(rb.linearVelocity, planeNormal);

        /// <summary>
        /// Returns the current speed of the rigidbody.
        /// </summary>
        public static float GetCurrentSpeed(Rigidbody rb) => rb.linearVelocity.magnitude;

        // ########################
        // INPUT PROCESSING HELPERS
        // ########################

        /// <summary>
        /// Converts an input direction to world space relative to a reference transform and up direction.
        /// </summary>
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

        /// <summary>
        /// Normalizes an input direction if magnitude exceeds 1 (useful for analog stick input).
        /// </summary>
        public static Vector2 NormalizeInputDirection(Vector2 inputDirection) =>
                inputDirection.magnitude > 1f
                        ? inputDirection.normalized
                        : inputDirection;

        // ################
        // ROTATION HELPERS
        // ################

        /// <summary>
        /// Smoothly rotates towards a target direction using the specified rotation speed.
        /// </summary>
        public static Quaternion RotateTowardsDirection(
            Quaternion currentRotation, Vector3 targetDirection, Vector3 upDirection, float rotationSpeed,
            float deltaTime) {
            if (targetDirection.sqrMagnitude < 1e-6f)
                return currentRotation;

            var targetRotation = Quaternion.LookRotation(targetDirection, upDirection);
            var maxRotationStep = rotationSpeed * deltaTime;

            return Quaternion.RotateTowards(currentRotation, targetRotation, maxRotationStep);
        }

        /// <summary>
        /// Returns the angle difference between current rotation and target direction.
        /// </summary>
        public static float GetRotationAngleDifference(
            Quaternion currentRotation, Vector3 targetDirection, Vector3 upDirection) {
            if (targetDirection.sqrMagnitude < 1e-6f)
                return 0f;

            var targetRotation = Quaternion.LookRotation(targetDirection, upDirection);

            return Quaternion.Angle(currentRotation, targetRotation);
        }

        // ###############
        // PHYSICS HELPERS
        // ###############

        /// <summary>
        /// Handles character rotation with angle-based speed falloff.
        /// This is the preferred rotation method for standard player-based character controllers.
        /// </summary>
        public static void HandleCharacterRotation(
            Rigidbody rb, Vector3 planarUp, float turnSpeed, float rotationFallOffAngle, float deltaTime) =>
                HandleCharacterRotation(rb, Vector3.ProjectOnPlane(rb.linearVelocity, planarUp), planarUp, turnSpeed,
                    rotationFallOffAngle, deltaTime);

        public static void HandleCharacterRotation(
            Rigidbody rb, Vector3 direction, Vector3 planarUp, float turnSpeed, float rotationFallOffAngle,
            float deltaTime) {
            var planarDirection = Vector3.ProjectOnPlane(direction, planarUp);

            if (planarDirection.sqrMagnitude < 1e-6f)
                return;

            var desiredDir = planarDirection.normalized;
            var targetRotation = Quaternion.LookRotation(desiredDir, planarUp);
            var angleDiff = Quaternion.Angle(rb.rotation, targetRotation);
            var speedFactor = Mathf.InverseLerp(0f, rotationFallOffAngle, angleDiff);

            var maxStepDeg = turnSpeed * speedFactor * deltaTime;

            var nextRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, maxStepDeg);

            rb.MoveRotation(nextRotation);
        }

        /// <summary>
        /// Applies force to achieve a target horizontal velocity.
        /// </summary>
        public static void ApplyHorizontalMovementForce(
            Rigidbody rb, Vector3 targetVelocity, float forceMultiplier, ForceMode forceMode) {
            var currentHorizontalVelocity = GetHorizontalVelocity(rb);
            var velocityDifference = targetVelocity - currentHorizontalVelocity;
            var force = velocityDifference * forceMultiplier;

            rb.AddForce(force, forceMode);
        }

        /// <summary>
        /// Removes the part of a vector that drives into a surface, leaving the component that slides along it.
        /// The surface normal points out of the surface (toward the mover).
        /// </summary>
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

        // #####################
        // GROUND PROBE & FLOAT
        // #####################

        /// <summary>
        /// Spherecasts along a direction for ground, reporting the hit distance, surface normal, and contact point.
        /// </summary>
        public static GroundProbeResult ProbeGround(
            Vector3 origin, Vector3 direction, float probeDistance, float probeRadius, LayerMask layers) {
            if (!Physics.SphereCast(origin, probeRadius, direction, out var hit, probeDistance, layers,
                    QueryTriggerInteraction.Ignore))
                return GroundProbeResult.Miss;

            return new GroundProbeResult(true, hit.distance + probeRadius, hit.normal, hit.point);
        }

        /// <summary>
        /// Returns the PD spring force that holds a floating body at its ride height along the probe direction.
        /// </summary>
        public static Vector3 SolveFloatSpring(
            Vector3 probeDirection, float hitDistance, float rideHeight, Vector3 currentVelocity, float strength,
            float damper) {
            var relativeVelocity = Vector3.Dot(probeDirection, currentVelocity);
            var offset = hitDistance - rideHeight;
            var springForce = offset * strength - relativeVelocity * damper;

            return probeDirection * springForce;
        }

        // ######
        // SLOPE
        // ######

        /// <summary>
        /// Returns the angle in degrees between a surface normal and the up direction.
        /// </summary>
        public static float GetSlopeAngle(Vector3 normal, Vector3 up) => Vector3.Angle(normal, up);

        /// <summary>
        /// Returns the downhill (fall-line) direction along a slope, or zero on flat ground.
        /// </summary>
        public static Vector3 GetSlopeDirection(Vector3 normal, Vector3 up) =>
                Vector3.ProjectOnPlane(-up, normal).normalized;

        /// <summary>
        /// Returns the horizontal move direction that yields constant speed along a slope surface.
        /// </summary>
        public static Vector3 GetSlopeAdjustedDirection(Vector3 horizontalDir, Vector3 normal, Vector3 up) {
            var magnitude = horizontalDir.magnitude;

            if (magnitude < 1e-4f)
                return Vector3.zero;

            var alongSlope = Vector3.ProjectOnPlane(horizontalDir, normal).normalized;

            return Vector3.ProjectOnPlane(alongSlope, up) * magnitude;
        }

        /// <summary>
        /// Returns the 1/cos(angle) factor (clamped) that grows a straight-down ground reach to track a slope.
        /// </summary>
        public static float GetSlopeReachFactor(float slopeAngle, float maxAngle) {
            var clamped = Mathf.Clamp(slopeAngle, 0f, maxAngle);

            return Mathf.Clamp(1f / Mathf.Cos(clamped * Mathf.Deg2Rad), 1f, 4f);
        }

        /// <summary>
        /// Returns the magnitude of gravitational acceleration along the fall line of a slope (g·sinθ).
        /// </summary>
        public static float GetSlopeAccelerationMagnitude(float gravityMagnitude, float slopeAngle) =>
                gravityMagnitude * Mathf.Sin(slopeAngle * Mathf.Deg2Rad);

        // ###########################
        // COLLISION & OVERLAP HELPERS
        // ###########################

        /// <summary>
        /// Checks for overlapping colliders within a specified radius.
        /// </summary>
        public static bool CheckOverlapSphere(Vector3 position, float radius, LayerMask layers) =>
                Physics.CheckSphere(position, radius, layers, QueryTriggerInteraction.Ignore);

        // ####################
        // MATHEMATICAL HELPERS
        // ####################

        /// <summary>
        /// Lerps between two values with a speed-based approach rather than time-based.
        /// </summary>
        public static float LerpWithSpeed(float current, float target, float speed, float deltaTime) {
            var maxChange = speed * deltaTime;
            var difference = target - current;
            var clampedChange = Mathf.Clamp(difference, -maxChange, maxChange);

            return current + clampedChange;
        }

        /// <summary>
        /// Smoothly damps a value towards a target using SmoothDamp.
        /// </summary>
        public static float SmoothDampFloat(
            float current, float target, ref float velocity, float smoothTime, float deltaTime,
            float maxSpeed = Mathf.Infinity) =>
                Mathf.SmoothDamp(current, target, ref velocity, smoothTime, maxSpeed, deltaTime);

        /// <summary>
        /// Maps a value from one range to another.
        /// </summary>
        public static float RemapValue(float value, float fromMin, float fromMax, float toMin, float toMax) {
            var normalizedValue = Mathf.InverseLerp(fromMin, fromMax, value);

            return Mathf.Lerp(toMin, toMax, normalizedValue);
        }
    }
}