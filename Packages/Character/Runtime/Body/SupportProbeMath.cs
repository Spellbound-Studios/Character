// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public static class SupportProbeMath {
        public static GroundProbeResult ProbeGround(
            Vector3 origin, Vector3 direction, float probeDistance, float probeRadius, LayerMask layers) {
            if (!Physics.SphereCast(origin, probeRadius, direction, out var hit, probeDistance, layers,
                    QueryTriggerInteraction.Ignore))
                return GroundProbeResult.Miss;

            return new GroundProbeResult(true, hit.distance + probeRadius, hit.normal, hit.point);
        }

        public static bool CheckOverlapSphere(Vector3 position, float radius, LayerMask layers) =>
                Physics.CheckSphere(position, radius, layers, QueryTriggerInteraction.Ignore);
    }
}
