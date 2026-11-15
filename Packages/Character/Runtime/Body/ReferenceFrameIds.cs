// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Text;
using Spellbound.Core.Hashing;
using Spellbound.Core.ReferenceFrames;
using UnityEngine;

namespace Spellbound.Character.Body {
    public static class ReferenceFrameIds {
        public static ReferenceFrameId FromHierarchyPath(Transform transform) {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            var path = new StringBuilder(transform.name);

            for (var parent = transform.parent; parent != null; parent = parent.parent)
                path.Insert(0, '/').Insert(0, parent.name);

            var hash = StableHash.Fnv1A32(path.ToString());

            return new ReferenceFrameId(hash == 0u ? 1ul : hash);
        }
    }
}
