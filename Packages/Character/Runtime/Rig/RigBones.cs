// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Character.Rig {
    public static class RigBones {
        public static Transform Resolve(Transform root, Animator animator, RigBoneReference reference) {
            if (reference.UseHumanBone && animator != null && animator.isHuman) {
                var human = animator.GetBoneTransform(reference.HumanBone);

                if (human != null)
                    return human;
            }

            if (root == null || string.IsNullOrEmpty(reference.BonePath))
                return null;

            return root.Find(reference.BonePath);
        }

        public static Transform FindDescendant(Transform root, string name) {
            if (root == null || name == null)
                return null;

            for (var i = 0; i < root.childCount; i++) {
                var child = root.GetChild(i);

                if (string.Equals(child.name, name, StringComparison.Ordinal))
                    return child;

                var found = FindDescendant(child, name);

                if (found != null)
                    return found;
            }

            return null;
        }

        public static string PathOf(Transform root, Transform descendant) {
            if (root == null || descendant == null)
                return null;

            var names = new List<string>();

            for (var current = descendant; current != root; current = current.parent) {
                if (current == null)
                    return null;

                names.Add(current.name);
            }

            names.Reverse();

            return string.Join("/", names);
        }
    }
}
