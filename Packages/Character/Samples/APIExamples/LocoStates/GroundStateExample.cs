// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    [CreateAssetMenu(fileName = "GroundStateExample", menuName = "Spellbound/StateMachine/GroundStateExample")]
    public class GroundStateExample : BaseLocomotionStateExample {
        [SerializeField] private BaseSoState drunkenVariant;

        [Header("Animation")]
        [SerializeField, Tooltip("Animator state (a blend tree) this state crossfades to.")]
        private string locoStateName = "Grounded";

        [field: SerializeField, Range(1f, 3f),
         Tooltip("Sprint speed as a multiple of movement speed (always > 1, so sprint stays faster than run and the " +
                 "run→sprint blend can never invert). Scales with movement speed automatically — no held stat.")]
        public float SprintMultiplier { get; set; } = 1.6f;

        private int _locoHash;
        private float[] _thresholds;

        /// <summary>
        /// The Animator state this loco state plays. Override to re-skin locomotion for a variant (e.g. a drunken
        /// idle/walk blend tree).
        /// </summary>
        protected virtual string LocoStateName => locoStateName;

        /// <summary>
        /// How many blend points this state's blend tree mixes (idle → run → sprint = 3). Must match the motion
        /// count authored on the blend tree and the thresholds written in <see cref="WriteBlendThresholds"/>.
        /// </summary>
        protected virtual int BlendPointCount => 3;

        protected override void OnStateInitialize() {
            _locoHash = Animator.StringToHash(LocoStateName);
            _thresholds = new float[BlendPointCount];
        }

        protected override void EnterStateLogic() {
            Ctx.ExampleInput.OnInteractPressed += HandleInteractPressed;
            Ctx.ExampleInput.OnJumpPressed += HandleJumpPressed;

            Ctx.Animation?.CrossFade(_locoHash, PlayerControllerExample.BaseLayer);
        }

        protected override void UpdateStateLogic() {
            if (Ctx.Animation == null)
                return;

            var speed = ControllerHelper.GetHorizontalSpeed(Ctx.Rb, Ctx.PlanarUp);
            WriteBlendThresholds(_thresholds);
            Ctx.Animation.SetFloat(Ctx.LocomotionBlend, AnimationMath.NormalizeBlend(speed, _thresholds));
            Ctx.Animation.SetFloat(Ctx.SpeedWarp,
                Mathf.Max(1f, AnimationMath.SpeedWarp(speed, _thresholds[_thresholds.Length - 1])));
        }

        /// <summary>
        /// Fills each clip's blend threshold — the ground speed at which it is fully shown, aligned with
        /// <see cref="BuildBlendRoles"/>: idle = 0, run = movement speed, sprint = sprint speed. A clip is pure
        /// exactly when the body moves at its threshold, with no required ratio between them. Read live so a runtime
        /// movement-speed change (a buff) re-positions the blend automatically.
        /// </summary>
        protected virtual void WriteBlendThresholds(float[] thresholds) {
            thresholds[0] = 0f;
            thresholds[1] = Ctx.StatData.movementSpeed;
            thresholds[2] = Ctx.StatData.movementSpeed * SprintMultiplier;
        }

        protected override void FixedUpdateStateLogic() {
            if (!PerformGroundCheck()) {
                Ctx.LocoStateMachine.ChangeState(LocoStateTypes.Falling);

                return;
            }

            if (IsSlopeTooSteep()) {
                Ctx.LocoStateMachine.ChangeState(LocoStateTypes.Sliding);

                return;
            }

            KeepCapsuleFloating();
            HSpeedModifier = ResolveSpeedModifier();
            HandleInput();
            HandleCharacterRotation();
        }

        /// <summary>
        /// Movement speed multiplier this tick — full speed, or the sprint top while <c>IsSprinting</c>. Override
        /// for variants with a fixed pace (e.g. the drunken walk).
        /// </summary>
        protected virtual float ResolveSpeedModifier() =>
                Ctx.ExampleInput.IsSprinting
                        ? SprintMultiplier
                        : 1f;

        protected override void ExitStateLogic() {
            Ctx.ExampleInput.OnInteractPressed -= HandleInteractPressed;
            Ctx.ExampleInput.OnJumpPressed -= HandleJumpPressed;
        }

        // Swap this slot to the drunken variant (e.g. as if a potion were consumed).
        protected virtual void HandleInteractPressed() =>
                Ctx.LocoStateMachine.ApplyVariant(LocoStateTypes.Grounded, drunkenVariant);

        private void HandleJumpPressed() => Ctx.LocoStateMachine.ChangeState(LocoStateTypes.Jumping);
    }
}
