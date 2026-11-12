// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;

namespace Spellbound.Controller {
    /// <summary>
    /// A reusable definition of a state machine's per-slot default states and its starting slot.
    /// </summary>
    public interface IStateMachineConfig<TStateEnum> {
        IReadOnlyList<IStateBinding<TStateEnum>> Bindings { get; }
        TStateEnum InitialState { get; }
    }
}
