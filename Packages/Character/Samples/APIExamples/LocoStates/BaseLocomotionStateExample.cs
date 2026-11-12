// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    public abstract class BaseLocomotionStateExample : BaseSoState {
        protected new PlayerControllerExample Ctx;
        protected float HSpeedModifier = 1f;
        protected GroundProbeResult Ground = GroundProbeResult.Miss;

        protected override void OnCtxInitialized() => Ctx = base.Ctx as PlayerControllerExample;

        protected virtual void HandleInput() {
            var inputDir = GetInputDirectionRelativeToCamera();
            var moveDir = inputDir;
            var slopeSpeed = 1f;

            if (Ground.HasHit) {
                moveDir = ControllerHelper.GetSlopeAdjustedDirection(inputDir, Ground.Normal, Ctx.PlanarUp);
                slopeSpeed = ResolveSlopeSpeed(inputDir);
            }

            var targetVelocity = slopeSpeed * HSpeedModifier * Ctx.StatData.movementSpeed * moveDir;
            var velocityChange = CancelWallPush(targetVelocity - ControllerHelper.GetHorizontalVelocity(Ctx.Rb));

            Ctx.Rb.AddForce(velocityChange, Ctx.RigidbodyData.horizontalForceMode);
        }

        /// <summary>
        /// Samples the slope-speed curve at the signed, normalized slope along the move direction
        /// (-1 uphill, +1 downhill).
        /// </summary>
        protected virtual float ResolveSlopeSpeed(Vector3 inputDir) {
            var curve = Ctx.StatData.slopeSpeedCurve;

            if (curve == null || curve.length == 0)
                return 1f;

            var up = Ctx.PlanarUp;
            var angle = ControllerHelper.GetSlopeAngle(Ground.Normal, up);
            var maxAngle = Ctx.StatData.maxSlopeAngle;

            if (angle < 0.5f || maxAngle < 0.01f || inputDir.sqrMagnitude < 1e-4f)
                return curve.Evaluate(0f);

            var alongSlope = Vector3.ProjectOnPlane(inputDir, Ground.Normal).normalized;
            var alignment = Vector3.Dot(alongSlope, ControllerHelper.GetSlopeDirection(Ground.Normal, up));
            var signedSlope = Mathf.Clamp(angle / maxAngle * alignment, -1f, 1f);

            return curve.Evaluate(signedSlope);
        }

        protected virtual Vector3 GetInputDirectionRelativeToCamera() =>
                ControllerHelper.GetInputDirectionRelativeToCamera(
                    Ctx.ExampleInput.Direction,
                    Ctx.ReferenceTransform,
                    Ctx.PlanarUp
                );

        /// <summary>
        /// Probes for ground beneath the capsule with a slope- and speed-scaled reach (faster movement reaches
        /// further, so ramps and bumps don't flicker to falling) and caches the hit on this state. Returns
        /// whether grounded; each state acts on the return value in its own loop.
        /// </summary>
        protected virtual bool PerformGroundCheck() {
            var floatData = Ctx.ResizableCapsuleCollider.CapsuleFloatData;
            var reach = floatData.RideHeight * SlopeReachFactor() + floatData.GroundedTolerance +
                        ControllerHelper.GetHorizontalSpeed(Ctx.Rb, Ctx.PlanarUp) * floatData.GroundProbeSpeedScale;

            Ground = Ctx.ResizableCapsuleCollider.ProbeGround(
                -Ctx.PlanarUp, reach + floatData.ProbeRadius, Ctx.LayerData.GroundLayer);

            return Ground.HasHit && Ground.Distance <= reach;
        }

        /// <summary>
        /// Applies the float-spring force that holds the capsule at its slope-scaled ride height, suppressing
        /// the upward push and upward velocity when a ceiling is close above so the capsule never pops into it.
        /// Call only after <see cref="PerformGroundCheck"/> has reported grounded this tick.
        /// </summary>
        protected virtual void KeepCapsuleFloating() {
            var floatData = Ctx.ResizableCapsuleCollider.CapsuleFloatData;

            var springForce = ControllerHelper.SolveFloatSpring(
                -Ctx.PlanarUp,
                Ground.Distance,
                floatData.RideHeight * SlopeReachFactor(),
                Ctx.Rb.linearVelocity,
                floatData.SpringStrength,
                floatData.SpringDamper);

            var ceiling = ProbeCeiling();

            if (ceiling.HasHit) {
                springForce = ControllerHelper.CancelIntoSurface(springForce, ceiling.Normal);
                Ctx.Rb.linearVelocity = ControllerHelper.CancelIntoSurface(Ctx.Rb.linearVelocity, ceiling.Normal);
            }

            Ctx.Rb.AddForce(springForce, ForceMode.Acceleration);
        }

        /// <summary>
        /// Cancels the part of a horizontal push that drives into a wall — a probed surface steeper than the
        /// walkable <see cref="StatData.maxSlopeAngle"/> — so the capsule slides along it instead of pressing in.
        /// Steep faces are never ascendable, so this is also what keeps the capsule from climbing walls.
        /// </summary>
        protected Vector3 CancelWallPush(Vector3 horizontalForce) {
            if (horizontalForce.sqrMagnitude < 1e-6f)
                return horizontalForce;

            var firstWall = ProbeWall(horizontalForce);

            if (!firstWall.HasHit)
                return horizontalForce;

            var adjusted = ControllerHelper.CancelIntoSurface(horizontalForce, firstWall.Normal);

            if (adjusted.sqrMagnitude < 1e-6f)
                return Vector3.zero;

            var secondWall = ProbeWall(adjusted);

            if (!secondWall.HasHit)
                return adjusted;

            adjusted = ControllerHelper.ProjectOntoCrease(adjusted, firstWall.Normal, secondWall.Normal);

            if (adjusted.sqrMagnitude < 1e-6f)
                return Vector3.zero;

            return ProbeWall(adjusted).HasHit
                    ? Vector3.zero
                    : adjusted;
        }

        private GroundProbeResult ProbeWall(Vector3 push) {
            var wall = Ctx.ResizableCapsuleCollider.ProbeGround(
                push.normalized,
                Ctx.ResizableCapsuleCollider.CapsuleFloatData.WallProbeDistance,
                Ctx.LayerData.GroundLayer);

            return wall.HasHit && ControllerHelper.GetSlopeAngle(wall.Normal, Ctx.PlanarUp) > Ctx.StatData.maxSlopeAngle
                    ? wall
                    : GroundProbeResult.Miss;
        }

        /// <summary>
        /// Probes upward for a ceiling within <see cref="CapsuleFloatData.CeilingClearance"/> of the capsule top.
        /// </summary>
        protected GroundProbeResult ProbeCeiling() {
            var capsule = Ctx.ResizableCapsuleCollider;
            var distance = capsule.collider.bounds.extents.y + capsule.CapsuleFloatData.CeilingClearance;

            return capsule.ProbeGround(Ctx.PlanarUp, distance, Ctx.LayerData.GroundLayer);
        }

        /// <summary>
        /// The 1/cos factor that lengthens the straight-down ground reach so descents stay grounded.
        /// </summary>
        protected virtual float SlopeReachFactor() =>
                ControllerHelper.GetSlopeReachFactor(CurrentSlopeAngle(), Ctx.StatData.maxSlopeAngle);

        /// <summary>
        /// The angle in degrees of the last probed ground surface relative to the controller's up direction.
        /// </summary>
        protected float CurrentSlopeAngle() => ControllerHelper.GetSlopeAngle(Ground.Normal, Ctx.PlanarUp);

        /// <summary>
        /// True when the last probed surface is steeper than the walkable <see cref="StatData.maxSlopeAngle"/>.
        /// </summary>
        protected bool IsSlopeTooSteep() => CurrentSlopeAngle() > Ctx.StatData.maxSlopeAngle;

        protected virtual void HandleCharacterRotation() {
            var velocity = ControllerHelper.GetPlanarVelocity(Ctx.Rb, Ctx.PlanarUp);
            var minSpeed = Ctx.RotationData.FaceVelocityMinSpeed;

            var direction = velocity.sqrMagnitude >= minSpeed * minSpeed
                    ? velocity
                    : GetInputDirectionRelativeToCamera();

            ControllerHelper.HandleCharacterRotation(
                Ctx.Rb,
                direction,
                Ctx.PlanarUp,
                Ctx.RotationData.turnTowardsInputSpeed,
                Ctx.RotationData.RotationFallOffAngle,
                Time.fixedDeltaTime);
        }
    }
}
