// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Controller {
    /// <summary>
    /// Queries a state machine's current state without needing its concrete context or enum.
    /// </summary>
    public interface IStateMachine {
        BaseSoState CurrentState { get; }
        uint CurrentStateHash { get; }
        bool IsInState<TVariant>() where TVariant : BaseSoState;
        bool IsInState(uint stateHash);
    }
}
