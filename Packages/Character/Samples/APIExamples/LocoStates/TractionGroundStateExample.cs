// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    [CreateAssetMenu(fileName = "TractionGroundStateExample",
        menuName = "Spellbound/StateMachine/TractionGroundStateExample")]
    public class TractionGroundStateExample : GroundStateExample {
        [SerializeField, Range(1f, 50f)] private float movementSharpness = 15f;

        protected override void HandleInput() {
            if (Ground.HasHit)
                ReorientVelocityAlongGround();

            var inputDir = GetInputDirectionRelativeToCamera();
            var moveDir = inputDir;
            var slopeSpeed = 1f;

            if (Ground.HasHit) {
                moveDir = ControllerHelper.GetSlopeAdjustedDirection(inputDir, Ground.Normal, Ctx.PlanarUp);
                slopeSpeed = ResolveSlopeSpeed(inputDir);
            }

            var targetVelocity = slopeSpeed * HSpeedModifier * Ctx.StatData.movementSpeed * moveDir;
            var blend = 1f - Mathf.Exp(-movementSharpness * Time.fixedDeltaTime);
            var velocityChange =
                    CancelWallPush((targetVelocity - ControllerHelper.GetHorizontalVelocity(Ctx.Rb)) * blend);

            Ctx.Rb.AddForce(velocityChange, Ctx.RigidbodyData.horizontalForceMode);
        }

        private void ReorientVelocityAlongGround() {
            var velocity = Ctx.Rb.linearVelocity;
            var speed = velocity.magnitude;

            if (speed < 1e-3f)
                return;

            var tangent = ControllerHelper.GetDirectionTangentToSurface(velocity, Ground.Normal, Ctx.PlanarUp);
            Ctx.Rb.linearVelocity = tangent * speed;
        }
    }
}
