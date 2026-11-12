// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// I partitioned this state to be the in-between of the state before it and grounding in the event you wanted to
    /// play a ground sound or a ground animation or delay the player from being able to jump immediately.
    /// </summary>
    [CreateAssetMenu(fileName = "LandingStateExample", menuName = "Spellbound/StateMachine/LandingStateExample")]
    public class LandingStateExample : BaseLocomotionStateExample {
        private float _landTimer;

        [Header("Animation")]
        [SerializeField, Tooltip("Animator state for a soft landing.")]
        private string landStateName = "Landing";

        [SerializeField, Tooltip("Animator state for a hard landing that rolls.")]
        private string rollStateName = "LandingRoll";

        [SerializeField, Min(0f), Tooltip("Seconds held in the soft-landing state before returning to grounded.")]
        private float landDuration = 0.15f;

        [SerializeField, Min(0f), Tooltip("Seconds held in the rolling-landing state before returning to grounded.")]
        private float rollDuration = 0.6f;

        private int _landHash;
        private int _rollHash;

        [field: SerializeField, Range(0f, 30f),
         Tooltip("Downward impact speed (m/s) at or above which the landing rolls instead of a soft landing.")]
        public float RollImpactSpeed { get; set; } = 8f;

        protected override void OnStateInitialize() {
            _landHash = Animator.StringToHash(landStateName);
            _rollHash = Animator.StringToHash(rollStateName);
        }

        protected override void EnterStateLogic() {
            var impact = -ControllerHelper.GetVerticalSpeed(Ctx.Rb, Ctx.PlanarUp);
            var roll = impact >= RollImpactSpeed;

            Ctx.Animation?.CrossFade(roll
                    ? _rollHash
                    : _landHash, PlayerControllerExample.BaseLayer);

            _landTimer = roll
                    ? rollDuration
                    : landDuration;
        }

        protected override void UpdateStateLogic() {
            _landTimer -= Time.deltaTime;

            if (_landTimer <= 0f)
                Ctx.LocoStateMachine.ChangeState(LocoStateTypes.Grounded);
        }

        protected override void FixedUpdateStateLogic() {
            if (PerformGroundCheck())
                KeepCapsuleFloating();
            HandleInput();
            HandleCharacterRotation();
        }

        protected override void ExitStateLogic() { }
    }
}