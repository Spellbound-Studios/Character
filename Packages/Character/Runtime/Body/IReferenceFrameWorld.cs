// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.Observations;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Core.Simulation;

namespace Spellbound.Character.Body {
    public interface IReferenceFrameWorld {
        ReferenceFrameDirectory ReferenceFrames { get; }
        ObservationContext Observations { get; }
        Tick CurrentTick { get; }
    }
}
