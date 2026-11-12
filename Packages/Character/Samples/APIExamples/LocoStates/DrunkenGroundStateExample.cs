// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    [CreateAssetMenu(fileName = "DrunkenGroundStateExample",
        menuName = "Spellbound/StateMachine/DrunkenGroundStateExample")]
    public class DrunkenGroundStateExample : GroundStateExample {
        [SerializeField, Tooltip("Animator state (a 2-motion blend tree) for the drunken idle/walk.")]
        private string drunkStateName = "DrunkenGrounded";

        [field: SerializeField, Range(0.2f, 0.6f),
         Tooltip("Movement speed multiplier while drunk — a slow, sloppy walk (≈ 1/3 to 1/2 of normal).")]
        public float DrunkenSpeedModifier { get; set; } = 0.4f;

        protected override string LocoStateName => drunkStateName;

        protected override int BlendPointCount => 2;

        protected override float ResolveSpeedModifier() => DrunkenSpeedModifier;

        protected override void WriteBlendThresholds(float[] thresholds) {
            thresholds[0] = 0f;
            thresholds[1] = Ctx.StatData.movementSpeed * DrunkenSpeedModifier;
        }
        
        protected override Vector3 GetInputDirectionRelativeToCamera() {
            // Reference transform right and forward projected on this transforms up normal plane to get a proper direction.
            var direction =
                    Vector3.ProjectOnPlane(
                        Ctx.ReferenceTransform.right, Ctx.PlanarUp).normalized *
                    Ctx.ExampleInput.Direction.y +
                    Vector3.ProjectOnPlane(
                        Ctx.ReferenceTransform.forward, Ctx.PlanarUp).normalized *
                    Ctx.ExampleInput.Direction.x;

            return direction.magnitude > 1f
                    ? direction.normalized
                    : direction;
        }

        // Swap the slot back to its default (the base ground state).
        protected override void HandleInteractPressed() =>
                Ctx.LocoStateMachine.RestoreDefault(LocoStateTypes.Grounded);
    }
}
