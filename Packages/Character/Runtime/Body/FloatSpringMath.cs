// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public static class FloatSpringMath {
        public static Vector3 SolveFloatSpring(
            Vector3 probeDirection, float hitDistance, float rideHeight, Vector3 currentVelocity, float strength,
            float damper) {
            var relativeVelocity = Vector3.Dot(probeDirection, currentVelocity);
            var offset = hitDistance - rideHeight;
            var springForce = offset * strength - relativeVelocity * damper;

            return probeDirection * springForce;
        }
    }
}
