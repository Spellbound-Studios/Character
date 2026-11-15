// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using Spellbound.Core.Observations;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Core.Simulation;
using UnityEngine;

namespace Spellbound.Character.Tests {
    public sealed class TestReferenceFrameWorld : MonoBehaviour, IReferenceFrameWorld {
        public ReferenceFrameDirectory ReferenceFrames { get; } = new();

        public ObservationContext Observations { get; set; } = ObservationContext.Discard;

        public Tick CurrentTick { get; set; }
    }
}
