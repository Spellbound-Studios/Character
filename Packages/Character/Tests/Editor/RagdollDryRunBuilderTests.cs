// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class RagdollDryRunBuilderTests {
        private Scene _previewScene;
        private Transform _root;
        private RagdollProfile _profile;

        [SetUp]
        public void CreateModel() {
            _previewScene = EditorSceneManager.NewPreviewScene();
            var root = new GameObject("Model");
            SceneManager.MoveGameObjectToScene(root, _previewScene);
            _root = root.transform;

            var hips = AddChild(_root, "Hips");
            AddChild(hips, "Spine");

            _profile = ScriptableObject.CreateInstance<RagdollProfile>();
            _profile.EditableBones.Add(new RagdollBoneDefinition {
                Name = "Hips",
                BonePath = "Hips",
                ParentIndex = -1,
                Shape = RagdollColliderShape.Box,
                MassShare = 0.6f
            });
            _profile.EditableBones.Add(new RagdollBoneDefinition {
                Name = "Spine",
                BonePath = "Hips/Spine",
                ParentIndex = 0,
                Shape = RagdollColliderShape.Capsule,
                MassShare = 0.4f
            });
        }

        [TearDown]
        public void DestroyModel() {
            Object.DestroyImmediate(_profile);
            EditorSceneManager.ClosePreviewScene(_previewScene);
        }

        [Test]
        public void DryRun_OnACompleteModel_WouldBuildEveryBoneAndAddsNothing() {
            var componentsBefore = ComponentCount();

            var dryRun = RagdollBuilder.DryRun(_root, null, _profile);

            Assert.IsTrue(dryRun.WouldBuild, dryRun.Error);
            Assert.AreEqual(string.Empty, dryRun.Error);
            Assert.AreEqual(2, dryRun.Bones.Count);
            Assert.AreEqual("Hips", dryRun.Bones[0].Name);
            Assert.AreEqual("Hips", dryRun.Bones[0].Path);
            Assert.AreEqual(RagdollColliderShape.Box, dryRun.Bones[0].Shape);
            Assert.AreEqual(0.6f, dryRun.Bones[0].MassShare);
            Assert.AreEqual("Spine", dryRun.Bones[1].Name);
            Assert.AreEqual("Hips/Spine", dryRun.Bones[1].Path);
            Assert.AreEqual(RagdollColliderShape.Capsule, dryRun.Bones[1].Shape);
            Assert.AreEqual(0.4f, dryRun.Bones[1].MassShare);
            Assert.IsEmpty(dryRun.Missing);
            Assert.AreEqual(componentsBefore, ComponentCount());
        }

        [Test]
        public void DryRun_OnAnIncompleteModel_NamesTheMissingBoneAndAddsNothing() {
            _profile.EditableBones.Add(new RagdollBoneDefinition {
                Name = "Head",
                BonePath = "Hips/Spine/Head",
                ParentIndex = 1,
                Shape = RagdollColliderShape.Sphere,
                MassShare = 0.1f
            });
            var componentsBefore = ComponentCount();

            var dryRun = RagdollBuilder.DryRun(_root, null, _profile);

            Assert.IsFalse(dryRun.WouldBuild);
            CollectionAssert.AreEqual(new[] { "Head" }, dryRun.Missing);
            StringAssert.Contains("Head", dryRun.Error);
            StringAssert.Contains("could not be resolved", dryRun.Error);
            Assert.AreEqual(2, dryRun.Bones.Count);
            Assert.AreEqual(componentsBefore, ComponentCount());
        }

        [Test]
        public void DryRun_OnABoneHoldingAnotherCollider_GivesTheColliderError() {
            _root.Find("Hips").gameObject.AddComponent<SphereCollider>();
            var componentsBefore = ComponentCount();

            var dryRun = RagdollBuilder.DryRun(_root, null, _profile);

            Assert.IsFalse(dryRun.WouldBuild);
            StringAssert.Contains("already holds a SphereCollider", dryRun.Error);
            Assert.IsEmpty(dryRun.Missing);
            Assert.AreEqual(componentsBefore, ComponentCount());
        }

        [Test]
        public void DryRun_WithNoProfile_GivesTheBuildersErrorAndNoBones() {
            var dryRun = RagdollBuilder.DryRun(_root, null, null);

            Assert.AreEqual("No ragdoll profile assigned.", dryRun.Error);
            Assert.IsEmpty(dryRun.Bones);
            Assert.IsEmpty(dryRun.Missing);
        }

        [Test]
        public void DryRun_WithAnInvalidProfile_GivesTheProfilesErrorAndNoBones() {
            _profile.EditableBones[1].ParentIndex = 3;
            _profile.Validate(out var expected);

            var dryRun = RagdollBuilder.DryRun(_root, null, _profile);

            Assert.AreEqual(expected, dryRun.Error);
            Assert.IsFalse(dryRun.WouldBuild);
            Assert.IsEmpty(dryRun.Bones);
        }

        [Test]
        public void DryRunRagdoll_GivesTheBuildersText() {
            var expected = RagdollBuilder.DryRun(_root, _root.GetComponent<Animator>(), _profile);

            var actual = CharacterKit.Rig.DryRunRagdoll(_root, _profile);

            Assert.AreEqual(expected.ToText(), actual.ToText());
            StringAssert.StartsWith("would build 2 bones", actual.ToText());
        }

        private int ComponentCount() => _root.GetComponentsInChildren<Component>(true).Length;

        private static Transform AddChild(Transform parent, string name) {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);

            return child;
        }
    }
}
