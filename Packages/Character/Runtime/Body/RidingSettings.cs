// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Authoring;
using UnityEngine;

namespace Spellbound.Character.Body {
    [CreateAssetMenu(menuName = "Spellbound/Character/Riding Settings", fileName = "RidingSettings")]
    public sealed class RidingSettings : ScriptableObject, IAuthoredDefinition {
        public LayerMask rideableLayers = ~0;

        public void Validate(ValidationReport report) {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            if (rideableLayers.value == 0) {
                report.Warning("character.rideable-layers-empty",
                    "No layer is rideable, so no rider binds to anything.", nameof(rideableLayers),
                    "Set it to the layers of the moving things riders stand on.");
            }
        }
    }
}
