// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    [CreateAssetMenu(fileName = "JumpState", menuName = "Spellbound/Character/Samples/Locomotion/Jump State")]
    public sealed class JumpState : AirborneState {
        [SerializeField, Min(0f), Tooltip("Metres per second of upward speed the jump starts with.")]
        private float jumpImpulse = 5f;

        [SerializeField, Tooltip("The Animator state this state cross-fades to.")]
        private string animatorState = "Jump";

        protected override string AnimatorState => animatorState;

        protected override void EnterStateLogic() {
            base.EnterStateLogic();

            var rigidbody = Body.Rigidbody;
            var up = Up;
            rigidbody.linearVelocity = Vector3.ProjectOnPlane(rigidbody.linearVelocity, up) + up * jumpImpulse;
        }

        protected override void FixedUpdateStateLogic() {
            if (VerticalSpeed <= 0f) {
                Locomotion.ChangeState(LocomotionStateKey.Fall);

                return;
            }

            SteerInAir();
        }
    }
}
