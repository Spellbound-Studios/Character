// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [Serializable]
    public class LayerData {
        [field: SerializeField] public LayerMask GroundLayer { get; private set; } = 1 << 6;
    }
}