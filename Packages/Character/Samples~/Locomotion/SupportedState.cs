// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    public abstract class SupportedState : LocomotionState {
        [SerializeField, Min(0f), Tooltip("Metres per second squared the body gains or sheds on its way to its speed.")]
        private float acceleration = 40f;

        [SerializeField, Min(0f), Tooltip("Degrees per second the body turns toward its move.")]
        private float turnSpeed = 720f;

        protected abstract float Speed { get; }

        protected bool IsRunning => Locomotion.RunRequested && Body.DesiredMove.sqrMagnitude > 1e-6f;

        protected abstract bool TryChangePace();

        protected override void FixedUpdateStateLogic() {
            if (!Body.Support.HasHit) {
                Locomotion.ChangeState(LocomotionStateKey.Fall);

                return;
            }

            if (Locomotion.JumpRequested) {
                Locomotion.ChangeState(LocomotionStateKey.Jump);

                return;
            }

            if (TryChangePace())
                return;

            HoldRideHeight();
            MoveTowards(SlopeMath.GetSlopeAdjustedDirection(Body.DesiredMove, Body.Support.Normal, Up), Speed,
                acceleration);
            TurnTowards(Body.DesiredMove, turnSpeed);
        }
    }
}
