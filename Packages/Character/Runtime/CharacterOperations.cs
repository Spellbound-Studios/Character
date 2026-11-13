// Copyright (c) Spellbound Studios. All rights reserved.

using System.Globalization;
using System.Text;
using Spellbound.Character.Rig;
using Spellbound.Core.Operations;
using UnityEngine;

namespace Spellbound.Character {
    public static class CharacterOperations {
        [Operation("sockets",
            "Resolves a character's sockets from a rig contract and lists each one's node, default or missing parent.",
            OperationEffect.Reads, Audience = OperationAudiences.Anyone)]
        public static OperationResult Sockets(OperationContext context,
            [Argument("The character's root in the open scene.")] GameObject root,
            [Argument("The rig contract whose sockets are resolved.")] RigContract contract) {
            if (root == null || contract == null) {
                return OperationResult.Invalid("--root and --contract are both required"
                                               + "\nRun: core help --group character");
            }

            var transform = root.transform;
            var table = SocketResolver.Resolve(transform, root.GetComponent<Animator>(), contract);
            var rows = new OperationTable("id", "state", "path");

            foreach (var id in table.Ids) {
                table.TryGet(id, out var socket);
                var state = socket.HasNode ? "node" : "default";
                var path = RigBones.PathOf(transform, socket.HasNode ? socket.Node : socket.Parent) ?? string.Empty;
                rows.AddRow(id, state, path);
            }

            foreach (var id in table.Missing)
                rows.AddRow(id, "missing", string.Empty);

            var count = table.Count + table.Missing.Count;
            var text = new StringBuilder();
            text.Append(Number(count)).Append(count == 1 ? " socket, " : " sockets, ")
                .Append(Number(table.Defaulted.Count)).Append(" defaulted, ")
                .Append(Number(table.Missing.Count)).Append(" missing");

            if (count > 0)
                text.Append('\n').Append(rows.ToText());

            return OperationResult.Success(text.ToString());
        }

        [Operation("ragdoll",
            "Runs the ragdoll builder in memory on a model and answers what it would build and what is missing.",
            OperationEffect.Reads, Audience = OperationAudiences.Anyone)]
        public static OperationResult Ragdoll(OperationContext context,
            [Argument("The model's root in the open scene.")] GameObject root,
            [Argument("The ragdoll profile the builder reads.")] RagdollProfile profile) {
            if (root == null)
                return OperationResult.Invalid("--root is required\nRun: core help --group character");

            var dryRun = RagdollBuilder.DryRun(root.transform, root.GetComponent<Animator>(), profile);

            return dryRun.WouldBuild
                    ? OperationResult.Success(dryRun.ToText())
                    : OperationResult.Failed(dryRun.Error);
        }

        private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
