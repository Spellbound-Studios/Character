// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using Spellbound.Character.Body;
using Spellbound.Character.Rig;
using Spellbound.Core.Authoring;
using UnityEngine;

namespace Spellbound.Character {
    public static class CharacterKit {
        public static class Body {
            public static bool IsPlayable(GameObject prefab) => PlayableCharacters.IsPlayable(prefab);
        }

        public static class Rig {
            public static bool BuildRagdoll(
                Transform rigRoot,
                Animator animator,
                RagdollProfile profile,
                List<RagdollBone> output,
                out string error) =>
                    RagdollBuilder.Build(rigRoot, animator, profile, output, out error);

            public static Pose HoldPose(
                Transform gripNode,
                RigContract contract,
                string socketId,
                ValidationReport report) =>
                    GripPlacement.HoldPose(gripNode, contract, socketId, report);

            public static SocketTable ResolveSockets(Transform modelRoot, RigContract contract) =>
                SocketResolver.Resolve(modelRoot, modelRoot.GetComponent<Animator>(), contract);

            public static RagdollDryRun DryRunRagdoll(Transform modelRoot, RagdollProfile profile) =>
                RagdollBuilder.DryRun(modelRoot, modelRoot.GetComponent<Animator>(), profile);
        }
    }
}
