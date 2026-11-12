// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Sample reusable config for the locomotion state machine.
    /// </summary>
    [CreateAssetMenu(fileName = "LocoStateConfigExample", menuName = "Spellbound/StateMachine/Loco Config Example")]
    public sealed class LocoStateConfigExample : ScriptableObject, IStateMachineConfig<LocoStateTypes> {
        [SerializeField] private List<LocoStateBindingExample> bindings = new();
        [SerializeField] private LocoStateTypes initialState = LocoStateTypes.Falling;
        public IReadOnlyList<IStateBinding<LocoStateTypes>> Bindings => bindings;
        public LocoStateTypes InitialState => initialState;
    }
}
