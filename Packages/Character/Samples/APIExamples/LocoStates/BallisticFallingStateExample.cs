// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// No-control airborne variant: ignores input entirely, so the capsule flies off in the exact direction it
    /// launched and only gravity bends its path. Swapping between this and <see cref="FallingStateExample"/> at
    /// runtime shows how a single asset swap changes feel without touching the controller.
    /// </summary>
    [CreateAssetMenu(fileName = "BallisticFallingStateExample",
        menuName = "Spellbound/StateMachine/BallisticFallingStateExample")]
    public class BallisticFallingStateExample : FallingStateExample {
        protected override void HandleAirControl() { }
    }
}
