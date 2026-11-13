// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Spellbound.Character.Body;
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

        public void Validate(ValidationReport report) {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            ValidateReferenceFigure(report);
            ValidateRequiredBones(report);
            ValidateSockets(report);
            ValidateGrips(report);
            ValidatePieces(report);
            ValidateHips(report);
            ValidateHeightRange(report);
            ValidateRagdollProfile(report);
        }

        private void ValidateReferenceFigure(ValidationReport report) {
            if (referenceFigure == null || PlayableCharacters.IsPlayable(referenceFigure))
                return;

            report.Error("character.reference-figure-not-playable",
                $"ReferenceFigure '{referenceFigure.name}' is not a playable character: its root carries no "
                + $"component implementing {nameof(IPlayableCharacter)}.", nameof(ReferenceFigure));
        }

        private void ValidateRequiredBones(ValidationReport report) {
            if (rigKind == RigKind.Humanoid && requiredHumanBones.Count == 0) {
                report.Warning("character.required-bones-empty",
                    $"RequiredHumanBones is empty for a {rigKind} rig, so no human bone is checked.",
                    nameof(RequiredHumanBones));
            }

            if (rigKind == RigKind.Generic && requiredBonePaths.Count == 0) {
                report.Warning("character.required-bones-empty",
                    $"RequiredBonePaths is empty for a {rigKind} rig, so no bone path is checked.",
                    nameof(RequiredBonePaths));
            }
        }

        private void ValidateSockets(ValidationReport report) {
            var firstIndexById = new Dictionary<string, int>(StringComparer.Ordinal);

            for (var i = 0; i < sockets.Count; i++) {
                var socket = sockets[i];
                var location = $"{nameof(Sockets)}[{i}]";

                if (socket == null || string.IsNullOrEmpty(socket.Id)) {
                    report.Error("character.socket-id-empty", $"{location}.Id is empty ('').", location + ".Id");
                }
                else if (firstIndexById.TryGetValue(socket.Id, out var first)) {
                    report.Error("character.socket-id-duplicate",
                        $"{location}.Id '{socket.Id}' repeats the id of {nameof(Sockets)}[{first}].",
                        location + ".Id");
                }
                else {
                    firstIndexById.Add(socket.Id, i);
                }

                if (socket != null && socket.ParentBone.IsEmpty) {
                    report.Error("character.socket-parent-bone-empty",
                        $"{location}.ParentBone of socket '{socket.Id}' is empty: it names neither a human bone "
                        + "nor a bone path.", location + ".ParentBone");
                }
            }
        }

        private void ValidateGrips(ValidationReport report) {
            var firstIndexById = new Dictionary<string, int>(StringComparer.Ordinal);

            for (var i = 0; i < grips.Count; i++) {
                var grip = grips[i];
                var location = $"{nameof(Grips)}[{i}]";

                if (grip == null || string.IsNullOrEmpty(grip.Id)) {
                    report.Error("character.grip-id-empty", $"{location}.Id is empty ('').", location + ".Id");
                }
                else if (firstIndexById.TryGetValue(grip.Id, out var first)) {
                    report.Error("character.grip-id-duplicate",
                        $"{location}.Id '{grip.Id}' repeats the id of {nameof(Grips)}[{first}].", location + ".Id");
                }
                else {
                    firstIndexById.Add(grip.Id, i);
                }

                if (grip != null && (string.IsNullOrEmpty(grip.SocketId) || FindSocket(grip.SocketId) == null)) {
                    report.Error("character.grip-socket-unknown",
                        $"{location}.SocketId '{grip.SocketId}' of grip '{grip.Id}' matches no socket of the "
                        + "contract.", location + ".SocketId");
                }
            }
        }

        private void ValidatePieces(ValidationReport report) {
            var firstIndexById = new Dictionary<string, int>(StringComparer.Ordinal);

            for (var i = 0; i < pieces.Count; i++) {
                var piece = pieces[i];
                var location = $"{nameof(Pieces)}[{i}]";

                if (piece == null || string.IsNullOrEmpty(piece.Id)) {
                    report.Error("character.piece-id-empty", $"{location}.Id is empty ('').", location + ".Id");
                }
                else if (firstIndexById.TryGetValue(piece.Id, out var first)) {
                    report.Error("character.piece-id-duplicate",
                        $"{location}.Id '{piece.Id}' repeats the id of {nameof(Pieces)}[{first}].", location + ".Id");
                }
                else {
                    firstIndexById.Add(piece.Id, i);
                }
            }
        }

        private void ValidateHips(ValidationReport report) {
            var contractHips = Hips;

            if (contractHips.Bone.IsEmpty) {
                report.Error("character.hips-bone-empty",
                    "Hips.Bone is empty: it names neither a human bone nor a bone path.", "Hips.Bone");
            }

            if (!IsPositive(contractHips.HeightMetres)) {
                report.Error("character.hips-height-not-positive",
                    $"Hips.HeightMetres is {Format(contractHips.HeightMetres)}, which is not a finite number "
                    + "above zero.", "Hips.HeightMetres");
            }

            var axis = contractHips.FacingAxis;

            if (!IsFinite(axis) || axis.magnitude <= Vector3.kEpsilon) {
                report.Error("character.facing-axis-not-a-direction",
                    $"Hips.FacingAxis is ({Format(axis.x)}, {Format(axis.y)}, {Format(axis.z)}), which is not "
                    + "a direction.", "Hips.FacingAxis");
            }
        }

        private void ValidateHeightRange(ValidationReport report) {
            var minimumValid = IsPositive(minimumHeightMetres);
            var maximumValid = IsPositive(maximumHeightMetres);

            if (!minimumValid) {
                report.Error("character.height-range-invalid",
                    $"MinimumHeightMetres is {Format(minimumHeightMetres)}, which is not a finite number above "
                    + "zero.", nameof(MinimumHeightMetres));
            }

            if (!maximumValid) {
                report.Error("character.height-range-invalid",
                    $"MaximumHeightMetres is {Format(maximumHeightMetres)}, which is not a finite number above "
                    + "zero.", nameof(MaximumHeightMetres));
            }

            if (minimumValid && maximumValid && minimumHeightMetres > maximumHeightMetres) {
                report.Error("character.height-range-invalid",
                    $"MinimumHeightMetres {Format(minimumHeightMetres)} is above MaximumHeightMetres "
                    + $"{Format(maximumHeightMetres)}.", nameof(MinimumHeightMetres));
            }
        }

        private void ValidateRagdollProfile(ValidationReport report) {
            if (ragdollProfile == null || ragdollProfile.Validate(out var error))
                return;

            report.Error("character.ragdoll-profile-invalid", $"RagdollProfile '{ragdollProfile.name}': {error}",
                nameof(RagdollProfile));
        }

        private static bool IsPositive(float value) => float.IsFinite(value) && value > 0f;

        private static bool IsFinite(Vector3 value) =>
                float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);

        private static string Format(float value) => value.ToString("R", CultureInfo.InvariantCulture);

        private string SerializedContent() {
            var content = new StringBuilder(nameof(RigContract));

            AppendText(content, rigKind.ToString());

            AppendCount(content, requiredHumanBones);

            foreach (var bone in requiredHumanBones)
                AppendText(content, bone.ToString());

            AppendCount(content, requiredBonePaths);

            foreach (var bonePath in requiredBonePaths)
                AppendText(content, bonePath);

            AppendCount(content, sockets);

            foreach (var socket in sockets)
                AppendSocket(content, socket);

            AppendCount(content, grips);

            foreach (var grip in grips)
                AppendGrip(content, grip);

            AppendCount(content, pieces);

            foreach (var piece in pieces)
                AppendText(content, piece?.Id);

            var contractHips = Hips;
            AppendBone(content, contractHips.Bone);
            AppendNumber(content, contractHips.HeightMetres);
            AppendVector(content, contractHips.FacingAxis);

            AppendReference(content, ragdollProfile);
            AppendNumber(content, minimumHeightMetres);
            AppendNumber(content, maximumHeightMetres);
            AppendReference(content, referenceFigure);

            AppendCount(content, poses);

            foreach (var pose in poses)
                AppendReference(content, pose);

            return content.ToString();
        }

        private static void AppendSocket(StringBuilder content, RigSocket socket) {
            if (socket == null) {
                AppendText(content, null);
                return;
            }

            AppendText(content, socket.Id);
            AppendBone(content, socket.ParentBone);
            AppendText(content, socket.NodeNamePattern);
            AppendVector(content, socket.DefaultLocalPosition);
            AppendVector(content, socket.DefaultLocalEulerAngles);
        }

        private static void AppendGrip(StringBuilder content, RigGrip grip) {
            if (grip == null) {
                AppendText(content, null);
                return;
            }

            AppendText(content, grip.Id);
            AppendText(content, grip.NodeNamePattern);
            AppendText(content, grip.SocketId);
        }

        private static void AppendBone(StringBuilder content, RigBoneReference bone) {
            AppendText(content, bone.UseHumanBone ? "human" : "path");
            AppendText(content, bone.HumanBone.ToString());
            AppendText(content, bone.BonePath);
        }

        private static void AppendVector(StringBuilder content, Vector3 value) {
            AppendNumber(content, value.x);
            AppendNumber(content, value.y);
            AppendNumber(content, value.z);
        }

        private static void AppendNumber(StringBuilder content, float value) => AppendText(content, Format(value));

        private static void AppendReference(StringBuilder content, UnityEngine.Object reference) =>
                AppendText(content, reference == null ? string.Empty : reference.name);

        private static void AppendCount<T>(StringBuilder content, List<T> list) =>
                AppendText(content, list.Count.ToString(CultureInfo.InvariantCulture));

        private static void AppendText(StringBuilder content, string text) {
            var value = text ?? string.Empty;

            content.Append('|').Append(value.Length.ToString(CultureInfo.InvariantCulture)).Append(':').Append(value);
        }
    }
}
