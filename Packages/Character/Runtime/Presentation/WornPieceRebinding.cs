// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Character.Rig;
using UnityEngine;

namespace Spellbound.Character.Presentation {
    public static class WornPieceRebinding {
        public static RebindMap Rebind(GameObject wornPiece, Transform targetRoot) {
            if (wornPiece == null)
                throw new ArgumentNullException(nameof(wornPiece));

            if (targetRoot == null)
                throw new ArgumentNullException(nameof(targetRoot));

            var renderers = wornPiece.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var bones = new Dictionary<string, Transform>(StringComparer.Ordinal);
            var missing = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var renderer in renderers) {
                foreach (var bone in renderer.bones)
                    Match(bone, targetRoot, seen, bones, missing);

                Match(renderer.rootBone, targetRoot, seen, bones, missing);
            }

            if (missing.Count > 0)
                return new RebindMap(bones, missing);

            foreach (var renderer in renderers) {
                var sourceBones = renderer.bones;
                var targetBones = new Transform[sourceBones.Length];

                for (var i = 0; i < sourceBones.Length; i++)
                    targetBones[i] = TargetOf(sourceBones[i], bones);

                renderer.bones = targetBones;
                renderer.rootBone = TargetOf(renderer.rootBone, bones);
            }

            return new RebindMap(bones, missing);
        }

        private static void Match(
            Transform bone,
            Transform targetRoot,
            HashSet<string> seen,
            Dictionary<string, Transform> bones,
            List<string> missing) {
            if (bone == null || !seen.Add(bone.name))
                return;

            var target = string.Equals(targetRoot.name, bone.name, StringComparison.Ordinal)
                ? targetRoot
                : RigBones.FindDescendant(targetRoot, bone.name);

            if (target == null)
                missing.Add(bone.name);
            else
                bones[bone.name] = target;
        }

        private static Transform TargetOf(Transform bone, Dictionary<string, Transform> bones) =>
            bone == null ? null : bones[bone.name];
    }
}
