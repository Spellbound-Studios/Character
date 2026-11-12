// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Controller {
    /// <summary>
    /// Maps a state-machine slot to the state SO that fills it.
    /// </summary>
    public interface IStateBinding<out TStateEnum> {
        TStateEnum Slot { get; }
        BaseSoState Variant { get; }
    }
}
