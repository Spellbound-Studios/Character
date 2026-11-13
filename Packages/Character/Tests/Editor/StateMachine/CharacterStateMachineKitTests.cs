// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.StateMachine;
using UnityEngine;

namespace Spellbound.Character.StateMachine.Editor.Tests {
    public class CharacterStateMachineKitTests {
        private GameObject _body;
        private FirstTestState _first;
        private SecondTestState _second;

        [SetUp]
        public void CreateBody() {
            _body = new GameObject("body", typeof(Rigidbody), typeof(CapsuleCollider));
            _first = ScriptableObject.CreateInstance<FirstTestState>();
            _second = ScriptableObject.CreateInstance<SecondTestState>();
        }

        [TearDown]
        public void DestroyBody() {
            Object.DestroyImmediate(_first);
            Object.DestroyImmediate(_second);
            Object.DestroyImmediate(_body);
        }

        [Test]
        public void Create_OverBodyContext_StartsInInitialStateWithThatContext() {
            var context = new BodyContext(_body.transform, _body.GetComponent<Rigidbody>(),
                new ResizableCapsuleCollider(), new ReferenceFrameBinding());
            var configuration = new StateMachineConfiguration<TestKey>(
                TestKey.Second,
                new StateBinding<TestKey>(TestKey.First, _first),
                new StateBinding<TestKey>(TestKey.Second, _second));

            using var machine = CharacterStateMachineKit.Create(context, configuration);

            Assert.AreSame(context, machine.Context);
            Assert.AreEqual(TestKey.Second, machine.CurrentStateKey);
            Assert.IsTrue(machine.IsInState<SecondTestState>());
        }
    }
}
