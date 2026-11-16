// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public sealed class PhysicsSupportProbe : ISupportProbe {
        public GroundProbeResult Probe(
            PhysicsScene scene, Vector3 origin, Vector3 down, float distance, float radius, int layers) {
            if (radius <= 0f) {
                if (!scene.Raycast(origin, down, out var rayHit, distance, layers, QueryTriggerInteraction.Ignore))
                    return GroundProbeResult.Miss;

                return new GroundProbeResult(true, rayHit.distance, rayHit.normal, rayHit.point);
            }

            if (!scene.SphereCast(origin, radius, down, out var hit, distance, layers, QueryTriggerInteraction.Ignore))
                return GroundProbeResult.Miss;

            return new GroundProbeResult(true, hit.distance + radius, hit.normal, hit.point);
        }
    }
}
