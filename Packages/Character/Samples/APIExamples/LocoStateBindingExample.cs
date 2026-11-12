// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// One locomotion slot-to-state binding for the sample config.
    /// </summary>
    [Serializable]
    public sealed class LocoStateBindingExample : IStateBinding<LocoStateTypes> {
        [SerializeField] private LocoStateTypes slot;
        [SerializeField] private BaseSoState variant;
        public LocoStateTypes Slot => slot;
        public BaseSoState Variant => variant;
    }
}
