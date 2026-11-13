// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.Authoring;
using Spellbound.Core.Hashing;
using UnityEngine;

namespace Spellbound.Character.Rig {
    [CreateAssetMenu(fileName = "RigContract", menuName = "Spellbound/Character/Rig Contract")]
    public sealed class RigContract : ScriptableObject, IAuthoredDefinition {
        public uint ContentHash => StableHash.Fnv1A32(SerializedContent());

        public void Validate(ValidationReport report) { }

        private string SerializedContent() => nameof(RigContract);
    }
}
