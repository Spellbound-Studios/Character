// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Character.Rig {
    public static class SocketResolver {
        public static SocketTable Resolve(Transform root, Animator animator, RigContract contract) {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            var resolved = new List<ResolvedSocket>();
            var defaulted = new List<string>();
            var missing = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var socket in contract.Sockets) {
                if (socket == null || string.IsNullOrEmpty(socket.Id) || !seen.Add(socket.Id))
                    continue;

                var parent = RigBones.Resolve(root, animator, socket.ParentBone);

                if (parent == null) {
                    missing.Add(socket.Id);

                    continue;
                }

                var node = DirectChild(parent, socket.NodeName);

                if (node != null) {
                    resolved.Add(new ResolvedSocket(socket.Id, node));

                    continue;
                }

                resolved.Add(new ResolvedSocket(socket.Id, parent, socket.DefaultLocalPose));
                defaulted.Add(socket.Id);
            }

            return new SocketTable(resolved, defaulted, missing);
        }

        private static Transform DirectChild(Transform parent, string name) {
            for (var i = 0; i < parent.childCount; i++) {
                var child = parent.GetChild(i);

                if (string.Equals(child.name, name, StringComparison.Ordinal))
                    return child;
            }

            return null;
        }
    }
}
