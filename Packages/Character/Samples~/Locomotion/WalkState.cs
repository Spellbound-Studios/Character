// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    [CreateAssetMenu(fileName = "WalkState", menuName = "Spellbound/Character/Samples/Locomotion/Walk State")]
    public sealed class WalkState : SupportedState {
        [SerializeField, Min(0f), Tooltip("Metres per second at full move input.")]
        private float walkSpeed = 2f;

        [SerializeField, Tooltip("The Animator state this state cross-fades to: a blend from idle to walking.")]
        private string animatorState = "Walk";

        [SerializeField, Tooltip("The Animator float set from 0 (standing) to 1 (walking at full speed).")]
        private string speedParameter = "Speed";

        private int _speedParameterHash;

        protected override float Speed => walkSpeed;

        protected override string AnimatorState => animatorState;

        protected override void OnStateInitialize() {
            base.OnStateInitialize();
            _speedParameterHash = Animator.StringToHash(speedParameter);
        }

        protected override void UpdateStateLogic() =>
                Locomotion.Animation?.SetFloat(_speedParameterHash, walkSpeed > 0f ? PlanarSpeed / walkSpeed : 0f);

        protected override bool TryChangePace() {
            if (!IsRunning)
                return false;

            Locomotion.ChangeState(LocomotionStateKey.Run);

            return true;
        }
    }
}
