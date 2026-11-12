// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Base for the sample's action states — caches the controller context so concrete action states (ready,
    /// aiming, …) can read input and drive the action state machine. Mirrors <see cref="BaseLocomotionStateExample"/>.
    /// </summary>
    public abstract class BaseActionStateExample : BaseSoState {
        protected new PlayerControllerExample Ctx;

        protected override void OnCtxInitialized() => Ctx = base.Ctx as PlayerControllerExample;
    }
}
