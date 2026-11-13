// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    [CreateAssetMenu(fileName = "FallState", menuName = "Spellbound/Character/Samples/Locomotion/Fall State")]
    public sealed class FallState : AirborneState {
        [SerializeField, Tooltip("The Animator state this state cross-fades to.")]
        private string animatorState = "Fall";

        protected override string AnimatorState => animatorState;

        protected override void FixedUpdateStateLogic() {
            if (Body.Support.HasHit) {
                Locomotion.ChangeState(LocomotionStateKey.Walk);

                return;
            }

            SteerInAir();
        }
    }
}
