// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [Serializable]
    public class RigidbodyData {
        [Header("Rigidbody Settings:")]
        [field: SerializeField,
         Tooltip("How horizontal movement force is applied. VelocityChange = instant, mass-independent, snappy " +
                 "control (recommended); Acceleration/Force = more gradual ramp-up.")]
        public ForceMode horizontalForceMode = ForceMode.VelocityChange;

        [field: SerializeField,
         Tooltip("How the jump/vertical force is applied. Impulse = instant pop scaled by mass; VelocityChange = " +
                 "instant and mass-independent.")]
        public ForceMode verticalForceMode = ForceMode.Impulse;
    }
}
