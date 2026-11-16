// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core;
using Spellbound.Core.Observations;

namespace Spellbound.Character.Body {
    public static class CharacterObservations {
        public static readonly Occurrence ReferenceFrameBound = CoreKit.Observations.Occurrence(
            "character.reference_frame.bound", "A body started riding a moving reference frame.");

        public static readonly Occurrence ContainmentRescue = CoreKit.Observations.Occurrence(
            "character.containment.rescue",
            "A body was moved out of containment trouble: the reason, then from x y z and to x y z as float bits.");

        public static readonly Guard ContainmentRepair = CoreKit.Observations.Guard(
            "character.containment.repair", "A body in containment trouble was repaired by a rescue.",
            GuardPolicyValues.Default);

        public static readonly Guard SupportHeld = CoreKit.Observations.Guard(
            "character.support.held", "A body was held still until the ground under it was ready.",
            GuardPolicyValues.Default);
    }
}
