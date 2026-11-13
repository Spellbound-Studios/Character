// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Character.Body {
    [Serializable]
    public class DefaultColliderData {
        [Header("Collider Configuration")]
        [field: SerializeField,
         Tooltip("Capsule height. Auto-measured from the mesh unless Override Height is on; drives ride height " +
                 "and the ceiling probe.")]
        public float Height { get; private set; }

        [field: SerializeField,
         Tooltip("Capsule radius. Auto-measured from the mesh unless Override Radius is on. Wider = fatter body " +
                 "that fits through fewer gaps.")]
        public float Radius { get; private set; }

        [Header("Override Settings")]
        [field: SerializeField,
         Tooltip("Use the manual Height above instead of auto-measuring it from the mesh.")]
        public bool OverrideHeight { get; private set; }

        [field: SerializeField,
         Tooltip("Use the manual Radius above instead of auto-measuring it from the mesh.")]
        public bool OverrideRadius { get; private set; }

        [Header("Fallback And Limits")]
        [field: SerializeField,
         Tooltip("Height used when no mesh bounds are given or the measured height is not positive.")]
        public float FallbackHeight { get; set; } = 1.8f;

        [field: SerializeField,
         Tooltip("Radius used when no mesh bounds are given or the measured radius is not positive.")]
        public float FallbackRadius { get; set; } = 0.3f;

        [field: SerializeField, Tooltip("Smallest height the capsule is clamped to.")]
        public float MinimumHeight { get; set; } = 0.1f;

        [field: SerializeField, Tooltip("Largest height the capsule is clamped to.")]
        public float MaximumHeight { get; set; } = 10f;

        [field: SerializeField, Tooltip("Smallest radius the capsule is clamped to.")]
        public float MinimumRadius { get; set; } = 0.05f;

        [field: SerializeField, Tooltip("Largest radius the capsule is clamped to.")]
        public float MaximumRadius { get; set; } = 200f;

        [Header("Log Suppression")]
        [field: SerializeField,
         Tooltip("Silence console logs about collider auto-sizing. On by default to keep the console clean.")]
        public bool SuppressConsole { get; private set; } = true;

        public void Initialize(Bounds? meshBounds) {
            if (!meshBounds.HasValue) {
                if (!SuppressConsole)
                    Log.Warn("No mesh bounds were given. Using fallback values for non-overridden values.");

                SetFallbackValues();

                return;
            }

            var bounds = meshBounds.Value;

            if (!SuppressConsole)
                Log.Info($"Using mesh bounds of size {bounds.size}");

            if (!OverrideHeight) {
                Height = bounds.size.y;
                ValidateAndClampHeight();
            }

            if (!OverrideRadius) {
                var meshRadius = Mathf.Max(bounds.size.x, bounds.size.z) * 0.5f;
                Radius = meshRadius;
                ValidateAndClampRadius();
            }

            ValidateConfiguration();
        }

        private void SetFallbackValues() {
            if (!OverrideHeight) {
                Height = FallbackHeight;
                ValidateAndClampHeight();
            }

            if (!OverrideRadius) {
                Radius = FallbackRadius;
                ValidateAndClampRadius();
            }
        }

        private void ValidateAndClampHeight() {
            if (Height <= 0f) {
                Log.Warn($"Calculated height {Height} is invalid. Using fallback height {FallbackHeight}.");
                Height = FallbackHeight;
            }

            Height = Mathf.Clamp(Height, MinimumHeight, MaximumHeight);
        }

        private void ValidateAndClampRadius() {
            if (Radius <= 0f) {
                Log.Warn($"Calculated radius {Radius} is invalid. Using fallback radius {FallbackRadius}.");
                Radius = FallbackRadius;
            }

            Radius = Mathf.Clamp(Radius, MinimumRadius, MaximumRadius);
        }

        private void ValidateConfiguration() {
            if (!SuppressConsole)
                Log.Info($"DefaultColliderData initialized: Height={Height:F2}, Radius={Radius:F2}");
        }

        public void HandleValidation() {
            ValidateAndClampHeight();
            ValidateAndClampRadius();
            ValidateConfiguration();
        }
    }
}
