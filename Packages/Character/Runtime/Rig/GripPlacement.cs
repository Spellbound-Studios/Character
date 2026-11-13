// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Authoring;
using UnityEngine;

namespace Spellbound.Character.Rig {
    public static class GripPlacement {
        public static Pose HoldPose(
            Transform gripNode,
            RigContract contract,
            string socketId,
            ValidationReport report) {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (report == null)
                throw new ArgumentNullException(nameof(report));

            if (gripNode == null) {
                report.Error("character.grip-node-missing",
                    $"No grip node was given to place in socket '{socketId}'.", nameof(gripNode));

                return Pose.identity;
            }

            if (contract.ReferenceFigure == null) {
                report.Warning("character.no-reference-figure", "The contract names no reference figure.",
                    nameof(RigContract.ReferenceFigure));

                return Pose.identity;
            }

            var socket = contract.FindSocket(socketId);

            if (socket == null || string.IsNullOrEmpty(socket.Id)) {
                report.Error("character.unknown-socket",
                    $"Socket '{socketId}' is not in the contract; its sockets are {SocketIds(contract)}.",
                    nameof(RigContract.Sockets));

                return Pose.identity;
            }

            var figureRoot = contract.ReferenceFigure.transform;
            var table = SocketResolver.Resolve(figureRoot, figureRoot.GetComponent<Animator>(), contract);

            if (!table.TryGet(socket.Id, out var resolved)) {
                report.Error("character.socket-parent-missing",
                    $"Socket '{socket.Id}' names the parent bone {BoneName(socket.ParentBone)}, which the "
                    + $"reference figure '{contract.ReferenceFigure.name}' does not have.",
                    nameof(RigContract.Sockets));

                return Pose.identity;
            }

            var socketPose = resolved.PoseIn(figureRoot);
            var propRoot = gripNode.root;
            var inverseRootRotation = Quaternion.Inverse(propRoot.rotation);
            var gripPosition = inverseRootRotation * (gripNode.position - propRoot.position);
            var gripRotation = inverseRootRotation * gripNode.rotation;
            var rotation = socketPose.rotation * Quaternion.Inverse(gripRotation);

            return new Pose(socketPose.position - rotation * gripPosition, rotation);
        }

        private static string SocketIds(RigContract contract) {
            var ids = new List<string>();

            foreach (var socket in contract.Sockets) {
                if (socket != null && !string.IsNullOrEmpty(socket.Id))
                    ids.Add($"'{socket.Id}'");
            }

            return ids.Count == 0 ? "none" : string.Join(", ", ids);
        }

        private static string BoneName(RigBoneReference bone) =>
                bone.UseHumanBone ? $"human bone {bone.HumanBone}" : $"'{bone.BonePath}'";
    }
}
