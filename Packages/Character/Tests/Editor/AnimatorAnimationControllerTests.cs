// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class AnimatorAnimationControllerTests {
        private const string Folder = "Assets/CharacterAnimatorTests";

        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Run = Animator.StringToHash("Run");

        private GameObject _character;
        private Animator _animator;

        [SetUp]
        public void CreateAnimator() {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets", "CharacterAnimatorTests");

            var controller = AnimatorController.CreateAnimatorControllerAtPath(Folder + "/Character.controller");
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var baseLayer = controller.layers[0].stateMachine;
            baseLayer.AddState("Idle");
            baseLayer.AddState("Run");
            controller.AddLayer("Upper Body");
            controller.layers[1].stateMachine.AddState("Wave");

            _character = new GameObject("animated-character");
            _animator = _character.AddComponent<Animator>();
            _animator.runtimeAnimatorController = controller;
            _animator.Rebind();
            _animator.Update(0f);
        }

        [TearDown]
        public void DestroyAnimator() {
            Object.DestroyImmediate(_character);
            AssetDatabase.DeleteAsset(Folder);
        }

        [Test]
        public void SetFloat_SetsTheAnimatorParameter() {
            IAnimationController controller = new AnimatorAnimationController(_animator);

            controller.SetFloat(Speed, 2.5f);

            Assert.AreEqual(2.5f, _animator.GetFloat(Speed));
        }

        [Test]
        public void SetLayerWeight_SetsTheAnimatorLayerWeight() {
            IAnimationController controller = new AnimatorAnimationController(_animator);

            controller.SetLayerWeight(1, 0.5f);

            Assert.AreEqual(0.5f, _animator.GetLayerWeight(1), 1e-5f);
        }

        [Test]
        public void CrossFade_StartsTheTransitionOnTheLayer() {
            IAnimationController controller = new AnimatorAnimationController(_animator);

            controller.CrossFade(Run, 0, 0.25f);
            _animator.Update(0.01f);

            Assert.IsTrue(_animator.IsInTransition(0));
            Assert.AreEqual(Run, _animator.GetNextAnimatorStateInfo(0).shortNameHash);
        }

        [Test]
        public void CrossFade_FadesOverTheGivenSeconds() {
            IAnimationController controller = new AnimatorAnimationController(_animator);

            controller.CrossFade(Run, 0, 0.25f);
            _animator.Update(0.01f);
            var transition = _animator.GetAnimatorTransitionInfo(0);

            Assert.AreEqual(DurationUnit.Fixed, transition.durationUnit);
            Assert.AreEqual(0.25f, transition.duration, 1e-5f);
        }

        [Test]
        public void Constructor_RejectsAMissingAnimator() {
            Assert.Throws<ArgumentNullException>(() => new AnimatorAnimationController(null));
        }
    }
}
