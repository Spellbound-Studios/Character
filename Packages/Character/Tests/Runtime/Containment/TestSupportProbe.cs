// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Tests {
    public sealed class TestSupportProbe : ISupportProbe {
        public GroundProbeResult Result { get; set; } = GroundProbeResult.Miss;

        public GroundProbeResult Probe(
            PhysicsScene scene, Vector3 origin, Vector3 down, float distance, float radius, int layers) => Result;
    }
}
