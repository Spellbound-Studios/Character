// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [Serializable]
    public class StatData {
        [Header("Stat Values")]
        [field: SerializeField, Range(0f, 20f),
         Tooltip("Ground run speed in m/s (the target horizontal speed input drives toward). Higher = faster " +
                 "running; lower = slower.")]
        public float movementSpeed { get; set; } = 5f;

        [field: SerializeField, Range(0f, 30f),
         Tooltip("Upward impulse applied when jumping. Higher = higher jumps; lower = shorter hops.")]
        public float jumpForce { get; set; } = 5f;

        [field: SerializeField, Range(0f, 5f),
         Tooltip("Extra multiplier on jump force. 1 = normal; above 1 = boosted jump height; below 1 = weaker jump.")]
        public float JumpMultiplier { get; set; } = 1f;

        [field: SerializeField, Range(-50f, 0f),
         Tooltip("Gravity reference the slide uses to pull down steep slopes (more negative = stronger pull, " +
                 "faster slides). Note: ordinary falling uses Unity's Physics.gravity, not this value.")]
        public float gravity { get; set; } = -20f;

        [Header("Slope")]
        [field: SerializeField, Range(0f, 89f),
         Tooltip("Steepest slope you can stand/walk on; steeper than this you slide. Higher = climb steeper hills; " +
                 "lower = slide off gentler inclines. Also the cutoff for walls you cannot push into.")]
        public float maxSlopeAngle { get; set; } = 50f;

        [field: SerializeField,
         Tooltip("Speed multiplier vs slope along your move direction. x: -1 full uphill, 0 flat/traverse, +1 full " +
                 "downhill. y: the multiplier (raise the uphill end to climb faster, the downhill end to descend faster).")]
        public AnimationCurve slopeSpeedCurve { get; set; } =
                new(new Keyframe(-1f, 0.4f), new Keyframe(-0.5f, 0.85f), new Keyframe(0f, 1f), new Keyframe(1f, 1.3f));

        [Header("Slide")]
        [field: SerializeField, Range(0f, 100f),
         Tooltip("Hard cap on horizontal slide speed (m/s). Higher = lets slides build more speed; lower = tamer slides.")]
        public float TerminalSlidingSpeed { get; set; } = 50f;

        [field: SerializeField, Range(0f, 5f),
         Tooltip("Multiplier on the gravity-driven slide acceleration. Higher = slides accelerate harder / feel " +
                 "slipperier; lower = gentler slides.")]
        public float slideAccelMultiplier { get; set; } = 2.0f;

        [field: SerializeField, Range(0f, 20f),
         Tooltip("How strongly input steers you sideways across the fall line while sliding. Higher = sharper " +
                 "carving; lower = you mostly go straight down the slope.")]
        public float lateralSteerAccel { get; set; } = 8.0f;

        [field: SerializeField, Range(0f, 10f),
         Tooltip("Drag on horizontal speed while sliding. Higher = slower, more controlled slides (lower terminal " +
                 "speed); lower = faster, slipperier slides.")]
        public float planarDrag { get; set; } = 1.2f;
    }
}
