// Copyright 2026 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// One action slot-to-state binding for the sample config.
    /// </summary>
    [Serializable]
    public sealed class ActionStateBindingExample : IStateBinding<ActionStateTypes> {
        [SerializeField] private ActionStateTypes slot;
        [SerializeField] private BaseSoState variant;
        public ActionStateTypes Slot => slot;
        public BaseSoState Variant => variant;
    }
}
