// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller {
    [Serializable]
    public class CapsuleFloatData {
        [Header("Ride Height")]
        [field: SerializeField, Range(0f, 5f),
         Tooltip("Hover height above the ground when Override Calculated Distance is on. Higher = floats taller; " +
                 "lower = sits closer to the ground.")]
        public float DesiredFloatDistance { get; set; } = 1f;

        [field: SerializeField,
         Tooltip("Hover height auto-derived from the collider. Read-only unless Override Calculated Distance is on.")]
        public float CalculatedFloatDistance { get; private set; }

        [field: SerializeField,
         Tooltip("Use Desired Float Distance instead of the auto-calculated one. On = manual ride height; off = automatic.")]
        public bool OverrideCalculatedDistance { get; set; }

        [Header("Float Spring")]
        [field: SerializeField, Range(0f, 500f),
         Tooltip("Stiffness of the spring holding the capsule at ride height. Higher = firmer, snappier hover " +
                 "(too high jitters); lower = softer, saggier float.")]
        public float SpringStrength { get; set; } = 200f;

        [field: SerializeField, Range(0f, 100f),
         Tooltip("Damping on the float spring's vertical motion. Higher = settles fast with little bounce (too " +
                 "high feels stiff); lower = bouncier landings.")]
        public float SpringDamper { get; set; } = 25f;

        [Header("Ground Probe")]
        [field: SerializeField, Range(0.01f, 1f),
         Tooltip("Radius of the sphere cast down for ground. Higher = more forgiving over edges/gaps; lower = " +
                 "more precise but can slip off small ledges.")]
        public float ProbeRadius { get; set; } = 0.25f;

        [field: SerializeField, Range(0f, 1f),
         Tooltip("Extra distance below ride height still counted as grounded. Higher = stickier to ground over " +
                 "bumps; lower = leaves the ground sooner.")]
        public float GroundedTolerance { get; set; } = 0.3f;

        [field: SerializeField, Range(0f, 2f),
         Tooltip("Extra ground reach added per unit of horizontal speed, so fast movement over ramps/bumps stays " +
                 "grounded instead of flickering to falling. Higher = stickier at speed; 0 = constant reach.")]
        public float GroundProbeSpeedScale { get; set; } = 0.1f;

        [Header("Wall & Ceiling")]
        [field: SerializeField, Range(0f, 1f),
         Tooltip("How far ahead to detect walls to slide along. Higher = reacts to walls sooner (can catch on " +
                 "nearby geometry); lower = must get closer before sliding.")]
        public float WallProbeDistance { get; set; } = 0.35f;

        [field: SerializeField, Range(0f, 1f),
         Tooltip("Clearance above the capsule's head for ceiling detection. Higher = suppresses float under taller " +
                 "overhangs; lower = only very low ceilings stop the float pop.")]
        public float CeilingClearance { get; set; } = 0.15f;

        [Header("Step")]
        [field: SerializeField, Range(0f, 1f),
         Tooltip("Fraction of character height treated as step height; raises the collider center so small steps " +
                 "are climbable. Higher = steps over taller ledges; lower = trips on small steps.")]
        public float StepHeightPercentage { get; private set; } = 0.25f;

        public float RideHeight =>
                OverrideCalculatedDistance
                        ? DesiredFloatDistance
                        : CalculatedFloatDistance;

        public void SetCalculatedFloatDistance(float distance) => CalculatedFloatDistance = distance;
    }
}
