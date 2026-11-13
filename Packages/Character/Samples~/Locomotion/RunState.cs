// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    [CreateAssetMenu(fileName = "RunState", menuName = "Spellbound/Character/Samples/Locomotion/Run State")]
    public sealed class RunState : SupportedState {
        [SerializeField, Min(0f), Tooltip("Metres per second at full move input while run is held.")]
        private float runSpeed = 5.5f;

        [SerializeField, Tooltip("The Animator state this state cross-fades to.")]
        private string animatorState = "Run";

        protected override float Speed => runSpeed;

        protected override string AnimatorState => animatorState;

        protected override bool TryChangePace() {
            if (IsRunning)
                return false;

            Locomotion.ChangeState(LocomotionStateKey.Walk);

            return true;
        }
    }
}
