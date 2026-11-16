// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public interface ISupportProbe {
        GroundProbeResult Probe(
            PhysicsScene scene, Vector3 origin, Vector3 down, float distance, float radius, int layers);
    }
}
