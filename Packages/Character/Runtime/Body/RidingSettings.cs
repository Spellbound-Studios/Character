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
        }
    }
}
