// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Default airborne state: preserves the horizontal momentum you launched with and lets input only nudge your
    /// heading (additive air steering, never braking). Swap in <see cref="BallisticFallingStateExample"/> for the
    /// no-control variant — a one-asset swap that shows how each state encapsulates its own feel.
    /// </summary>
    [CreateAssetMenu(fileName = "FallingStateExample", menuName = "Spellbound/StateMachine/FallingStateExample")]
    public class FallingStateExample : BaseLocomotionStateExample {
        [field: SerializeField, Range(0f, 30f),
         Tooltip("Air steering strength. Nudges your preserved launch momentum toward input without braking it. " +
                 "Higher = more in-air control; lower = barely steerable; 0 = none (pure ballistic).")]
        public float AirControlAccel { get; set; } = 5f;

        [Header("Animation")]
        [SerializeField, Tooltip("Animator state (a 2-motion blend tree: idle-fall → terminal) this state plays.")]
        private string fallStateName = "Falling";

        [field: SerializeField, Range(1f, 40f),
         Tooltip("Downward speed (m/s) at which the fall blend reaches the terminal pose; slower falls ease back " +
                 "toward the idle-fall pose.")]
        public float TerminalFallSpeed { get; set; } = 20f;

        private int _fallHash;
        private readonly float[] _fallThresholds = new float[2];

        protected override void OnStateInitialize() => _fallHash = Animator.StringToHash(fallStateName);

        protected override void EnterStateLogic() =>
                Ctx.Animation?.CrossFade(_fallHash, PlayerControllerExample.BaseLayer);

        protected override void UpdateStateLogic() {
            if (Ctx.Animation == null)
                return;

            _fallThresholds[0] = 0f;
            _fallThresholds[1] = TerminalFallSpeed;

            var fallSpeed = -ControllerHelper.GetVerticalSpeed(Ctx.Rb, Ctx.PlanarUp);
            Ctx.Animation.SetFloat(Ctx.LocomotionBlend, AnimationMath.NormalizeBlend(fallSpeed, _fallThresholds));
        }

        protected override void FixedUpdateStateLogic() {
            if (PerformGroundCheck()) {
                Ctx.LocoStateMachine.ChangeState(LocoStateTypes.Landing);

                return;
            }

            HandleAirControl();
            HandleCharacterRotation();
        }

        protected override void ExitStateLogic() { }

        /// <summary>
        /// Adds a gentle steering acceleration toward input on top of the existing horizontal velocity, so the
        /// capsule keeps its momentum and only adjusts heading. Walls cancel the into-surface part.
        /// </summary>
        protected virtual void HandleAirControl() {
            var steer = GetInputDirectionRelativeToCamera() * AirControlAccel;

            Ctx.Rb.AddForce(CancelWallPush(steer), ForceMode.Acceleration);
        }
    }
}
