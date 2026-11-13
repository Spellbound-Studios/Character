// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Tests {
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class CapsuleHolder : MonoBehaviour {
        [SerializeField] private ResizableCapsuleCollider capsule = new();

        public ResizableCapsuleCollider Capsule => capsule;
    }
}
