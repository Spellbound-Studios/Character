// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [Serializable]
    public class RotationData {
        [Header("Rotation Settings")]
        [field: SerializeField, Range(0f, 2000f),
         Tooltip("How fast the character turns to face its movement direction (deg/s). Higher = snappier, more " +
                 "responsive turns; lower = slower, floatier turning.")]
        public float turnTowardsInputSpeed { get; set; } = 1000f;

        [field: SerializeField, Range(0f, 180f),
         Tooltip("Heading error (degrees) at which the turn reaches full speed; below it the turn eases off. " +
                 "Higher = gentler, more gradual small corrections; lower = even tiny corrections snap to face the input.")]
        public float RotationFallOffAngle { get; set; } = 90f;

        [field: SerializeField, Range(0f, 5f)]
        public float FaceVelocityMinSpeed { get; set; } = 0.5f;
    }
}
