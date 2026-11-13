// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public static class InterpolationMath {
        public static float LerpWithSpeed(float current, float target, float speed, float deltaTime) {
            var maxChange = speed * deltaTime;
            var difference = target - current;
            var clampedChange = Mathf.Clamp(difference, -maxChange, maxChange);

            return current + clampedChange;
        }

        public static float SmoothDampFloat(
            float current, float target, ref float velocity, float smoothTime, float deltaTime,
            float maxSpeed = Mathf.Infinity) =>
                Mathf.SmoothDamp(current, target, ref velocity, smoothTime, maxSpeed, deltaTime);

        public static float RemapValue(float value, float fromMin, float fromMax, float toMin, float toMax) {
            var normalizedValue = Mathf.InverseLerp(fromMin, fromMax, value);

            return Mathf.Lerp(toMin, toMax, normalizedValue);
        }
    }
}
