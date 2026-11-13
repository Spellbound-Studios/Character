// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    public abstract class AirborneState : LocomotionState {
        [SerializeField, Min(0f), Tooltip("Metres per second the body steers toward in the air.")]
        private float airSpeed = 4f;

        [SerializeField, Min(0f), Tooltip("Metres per second squared of steering in the air. 0 = no air control.")]
        private float airAcceleration = 10f;

        [SerializeField, Min(0f), Tooltip("Degrees per second the body turns toward its move in the air.")]
        private float airTurnSpeed = 360f;

        protected void SteerInAir() {
            MoveTowards(Body.DesiredMove, airSpeed, airAcceleration);
            TurnTowards(Body.DesiredMove, airTurnSpeed);
        }
    }
}
