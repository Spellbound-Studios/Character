// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Character.Rig {
    public sealed class RebindMap {
        private readonly Dictionary<string, Transform> _bones;
        private readonly string[] _missing;

        public RebindMap(IReadOnlyDictionary<string, Transform> bones, IReadOnlyList<string> missing) {
            if (bones == null)
                throw new ArgumentNullException(nameof(bones));

            if (missing == null)
                throw new ArgumentNullException(nameof(missing));

            _bones = new Dictionary<string, Transform>(bones.Count, StringComparer.Ordinal);

            foreach (var pair in bones)
                _bones[pair.Key] = pair.Value;

            _missing = new string[missing.Count];

            for (var i = 0; i < missing.Count; i++)
                _missing[i] = missing[i];
        }

        public IReadOnlyDictionary<string, Transform> Bones => _bones;

        public IReadOnlyList<string> Missing => _missing;

        public bool IsComplete => _missing.Length == 0;
    }
}
