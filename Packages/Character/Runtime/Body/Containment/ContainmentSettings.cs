// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Authoring;
using UnityEngine;

namespace Spellbound.Character.Body {
    [CreateAssetMenu(menuName = "Spellbound/Character/Containment Settings", fileName = "ContainmentSettings")]
    public sealed class ContainmentSettings : ScriptableObject, IAuthoredDefinition {
        public bool hasFloor = true;
        public float floorHeight = -100f;
        public int embeddedChecksBeforeRescue = 5;
        public float openSearchStepMetres = 0.5f;
        public float maxOpenSearchMetres = 20f;
        public float sustainedFallSpeed = 25f;
        public float sustainedFallSeconds = 8f;
        public Vector3 fallbackAnchor;
        public int checkStride = 5;
        public int rescuePriority = 1000;
        public float supportHoldTimeoutSeconds = 10f;
        public float supportReachMetres = 1f;
        public LayerMask supportLayers = ~0;

        public void Validate(ValidationReport report) {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            if (checkStride < 1) {
                report.Error("character.containment-stride-under-one",
                    "The check stride is under one, so no body is ever checked.", nameof(checkStride),
                    "Set the stride to one or more fixed steps.");
            }

            if (!(openSearchStepMetres > 0f)) {
                report.Error("character.containment-search-step-not-positive",
                    "The open search step is not above zero, so the search never moves.",
                    nameof(openSearchStepMetres), "Set a step above zero metres.");
            }

            if (!(maxOpenSearchMetres > 0f)) {
                report.Error("character.containment-search-range-not-positive",
                    "The open search range is not above zero, so no open space above is ever found.",
                    nameof(maxOpenSearchMetres), "Set a range above zero metres.");
            }

            if (embeddedChecksBeforeRescue < 1) {
                report.Error("character.containment-embedded-checks-under-one",
                    "The embedded checks before a rescue are under one.", nameof(embeddedChecksBeforeRescue),
                    "Set one or more checks.");
            }

            if (!(sustainedFallSpeed >= 0f) || !(sustainedFallSeconds >= 0f)) {
                report.Error("character.containment-fall-limits-negative",
                    "The sustained fall speed or time is negative.",
                    sustainedFallSpeed >= 0f ? nameof(sustainedFallSeconds) : nameof(sustainedFallSpeed),
                    "Set both the speed and the seconds to zero or more.");
            }

            if (!(supportHoldTimeoutSeconds >= 0f)) {
                report.Error("character.containment-hold-timeout-negative",
                    "The support hold timeout is negative.", nameof(supportHoldTimeoutSeconds),
                    "Set the timeout to zero or more seconds.");
            }

            if (!(supportReachMetres >= 0f)) {
                report.Error("character.containment-support-reach-negative",
                    "The support reach under the body is negative, so no position is ever remembered as supported.",
                    nameof(supportReachMetres), "Set the reach to zero or more metres.");
            }

            if (rescuePriority < 1) {
                report.Error("character.containment-rescue-priority-under-one",
                    "The rescue priority is under one, so a rescue claim never takes over a driver.",
                    nameof(rescuePriority), "Set a priority above every motion driver's.");
            }
        }
    }
}
