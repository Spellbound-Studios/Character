// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public readonly struct RescueStep {
        public RescueStep(Pose target, RescueRung rung, ContainmentReason reason) {
            Target = target;
            Rung = rung;
            Reason = reason;
        }

        public Pose Target { get; }

        public RescueRung Rung { get; }

        public ContainmentReason Reason { get; }
    }
}
