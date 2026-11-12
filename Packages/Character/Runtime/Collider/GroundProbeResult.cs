// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// Outcome of a ground probe: whether ground was found, the origin-to-surface distance, the surface normal, and
    /// the contact point.
    /// </summary>
    public readonly struct GroundProbeResult {
        public readonly bool HasHit;
        public readonly float Distance;
        public readonly Vector3 Normal;
        public readonly Vector3 Point;

        public GroundProbeResult(bool hasHit, float distance, Vector3 normal, Vector3 point) {
            HasHit = hasHit;
            Distance = distance;
            Normal = normal;
            Point = point;
        }

        public static GroundProbeResult Miss => new(false, float.PositiveInfinity, Vector3.up, Vector3.zero);
    }
}
