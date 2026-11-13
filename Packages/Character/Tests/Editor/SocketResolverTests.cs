// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class SocketResolverTests {
        private const float Tolerance = 1e-4f;

        private Scene _previewScene;
        private Transform _root;
        private Transform _rightHand;
        private Transform _leftHand;
        private Transform _handNode;
        private RigContract _contract;

        [SetUp]
        public void CreateFigure() {
            _previewScene = EditorSceneManager.NewPreviewScene();
            var root = new GameObject("Character");
            SceneManager.MoveGameObjectToScene(root, _previewScene);
            _root = root.transform;
            _root.SetPositionAndRotation(new Vector3(5f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));

            var hips = AddChild(_root, "Hips", new Vector3(0f, 1f, 0f), Quaternion.identity);
            _rightHand = AddChild(hips, "RightHand", new Vector3(0.5f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
            _leftHand = AddChild(hips, "LeftHand", new Vector3(-0.5f, 0f, 0f), Quaternion.identity);
            _handNode = AddChild(_rightHand, "socket_hand", new Vector3(0f, 0f, 0.2f), Quaternion.identity);

            _contract = ScriptableObject.CreateInstance<RigContract>();
            _contract.EditableSockets.Add(new RigSocket {
                Id = "hand",
                ParentBone = RigBoneReference.Path("Hips/RightHand")
            });
            _contract.EditableSockets.Add(new RigSocket {
                Id = "offhand",
                ParentBone = RigBoneReference.Path("Hips/LeftHand"),
                DefaultLocalPosition = new Vector3(0f, 0f, 0.2f),
                DefaultLocalEulerAngles = new Vector3(0f, 90f, 0f)
            });
            _contract.EditableSockets.Add(new RigSocket {
                Id = "back",
                ParentBone = RigBoneReference.Path("Hips/Spine")
            });
        }

        [TearDown]
        public void DestroyFigure() {
            Object.DestroyImmediate(_contract);
            EditorSceneManager.ClosePreviewScene(_previewScene);
        }

        [Test]
        public void Resolve_FindsTheNodeUnderTheParentBone() {
            var table = SocketResolver.Resolve(_root, null, _contract);

            Assert.IsTrue(table.TryGet("hand", out var socket));
            Assert.AreEqual("hand", socket.Id);
            Assert.IsTrue(socket.HasNode);
            Assert.AreSame(_handNode, socket.Node);
            Assert.AreSame(_rightHand, socket.Parent);
            AssertPose(new Pose(new Vector3(0f, 0f, 0.2f), Quaternion.identity), socket.LocalPose);
        }

        [Test]
        public void Resolve_DefaultsASocketWhoseNodeIsAbsent() {
            var table = SocketResolver.Resolve(_root, null, _contract);

            Assert.IsTrue(table.TryGet("offhand", out var socket));
            Assert.IsFalse(socket.HasNode);
            Assert.IsNull(socket.Node);
            Assert.AreSame(_leftHand, socket.Parent);
            AssertPose(new Pose(new Vector3(0f, 0f, 0.2f), Quaternion.Euler(0f, 90f, 0f)), socket.LocalPose);
            CollectionAssert.AreEqual(new[] { "offhand" }, table.Defaulted);
        }

        [Test]
        public void Resolve_ListsASocketWhoseParentIsAbsentAsMissing() {
            var table = SocketResolver.Resolve(_root, null, _contract);

            CollectionAssert.AreEqual(new[] { "back" }, table.Missing);
            Assert.IsFalse(table.IsComplete);
            Assert.IsFalse(table.TryGet("back", out _));
            Assert.AreEqual(2, table.Count);
            CollectionAssert.AreEqual(new[] { "hand", "offhand" }, table.Ids);
        }

        [Test]
        public void Resolve_IsCompleteWhenEveryParentResolves() {
            _contract.EditableSockets.RemoveAt(2);

            var table = SocketResolver.Resolve(_root, null, _contract);

            Assert.IsTrue(table.IsComplete);
            Assert.IsEmpty(table.Missing);
        }

        [Test]
        public void Resolve_LooksOnlyAtTheParentsDirectChildren() {
            AddChild(AddChild(_leftHand, "Palm", Vector3.zero, Quaternion.identity), "socket_offhand", Vector3.zero,
                Quaternion.identity);

            var table = SocketResolver.Resolve(_root, null, _contract);

            Assert.IsTrue(table.TryGet("offhand", out var socket));
            Assert.IsFalse(socket.HasNode);
        }

        [Test]
        public void Resolve_ThrowsOnANullRootOrContract() {
            Assert.Throws<ArgumentNullException>(() => SocketResolver.Resolve(null, null, _contract));
            Assert.Throws<ArgumentNullException>(() => SocketResolver.Resolve(_root, null, null));
        }

        [Test]
        public void Resolve_AddsNoTransformOrComponent() {
            var before = _root.GetComponentsInChildren<Component>(true).Length;

            SocketResolver.Resolve(_root, null, _contract);

            Assert.AreEqual(before, _root.GetComponentsInChildren<Component>(true).Length);
        }

        [Test]
        public void Table_KeepsItsAnswersWhenTheHierarchyChangesAfterwards() {
            var table = SocketResolver.Resolve(_root, null, _contract);
            AddChild(_leftHand, "socket_offhand", Vector3.zero, Quaternion.identity);

            Assert.IsTrue(table.TryGet("offhand", out var socket));
            Assert.IsFalse(socket.HasNode);
            CollectionAssert.AreEqual(new[] { "offhand" }, table.Defaulted);
        }

        [Test]
        public void PoseIn_GivesTheNodesPoseInTheRootsSpace() {
            var table = SocketResolver.Resolve(_root, null, _contract);
            table.TryGet("hand", out var socket);

            AssertPose(new Pose(new Vector3(0.7f, 1f, 0f), Quaternion.Euler(0f, 90f, 0f)), socket.PoseIn(_root));
        }

        [Test]
        public void PoseIn_ComposesTheParentWithTheDefaultLocalPose() {
            var table = SocketResolver.Resolve(_root, null, _contract);
            table.TryGet("offhand", out var socket);

            AssertPose(new Pose(new Vector3(-0.5f, 1f, 0.2f), Quaternion.Euler(0f, 90f, 0f)), socket.PoseIn(_root));
        }

        [Test]
        public void Table_ThrowsOnANullList() {
            var none = Array.Empty<string>();
            var resolved = Array.Empty<ResolvedSocket>();

            Assert.Throws<ArgumentNullException>(() => new SocketTable(null, none, none));
            Assert.Throws<ArgumentNullException>(() => new SocketTable(resolved, null, none));
            Assert.Throws<ArgumentNullException>(() => new SocketTable(resolved, none, null));
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
