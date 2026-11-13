// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Linq;
using NUnit.Framework;
using Spellbound.Character.Rig;
using Spellbound.Character.Tests;
using Spellbound.Core.Authoring;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class RigContractValidationTests {
        private RigContract _contract;
        private GameObject _figure;
        private RagdollProfile _profile;

        [SetUp]
        public void CreateContract() {
            _contract = ScriptableObject.CreateInstance<RigContract>();
            _contract.name = "Contract";
        }

        [TearDown]
        public void DestroyContract() {
            Object.DestroyImmediate(_contract);

            if (_figure != null)
                Object.DestroyImmediate(_figure);

            if (_profile != null)
                Object.DestroyImmediate(_profile);
        }

        [Test]
        public void Validate_ThrowsOnNullReport() {
            Assert.Throws<ArgumentNullException>(() => _contract.Validate(null));
        }

        [Test]
        public void Validate_PlayableFigure_ReportsNothing() {
            _figure = new GameObject("Figure");
            _figure.AddComponent<PlayableTestCharacter>();
            _contract.ReferenceFigure = _figure;

            var report = Validate();

            Assert.IsTrue(report.IsEmpty, report.ToText());
        }

        [Test]
        public void Validate_CrateFigure_ReportsNotPlayable() {
            _figure = new GameObject("Crate");
            _contract.ReferenceFigure = _figure;

            var finding = SingleFinding("character.reference-figure-not-playable", FindingSeverity.Error);

            StringAssert.Contains("Crate", finding.Message);
            StringAssert.Contains("ReferenceFigure", finding.Location);
        }

        [Test]
        public void Validate_EmptyFigureAndBadHeightRange_StillReportsTheRange() {
            _contract.ReferenceFigure = null;
            _contract.MinimumHeightMetres = 4f;
            _contract.MaximumHeightMetres = 2f;

            var report = Validate();

            Assert.IsTrue(report.Findings.Any(finding => finding.Code == "character.height-range-invalid"),
                report.ToText());
            Assert.IsFalse(report.Findings.Any(finding => finding.Code == "character.reference-figure-not-playable"),
                report.ToText());
        }

        [Test]
        public void Validate_SocketWithEmptyId_ReportsSocketIdEmpty() {
            _contract.EditableSockets.Add(new RigSocket { Id = "", ParentBone = RigBoneReference.Path("Hips") });

            var finding = SingleFinding("character.socket-id-empty", FindingSeverity.Error);

            StringAssert.Contains("Sockets[0]", finding.Location);
        }

        [Test]
        public void Validate_SocketsWithTheSameId_ReportsSocketIdDuplicate() {
            _contract.EditableSockets.Add(new RigSocket { Id = "hand", ParentBone = RigBoneReference.Path("Hips") });
            _contract.EditableSockets.Add(new RigSocket { Id = "hand", ParentBone = RigBoneReference.Path("Hips") });

            var finding = SingleFinding("character.socket-id-duplicate", FindingSeverity.Error);

            StringAssert.Contains("hand", finding.Message);
            StringAssert.Contains("Sockets[1]", finding.Location);
        }

        [Test]
        public void Validate_SocketWithNoParentBone_ReportsSocketParentBoneEmpty() {
            _contract.EditableSockets.Add(new RigSocket { Id = "hand", ParentBone = RigBoneReference.Path("") });

            var finding = SingleFinding("character.socket-parent-bone-empty", FindingSeverity.Error);

            StringAssert.Contains("hand", finding.Message);
            StringAssert.Contains("Sockets[0].ParentBone", finding.Location);
        }

        [Test]
        public void Validate_GripWithEmptyId_ReportsGripIdEmpty() {
            AddSocket("hand");
            _contract.EditableGrips.Add(new RigGrip { Id = "", SocketId = "hand" });

            var finding = SingleFinding("character.grip-id-empty", FindingSeverity.Error);

            StringAssert.Contains("Grips[0]", finding.Location);
        }

        [Test]
        public void Validate_GripsWithTheSameId_ReportsGripIdDuplicate() {
            AddSocket("hand");
            _contract.EditableGrips.Add(new RigGrip { Id = "handle", SocketId = "hand" });
            _contract.EditableGrips.Add(new RigGrip { Id = "handle", SocketId = "hand" });

            var finding = SingleFinding("character.grip-id-duplicate", FindingSeverity.Error);

            StringAssert.Contains("handle", finding.Message);
            StringAssert.Contains("Grips[1]", finding.Location);
        }

        [Test]
        public void Validate_GripToAnUnknownSocket_ReportsGripSocketUnknown() {
            AddSocket("hand");
            _contract.EditableGrips.Add(new RigGrip { Id = "handle", SocketId = "back" });

            var finding = SingleFinding("character.grip-socket-unknown", FindingSeverity.Error);

            StringAssert.Contains("back", finding.Message);
            StringAssert.Contains("Grips[0].SocketId", finding.Location);
        }

        [Test]
        public void Validate_PieceWithEmptyId_ReportsPieceIdEmpty() {
            _contract.EditablePieces.Add(new RigPieceSlot { Id = "" });

            var finding = SingleFinding("character.piece-id-empty", FindingSeverity.Error);

            StringAssert.Contains("Pieces[0]", finding.Location);
        }

        [Test]
        public void Validate_PiecesWithTheSameId_ReportsPieceIdDuplicate() {
            _contract.EditablePieces.Add(new RigPieceSlot { Id = "head" });
            _contract.EditablePieces.Add(new RigPieceSlot { Id = "head" });

            var finding = SingleFinding("character.piece-id-duplicate", FindingSeverity.Error);

            StringAssert.Contains("head", finding.Message);
            StringAssert.Contains("Pieces[1]", finding.Location);
        }

        [Test]
        public void Validate_HipsWithNoBone_ReportsHipsBoneEmpty() {
            _contract.Hips.Bone = RigBoneReference.Path("");

            var finding = SingleFinding("character.hips-bone-empty", FindingSeverity.Error);

            StringAssert.Contains("Hips.Bone", finding.Location);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Validate_HipsHeightNotAPositiveNumber_ReportsHipsHeightNotPositive(float height) {
            _contract.Hips.HeightMetres = height;

            var finding = SingleFinding("character.hips-height-not-positive", FindingSeverity.Error);

            StringAssert.Contains("Hips.HeightMetres", finding.Location);
        }

        [Test]
        public void Validate_ZeroFacingAxis_ReportsFacingAxisNotADirection() {
            _contract.Hips.FacingAxis = Vector3.zero;

            var finding = SingleFinding("character.facing-axis-not-a-direction", FindingSeverity.Error);

            StringAssert.Contains("Hips.FacingAxis", finding.Location);
        }

        [Test]
        public void Validate_FacingAxisNotFinite_ReportsFacingAxisNotADirection() {
            _contract.Hips.FacingAxis = new Vector3(float.NaN, 0f, 1f);

            SingleFinding("character.facing-axis-not-a-direction", FindingSeverity.Error);
        }

        [Test]
        public void Validate_MinimumAboveMaximum_ReportsHeightRangeInvalid() {
            _contract.MinimumHeightMetres = 2.5f;
            _contract.MaximumHeightMetres = 1.5f;

            var finding = SingleFinding("character.height-range-invalid", FindingSeverity.Error);

            StringAssert.Contains("2.5", finding.Message);
            StringAssert.Contains("1.5", finding.Message);
        }

        [TestCase(0f, 3f)]
        [TestCase(-1f, 3f)]
        [TestCase(float.NaN, 3f)]
        [TestCase(0.5f, float.PositiveInfinity)]
        [TestCase(0.5f, 0f)]
        public void Validate_HeightBoundNotAPositiveNumber_ReportsHeightRangeInvalid(float minimum, float maximum) {
            _contract.MinimumHeightMetres = minimum;
            _contract.MaximumHeightMetres = maximum;

            var report = Validate();

            Assert.IsTrue(report.Findings.All(finding => finding.Code == "character.height-range-invalid"),
                report.ToText());
            Assert.IsTrue(report.HasErrors, report.ToText());
        }

        [Test]
        public void Validate_HumanoidWithNoRequiredBones_WarnsRequiredBonesEmpty() {
            _contract.Kind = RigKind.Humanoid;
            _contract.EditableRequiredHumanBones.Clear();

            var finding = SingleFinding("character.required-bones-empty", FindingSeverity.Warning);

            StringAssert.Contains("RequiredHumanBones", finding.Location);
        }

        [Test]
        public void Validate_GenericWithNoRequiredBonePaths_WarnsRequiredBonesEmpty() {
            _contract.Kind = RigKind.Generic;

            var finding = SingleFinding("character.required-bones-empty", FindingSeverity.Warning);

            StringAssert.Contains("RequiredBonePaths", finding.Location);
        }

        [Test]
        public void Validate_GenericWithRequiredBonePaths_ReportsNothing() {
            _contract.Kind = RigKind.Generic;
            _contract.EditableRequiredBonePaths.Add("Root/Hips");

            var report = Validate();

            Assert.IsTrue(report.IsEmpty, report.ToText());
        }

        [Test]
        public void Validate_InvalidRagdollProfile_ReportsRagdollProfileInvalidWithItsError() {
            _profile = ScriptableObject.CreateInstance<RagdollProfile>();
            _profile.name = "Profile";
            _contract.RagdollProfile = _profile;
            _profile.Validate(out var error);

            var finding = SingleFinding("character.ragdoll-profile-invalid", FindingSeverity.Error);

            StringAssert.Contains(error, finding.Message);
            StringAssert.Contains("RagdollProfile", finding.Location);
        }

        [Test]
        public void Validate_ValidRagdollProfile_ReportsNothing() {
            _profile = ScriptableObject.CreateInstance<RagdollProfile>();
            _profile.EditableBones.Add(new RagdollBoneDefinition { Name = "Hips", BonePath = "Hips" });
            _contract.RagdollProfile = _profile;

            var report = Validate();

            Assert.IsTrue(report.IsEmpty, report.ToText());
        }

        [Test]
        public void Validate_WellFormedSocketsGripsAndPieces_ReportNothing() {
            AddSocket("hand");
            AddSocket("back");
            _contract.EditableGrips.Add(new RigGrip { Id = "handle", SocketId = "hand" });
            _contract.EditableGrips.Add(new RigGrip { Id = "strap", SocketId = "back" });
            _contract.EditablePieces.Add(new RigPieceSlot { Id = "head" });
            _contract.EditablePieces.Add(new RigPieceSlot { Id = "body" });

            var report = Validate();

            Assert.IsTrue(report.IsEmpty, report.ToText());
        }

        private void AddSocket(string id) {
            _contract.EditableSockets.Add(new RigSocket { Id = id, ParentBone = RigBoneReference.Path("Hips") });
        }

        private ValidationReport Validate() {
            var report = new ValidationReport(_contract.name);
            _contract.Validate(report);

            return report;
        }

        private ValidationFinding SingleFinding(string code, FindingSeverity severity) {
            var report = Validate();

            Assert.AreEqual(1, report.Findings.Count, report.ToText());

            var finding = report.Findings[0];

            Assert.AreEqual(code, finding.Code, report.ToText());
            Assert.AreEqual(severity, finding.Severity, report.ToText());
            Assert.IsNotNull(finding.Location, report.ToText());

            return finding;
        }
    }
}
