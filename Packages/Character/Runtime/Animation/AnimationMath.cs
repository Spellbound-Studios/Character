// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// Stateless helpers for feeding a single normalized blend axis and a stride-matched playback multiplier — the
    /// reusable math distilled from a blended-clip locomotion setup, with no dependency on any animation backend.
    /// </summary>
    public static class AnimationMath {
        /// <summary>
        /// Maps <paramref name="value"/> onto a 0..1 blend axis against ascending <paramref name="thresholds"/> — the
        /// value at which each blend point is fully shown. The first threshold maps to 0 and the last to 1, with the
        /// points in between landing on evenly spaced fractions (i / (count − 1)), interpolating linearly across the
        /// bracket the value falls in and clamping past the ends. Mirror these fractions on the backend's blend tree
        /// (e.g. three motions at 0, 0.5, 1) and each clip is pure exactly when the body moves at its threshold —
        /// thresholds stay live and stat-driven, so a runtime speed change reflows the blend with no re-authoring.
        /// </summary>
        public static float NormalizeBlend(float value, ReadOnlySpan<float> thresholds) {
            var count = thresholds.Length;

            if (count < 2)
                return 0f;

            if (value <= thresholds[0])
                return 0f;

            if (value >= thresholds[count - 1])
                return 1f;

            var lower = 0;

            while (lower < count - 1 && value > thresholds[lower + 1])
                lower++;

            var span = thresholds[lower + 1] - thresholds[lower];
            var frac = span > 1e-5f
                    ? (value - thresholds[lower]) / span
                    : 0f;

            return (lower + frac) / (count - 1);
        }

        /// <summary>
        /// The playback multiplier that scales a clip authored for <paramref name="authoredSpeed"/> (m/s of ground
        /// travel) to the body's actual <paramref name="currentSpeed"/>, so stride matches displacement and feet stop
        /// sliding. Returns 1 when no authored speed is given (an in-place clip, or nothing to match against).
        /// </summary>
        public static float SpeedWarp(float currentSpeed, float authoredSpeed) =>
                authoredSpeed > 1e-4f
                        ? currentSpeed / authoredSpeed
                        : 1f;
    }
}
