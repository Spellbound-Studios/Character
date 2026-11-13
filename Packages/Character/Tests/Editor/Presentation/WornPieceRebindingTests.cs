// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Linq;
using NUnit.Framework;
using Spellbound.Character.Presentation;
using Spellbound.Character.Rig;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Presentation.Editor.Tests {
    public class WornPieceRebindingTests {
        private static readonly string[] BoneNames = { "hips", "spine", "hand" };

        private Scene _previewScene;
        private Transform _targetRoot;
        private Transform[] _targetBones;
        private GameObject _piece;

        [SetUp]
        public void CreateFixtures() {
            _previewScene = EditorSceneManager.NewPreviewScene();

            var target = new GameObject("root");
            SceneManager.MoveGameObjectToScene(target, _previewScene);
            _targetRoot = target.transform;
            _targetBones = AddChain(_targetRoot, BoneNames);

            _piece = new GameObject("piece");
            SceneManager.MoveGameObjectToScene(_piece, _previewScene);
        }

        [TearDown]
        public void DestroyFixtures() {
            EditorSceneManager.ClosePreviewScene(_previewScene);
        }

        [Test]
        public void Rebind_RewritesTheBonesWhenEveryBoneExists() {
            var renderer = AddRenderer(_piece.transform, "body", BoneNames);

            var map = WornPieceRebinding.Rebind(_piece, _targetRoot);

            AssertSameBones(_targetBones, renderer.bones);
            Assert.AreSame(_targetBones[0], renderer.rootBone);
            Assert.IsTrue(map.IsComplete);
            CollectionAssert.IsEmpty(map.Missing);
            Assert.AreEqual(BoneNames.Length, map.Bones.Count);

            for (var i = 0; i < BoneNames.Length; i++)
                Assert.AreSame(_targetBones[i], map.Bones[BoneNames[i]]);
        }

        [Test]
        public void Rebind_ChangesNothingWhenABoneIsMissing() {
            var renderer = AddRenderer(_piece.transform, "body", new[] { "hips", "spine", "hand_old" });
            var bonesBefore = EntityIdsOf(renderer.bones);
            var rootBoneBefore = renderer.rootBone.GetEntityId();

            var map = WornPieceRebinding.Rebind(_piece, _targetRoot);

            Assert.IsFalse(map.IsComplete);
            CollectionAssert.AreEqual(new[] { "hand_old" }, map.Missing);
            CollectionAssert.AreEqual(bonesBefore, EntityIdsOf(renderer.bones));
            Assert.AreEqual(rootBoneBefore, renderer.rootBone.GetEntityId());
            Assert.AreSame(_targetBones[0], map.Bones["hips"]);
            Assert.AreSame(_targetBones[1], map.Bones["spine"]);
            Assert.IsFalse(map.Bones.ContainsKey("hand_old"));
        }

        [Test]
        public void Rebind_ChangesNeitherRendererWhenOneMissesABone() {
            var complete = AddRenderer(_piece.transform, "body", BoneNames);
            var incomplete = AddRenderer(_piece.transform, "glove", new[] { "hips", "hand_old" });
            var completeBefore = EntityIdsOf(complete.bones);
            var incompleteBefore = EntityIdsOf(incomplete.bones);
            var completeRootBefore = complete.rootBone.GetEntityId();
            var incompleteRootBefore = incomplete.rootBone.GetEntityId();

            var map = WornPieceRebinding.Rebind(_piece, _targetRoot);

            Assert.IsFalse(map.IsComplete);
            CollectionAssert.AreEqual(completeBefore, EntityIdsOf(complete.bones));
            CollectionAssert.AreEqual(incompleteBefore, EntityIdsOf(incomplete.bones));
            Assert.AreEqual(completeRootBefore, complete.rootBone.GetEntityId());
            Assert.AreEqual(incompleteRootBefore, incomplete.rootBone.GetEntityId());
        }

        [Test]
        public void Rebind_ListsEachMissingNameOnceInFirstSeenOrder() {
            AddRenderer(_piece.transform, "body", new[] { "hips", "tail", "hand_old" });
            AddRenderer(_piece.transform, "glove", new[] { "hips", "hand_old", "tail" });

            var map = WornPieceRebinding.Rebind(_piece, _targetRoot);

            CollectionAssert.AreEqual(new[] { "tail", "hand_old" }, map.Missing);
        }

        [Test]
        public void Rebind_MatchesTheTargetRootByItsOwnName() {
            var renderer = AddRenderer(_piece.transform, "body", new[] { "root", "hips" });

            var map = WornPieceRebinding.Rebind(_piece, _targetRoot);

            Assert.IsTrue(map.IsComplete);
            Assert.AreSame(_targetRoot, renderer.bones[0]);
            Assert.AreSame(_targetRoot, renderer.rootBone);
        }

        [Test]
        public void Rebind_RewritesARendererOnThePieceItself() {
            var renderer = _piece.AddComponent<SkinnedMeshRenderer>();
            var skeleton = AddChain(_piece.transform, BoneNames);
            renderer.bones = skeleton;
            renderer.rootBone = skeleton[0];

            var map = WornPieceRebinding.Rebind(_piece, _targetRoot);

            Assert.IsTrue(map.IsComplete);
            AssertSameBones(_targetBones, renderer.bones);
        }

        [Test]
        public void Rebind_GivesAnEmptyCompleteMapWithNoRenderer() {
            AddChain(_piece.transform, BoneNames);

            var map = WornPieceRebinding.Rebind(_piece, _targetRoot);

            Assert.IsTrue(map.IsComplete);
            CollectionAssert.IsEmpty(map.Missing);
            Assert.AreEqual(0, map.Bones.Count);
        }

        [Test]
        public void Rebind_AddsNoComponentAndNoObject() {
            AddRenderer(_piece.transform, "body", BoneNames);
            var componentsBefore = _piece.GetComponentsInChildren<Component>(true).Length
                + _targetRoot.GetComponentsInChildren<Component>(true).Length;
            var objectsBefore = _previewScene.rootCount;

            WornPieceRebinding.Rebind(_piece, _targetRoot);

            Assert.AreEqual(componentsBefore, _piece.GetComponentsInChildren<Component>(true).Length
                + _targetRoot.GetComponentsInChildren<Component>(true).Length);
            Assert.AreEqual(objectsBefore, _previewScene.rootCount);
        }

        [Test]
        public void Rebind_ThrowsOnANullArgument() {
            Assert.Throws<ArgumentNullException>(() => WornPieceRebinding.Rebind(null, _targetRoot));
            Assert.Throws<ArgumentNullException>(() => WornPieceRebinding.Rebind(_piece, null));
        }

        private static SkinnedMeshRenderer AddRenderer(Transform parent, string name, string[] boneNames) {
            var holder = new GameObject(name);
            holder.transform.SetParent(parent, false);
            var renderer = holder.AddComponent<SkinnedMeshRenderer>();
            var skeleton = AddChain(holder.transform, boneNames);
            renderer.bones = skeleton;
            renderer.rootBone = skeleton[0];
            return renderer;
        }

        private static Transform[] AddChain(Transform parent, string[] names) {
            var chain = new Transform[names.Length];
            var current = parent;

            for (var i = 0; i < names.Length; i++) {
                var bone = new GameObject(names[i]).transform;
                bone.SetParent(current, false);
                chain[i] = bone;
                current = bone;
            }

            return chain;
        }

        private static void AssertSameBones(Transform[] expected, Transform[] actual) {
            Assert.AreEqual(expected.Length, actual.Length);

            for (var i = 0; i < expected.Length; i++)
                Assert.AreSame(expected[i], actual[i]);
        }

        private static EntityId[] EntityIdsOf(Transform[] bones) => bones.Select(bone => bone.GetEntityId()).ToArray();
    }
}
