// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Sample reusable config for the action state machine.
    /// </summary>
    [CreateAssetMenu(fileName = "ActionStateConfigExample", menuName = "Spellbound/StateMachine/Action Config Example")]
    public sealed class ActionStateConfigExample : ScriptableObject, IStateMachineConfig<ActionStateTypes> {
        [SerializeField] private List<ActionStateBindingExample> bindings = new();
        [SerializeField] private ActionStateTypes initialState = ActionStateTypes.Ready;
        public IReadOnlyList<IStateBinding<ActionStateTypes>> Bindings => bindings;
        public ActionStateTypes InitialState => initialState;
    }
}
