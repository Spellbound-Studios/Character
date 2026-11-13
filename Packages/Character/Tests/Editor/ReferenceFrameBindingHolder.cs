// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public sealed class ReferenceFrameBindingHolder : ScriptableObject {
        [SerializeField] private ReferenceFrameBinding binding = new();

        public ReferenceFrameBinding Binding => binding;
    }
}
