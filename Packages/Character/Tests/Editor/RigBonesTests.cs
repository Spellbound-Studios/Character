// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spellbound.Character.Editor.Tests {
    public class RigBonesTests {
        private Scene _previewScene;
        private Transform _root;
        private Transform _hips;
        private Transform _spine;

        [SetUp]
        public void CreateHierarchy() {
            _previewScene = EditorSceneManager.NewPreviewScene();
            var root = new GameObject("Character");
            SceneManager.MoveGameObjectToScene(root, _previewScene);
            _root = root.transform;
            _hips = AddChild(_root, "Hips");
            _spine = AddChild(_hips, "Spine");
        }

        [TearDown]
        public void DestroyHierarchy() {
            EditorSceneManager.ClosePreviewScene(_previewScene);
        }

        [Test]
        public void Human_WantsTheHumanBoneAndNoPath() {
            var reference = RigBoneReference.Human(HumanBodyBones.Chest);

            Assert.IsTrue(reference.UseHumanBone);
            Assert.AreEqual(HumanBodyBones.Chest, reference.HumanBone);
            Assert.IsTrue(string.IsNullOrEmpty(reference.BonePath));
            Assert.IsFalse(reference.IsEmpty);
        }

        [Test]
        public void Path_WantsThePathAndNoHumanBone() {
            var reference = RigBoneReference.Path("Hips/Spine");

            Assert.IsFalse(reference.UseHumanBone);
            Assert.AreEqual("Hips/Spine", reference.BonePath);
            Assert.IsFalse(reference.IsEmpty);
        }

        [Test]
        public void IsEmpty_WhenNoHumanBoneIsWantedAndThePathIsEmpty() {
            Assert.IsTrue(new RigBoneReference().IsEmpty);
            Assert.IsTrue(RigBoneReference.Path(string.Empty).IsEmpty);
            Assert.IsNull(RigBones.Resolve(_root, null, new RigBoneReference()));
        }

        [Test]
        public void Resolve_FindsTheBonePathUnderTheRoot() {
            Assert.AreSame(_spine, RigBones.Resolve(_root, null, RigBoneReference.Path("Hips/Spine")));
        }

        [Test]
        public void Resolve_IsNullWhenThePathMatchesNothingOrTheRootIsNull() {
            Assert.IsNull(RigBones.Resolve(_root, null, RigBoneReference.Path("Hips/Neck")));
            Assert.IsNull(RigBones.Resolve(null, null, RigBoneReference.Path("Hips/Spine")));
        }

        [Test]
        public void Resolve_FallsBackToThePathWhenTheAnimatorIsNotHuman() {
            var animator = _root.gameObject.AddComponent<Animator>();
            var reference = new RigBoneReference {
                UseHumanBone = true,
                HumanBone = HumanBodyBones.Spine,
                BonePath = "Hips/Spine"
            };

            Assert.IsFalse(animator.isHuman);
            Assert.AreSame(_spine, RigBones.Resolve(_root, animator, reference));
            Assert.IsNull(RigBones.Resolve(_root, animator, RigBoneReference.Human(HumanBodyBones.Spine)));
        }

        [Test]
        public void FindDescendant_ReturnsTheFirstDepthFirstMatch() {
            var deep = AddChild(AddChild(_spine, "Chest"), "Hand");
            AddChild(_root, "Hand");

            Assert.AreSame(deep, RigBones.FindDescendant(_root, "Hand"));
        }

        [Test]
        public void FindDescendant_MatchesTheExactNameOnly() {
            Assert.IsNull(RigBones.FindDescendant(_root, "spine"));
            Assert.IsNull(RigBones.FindDescendant(_root, "Character"));
            Assert.AreSame(_spine, RigBones.FindDescendant(_root, "Spine"));
        }

        [Test]
        public void PathOf_JoinsTheNamesBelowTheRoot() {
            Assert.AreEqual("Hips/Spine", RigBones.PathOf(_root, _spine));
            Assert.AreEqual("Spine", RigBones.PathOf(_hips, _spine));
        }

        [Test]
        public void PathOf_IsEmptyForTheRootAndNullOutsideIt() {
            var outside = new GameObject("Outside");
            SceneManager.MoveGameObjectToScene(outside, _previewScene);

            Assert.AreEqual(string.Empty, RigBones.PathOf(_root, _root));
            Assert.IsNull(RigBones.PathOf(_root, outside.transform));
            Assert.IsNull(RigBones.PathOf(_spine, _hips));
        }

        private static Transform AddChild(Transform parent, string childName) {
            var child = new GameObject(childName).transform;
            child.SetParent(parent, false);

            return child;
        }
    }
}
