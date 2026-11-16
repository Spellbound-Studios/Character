// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Samples.SwimAndFly {
    [Serializable]
    public sealed class FlyDriver : SampleMotionDriver {
        [SerializeField, Tooltip("The flying speed, in metres per second.")]
        private float flySpeed = 6f;

        public override int Priority => 20;

        public override bool NeedsSupport => false;

        public float FlySpeed {
            get => flySpeed;
            set => flySpeed = value;
        }

        protected override Vector3 Velocity(Rigidbody body, Vector3 desiredMove, Vector3 up, float stepSeconds) {
            return Vector3.ClampMagnitude(desiredMove, 1f) * flySpeed;
        }
    }
}
