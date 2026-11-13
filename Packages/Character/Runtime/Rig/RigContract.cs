// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Authoring;
using Spellbound.Core.Hashing;
using UnityEngine;

namespace Spellbound.Character.Rig {
    [CreateAssetMenu(fileName = "RigContract", menuName = "Spellbound/Character/Rig Contract")]
    public sealed class RigContract : ScriptableObject, IAuthoredDefinition {
        [SerializeField] private RigKind rigKind = RigKind.Humanoid;

        [SerializeField] private List<HumanBodyBones> requiredHumanBones = new() {
            HumanBodyBones.Hips,
            HumanBodyBones.Spine,
            HumanBodyBones.Head,
            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.RightFoot,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.RightHand
        };

        [SerializeField] private List<string> requiredBonePaths = new();
        [SerializeField] private List<RigSocket> sockets = new();
        [SerializeField] private List<RigGrip> grips = new();
        [SerializeField] private List<RigPieceSlot> pieces = new();
        [SerializeField] private RigHips hips = new();
        [SerializeField] private RagdollProfile ragdollProfile;
        [SerializeField] private float minimumHeightMetres = 0.5f;
        [SerializeField] private float maximumHeightMetres = 3f;
        [SerializeField] private GameObject referenceFigure;
        [SerializeField] private List<AnimationClip> poses = new();

        public RigKind Kind {
            get => rigKind;
            set => rigKind = value;
        }

        public IReadOnlyList<HumanBodyBones> RequiredHumanBones => requiredHumanBones;

        public List<HumanBodyBones> EditableRequiredHumanBones => requiredHumanBones;

        public IReadOnlyList<string> RequiredBonePaths => requiredBonePaths;

        public List<string> EditableRequiredBonePaths => requiredBonePaths;

        public IReadOnlyList<RigSocket> Sockets => sockets;

        public List<RigSocket> EditableSockets => sockets;

        public IReadOnlyList<RigGrip> Grips => grips;

        public List<RigGrip> EditableGrips => grips;

        public IReadOnlyList<RigPieceSlot> Pieces => pieces;

        public List<RigPieceSlot> EditablePieces => pieces;

        public RigHips Hips {
            get => hips ??= new RigHips();
            set => hips = value ?? new RigHips();
        }

        public RagdollProfile RagdollProfile {
            get => ragdollProfile;
            set => ragdollProfile = value;
        }

        public float MinimumHeightMetres {
            get => minimumHeightMetres;
            set => minimumHeightMetres = value;
        }

        public float MaximumHeightMetres {
            get => maximumHeightMetres;
            set => maximumHeightMetres = value;
        }

        public GameObject ReferenceFigure {
            get => referenceFigure;
            set => referenceFigure = value;
        }

        public IReadOnlyList<AnimationClip> Poses => poses;

        public List<AnimationClip> EditablePoses => poses;

        public uint ContentHash => StableHash.Fnv1A32(SerializedContent());

        public RigSocket FindSocket(string id) {
            if (id == null)
                return null;

            foreach (var socket in sockets) {
                if (socket != null && string.Equals(socket.Id, id, StringComparison.Ordinal))
                    return socket;
            }

            return null;
        }

        public void Validate(ValidationReport report) { }

        private string SerializedContent() => nameof(RigContract);
    }
}
