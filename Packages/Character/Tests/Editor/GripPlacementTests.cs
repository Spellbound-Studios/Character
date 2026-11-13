// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Character.Rig;
using Spellbound.Core.Authoring;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class GripPlacementTests {
        private const float Tolerance = 1e-4f;

        private static readonly Vector3 HipsPosition = new(0f, 1f, 0f);
        private static readonly Quaternion HipsRotation = Quaternion.Euler(0f, 0f, 10f);
        private static readonly Vector3 HandPosition = new(0.3f, 0.2f, 0.1f);
        private static readonly Quaternion HandRotation = Quaternion.Euler(10f, 20f, 30f);
        private static readonly Vector3 BackPosition = new(0f, 0.1f, -0.2f);
        private static readonly Vector3 BackEulerAngles = new(0f, 180f, 0f);
        private static readonly Vector3 GripPosition = new(0.1f, 0.05f, -0.3f);
        private static readonly Quaternion GripRotation = Quaternion.Euler(0f, 90f, 15f);

        private Scene _previewScene;
        private Transform _figure;
        private Transform _propRoot;
        private Transform _centredGrip;
        private Transform _offsetGrip;
        private RigContract _contract;
        private ValidationReport _report;

        [SetUp]
        public void CreateFixtures() {
            _previewScene = EditorSceneManager.NewPreviewScene();

            var figure = new GameObject("Figure");
            SceneManager.MoveGameObjectToScene(figure, _previewScene);
            _figure = figure.transform;
            _figure.SetPositionAndRotation(new Vector3(2f, 0f, 3f), Quaternion.Euler(0f, 45f, 0f));

            var hips = AddChild(_figure, "Hips", HipsPosition, HipsRotation);
            AddChild(hips, "socket_Hand", HandPosition, HandRotation);

            var prop = new GameObject("Prop");
            SceneManager.MoveGameObjectToScene(prop, _previewScene);
            _propRoot = prop.transform;
            _propRoot.SetPositionAndRotation(new Vector3(-1f, 2f, 0f), Quaternion.Euler(0f, 30f, 0f));
            _centredGrip = AddChild(_propRoot, "grip_Centre", Vector3.zero, Quaternion.identity);
            _offsetGrip = AddChild(_propRoot, "grip_Hand", GripPosition, GripRotation);

            _contract = ScriptableObject.CreateInstance<RigContract>();
            _contract.EditableSockets.Add(new RigSocket { Id = "Hand", ParentBone = RigBoneReference.Path("Hips") });
            _contract.EditableSockets.Add(new RigSocket {
                Id = "Back",
                ParentBone = RigBoneReference.Path("Hips"),
                DefaultLocalPosition = BackPosition,
                DefaultLocalEulerAngles = BackEulerAngles
            });
            _contract.EditableSockets.Add(new RigSocket {
                Id = "Tail",
                ParentBone = RigBoneReference.Path("Hips/Tail")
            });
            _contract.ReferenceFigure = figure;

            _report = new ValidationReport("grip");
        }

        [TearDown]
        public void DestroyFixtures() {
            Object.DestroyImmediate(_contract);
            EditorSceneManager.ClosePreviewScene(_previewScene);
        }

        [Test]
        public void HoldPose_GripAtThePropOrigin_IsTheSocketPoseInFigureSpace() {
            var pose = GripPlacement.HoldPose(_centredGrip, _contract, "Hand", _report);

            AssertPose(HandPoseInFigure(), pose);
            Assert.IsTrue(_report.IsEmpty, _report.ToText());
        }

        [Test]
        public void HoldPose_OffsetGrip_ComposedWithTheGripLandsOnTheSocket() {
            var pose = GripPlacement.HoldPose(_offsetGrip, _contract, "Hand", _report);

            var gripInFigure = new Pose(pose.position + pose.rotation * GripPosition, pose.rotation * GripRotation);

            AssertPose(HandPoseInFigure(), gripInFigure);
            Assert.IsTrue(_report.IsEmpty, _report.ToText());
        }

        [Test]
        public void HoldPose_SocketWithoutANode_UsesTheDefaultLocalPoseUnderTheParent() {
            var pose = GripPlacement.HoldPose(_centredGrip, _contract, "Back", _report);

            var expected = new Pose(
                HipsPosition + HipsRotation * BackPosition,
                HipsRotation * Quaternion.Euler(BackEulerAngles));

            AssertPose(expected, pose);
            Assert.IsTrue(_report.IsEmpty, _report.ToText());
        }

        [Test]
        public void HoldPose_UnknownSocket_ReportsUnknownSocketAndReturnsIdentity() {
            var pose = GripPlacement.HoldPose(_offsetGrip, _contract, "Belt", _report);

            AssertIdentity(pose);
            var finding = SingleFinding("character.unknown-socket", FindingSeverity.Error);
            StringAssert.Contains("Belt", finding.Message);
            StringAssert.Contains("Hand", finding.Message);
        }

        [Test]
        public void HoldPose_NoReferenceFigure_WarnsAndReturnsIdentity() {
            _contract.ReferenceFigure = null;

            var pose = GripPlacement.HoldPose(_offsetGrip, _contract, "Hand", _report);

            AssertIdentity(pose);
            var finding = SingleFinding("character.no-reference-figure", FindingSeverity.Warning);
            Assert.AreEqual("The contract names no reference figure.", finding.Message);
        }

        [Test]
        public void HoldPose_SocketWhoseParentIsMissing_ReportsTheParentBone() {
            var pose = GripPlacement.HoldPose(_offsetGrip, _contract, "Tail", _report);

            AssertIdentity(pose);
            var finding = SingleFinding("character.socket-parent-missing", FindingSeverity.Error);
            StringAssert.Contains("Hips/Tail", finding.Message);
        }

        [Test]
        public void HoldPose_NoGripNode_ReportsGripNodeMissing() {
            var pose = GripPlacement.HoldPose(null, _contract, "Hand", _report);

            AssertIdentity(pose);
            SingleFinding("character.grip-node-missing", FindingSeverity.Error);
        }

        [Test]
        public void HoldPose_ThrowsOnANullContractOrReport() {
            Assert.Throws<ArgumentNullException>(() => GripPlacement.HoldPose(_offsetGrip, null, "Hand", _report));
            Assert.Throws<ArgumentNullException>(() => GripPlacement.HoldPose(_offsetGrip, _contract, "Hand", null));
        }

        [Test]
        public void HoldPose_AddsNothingToTheFigureOrTheProp() {
            var figureChildren = _figure.GetComponentsInChildren<Transform>().Length;
            var figureComponents = _figure.GetComponentsInChildren<Component>().Length;
            var propComponents = _propRoot.GetComponentsInChildren<Component>().Length;

            GripPlacement.HoldPose(_offsetGrip, _contract, "Back", _report);

            Assert.AreEqual(figureChildren, _figure.GetComponentsInChildren<Transform>().Length);
            Assert.AreEqual(figureComponents, _figure.GetComponentsInChildren<Component>().Length);
            Assert.AreEqual(propComponents, _propRoot.GetComponentsInChildren<Component>().Length);
        }

        [Test]
        public void CharacterKitHoldPose_GivesTheSameAnswerAsGripPlacement() {
            var expected = GripPlacement.HoldPose(_offsetGrip, _contract, "Hand", new ValidationReport("grip"));

            var pose = CharacterKit.Rig.HoldPose(_offsetGrip, _contract, "Hand", _report);

            AssertPose(expected, pose);
            AssertPose(HandPoseInFigure(), new Pose(
                pose.position + pose.rotation * GripPosition,
                pose.rotation * GripRotation));
        }

        private ValidationFinding SingleFinding(string code, FindingSeverity severity) {
            Assert.AreEqual(1, _report.Findings.Count, _report.ToText());
            var finding = _report.Findings[0];
            Assert.AreEqual(code, finding.Code);
            Assert.AreEqual(severity, finding.Severity);

            return finding;
        }

        private static Pose HandPoseInFigure() =>
                new(HipsPosition + HipsRotation * HandPosition, HipsRotation * HandRotation);

        private static void AssertIdentity(Pose actual) {
            Assert.AreEqual(Vector3.zero, actual.position);
            Assert.AreEqual(Quaternion.identity, actual.rotation);
        }

        private static void AssertPose(Pose expected, Pose actual) {
            Assert.That(Vector3.Distance(expected.position, actual.position), Is.LessThan(Tolerance),
                $"expected {expected.position:F4}, was {actual.position:F4}");
            Assert.That(Quaternion.Angle(expected.rotation, actual.rotation), Is.LessThan(0.01f),
                $"expected {expected.rotation.eulerAngles:F2}, was {actual.rotation.eulerAngles:F2}");
        }

        private static Transform AddChild(Transform parent, string childName, Vector3 position, Quaternion rotation) {
            var child = new GameObject(childName).transform;
            child.SetParent(parent, false);
            child.SetLocalPositionAndRotation(position, rotation);

            return child;
        }
    }
}
