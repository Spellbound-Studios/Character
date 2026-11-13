// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Character.Rig;
using UnityEditor;

namespace Spellbound.Character.Editor {
    public static class RigKindMapping {
        public static ModelImporterAnimationType ToAnimationType(RigKind kind) =>
                kind switch {
                    RigKind.Humanoid => ModelImporterAnimationType.Human,
                    RigKind.Generic => ModelImporterAnimationType.Generic,
                    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind,
                            "The rig kind has no animation type.")
                };

        public static bool TryFromAnimationType(ModelImporterAnimationType type, out RigKind kind) {
            switch (type) {
                case ModelImporterAnimationType.Human:
                    kind = RigKind.Humanoid;
                    return true;
                case ModelImporterAnimationType.Generic:
                    kind = RigKind.Generic;
                    return true;
                default:
                    kind = default;
                    return false;
            }
        }
    }
}
