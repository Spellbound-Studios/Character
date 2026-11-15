// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core;
using Spellbound.Core.Observations;

namespace Spellbound.Character.Body {
    public static class CharacterObservations {
        public static readonly Occurrence ReferenceFrameBound = CoreKit.Observations.Occurrence(
            "character.reference_frame.bound", "A body started riding a moving reference frame.");
    }
}
