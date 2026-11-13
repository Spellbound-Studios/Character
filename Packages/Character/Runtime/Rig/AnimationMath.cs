// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Character.Rig {
    public static class AnimationMath {
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
            var fraction = span > 1e-5f
                    ? (value - thresholds[lower]) / span
                    : 0f;

            return (lower + fraction) / (count - 1);
        }

        public static float SpeedWarp(float currentSpeed, float authoredSpeed) =>
                authoredSpeed > 1e-4f
                        ? currentSpeed / authoredSpeed
                        : 1f;
    }
}
