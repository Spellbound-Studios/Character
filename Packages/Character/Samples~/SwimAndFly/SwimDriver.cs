// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Samples.SwimAndFly {
    [Serializable]
    public sealed class SwimDriver : SampleMotionDriver {
        [SerializeField, Tooltip("The height along up the swimmer drifts to, in metres.")]
        private float waterHeight;

        [SerializeField, Tooltip("The swimming speed, in metres per second.")]
        private float swimSpeed = 3f;

        [SerializeField, Tooltip("How fast the swimmer closes on the water height, per second.")]
        private float settleRate = 4f;

        public override int Priority => 10;

        public override bool NeedsSupport => false;

        public float WaterHeight {
            get => waterHeight;
            set => waterHeight = value;
        }

        public float SwimSpeed {
            get => swimSpeed;
            set => swimSpeed = value;
        }

        public float SettleRate {
            get => settleRate;
            set => settleRate = value;
        }

        protected override Vector3 Velocity(Rigidbody body, Vector3 desiredMove, Vector3 up, float stepSeconds) {
            var planar = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(desiredMove, up), 1f) * swimSpeed;
            var height = Vector3.Dot(body.position, up);
            var drift = Mathf.Clamp((waterHeight - height) * settleRate, -swimSpeed, swimSpeed);

            return planar + up * drift;
        }
    }
}
