// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.StateMachine.Editor.Tests {
    public class BodyStateTests {
        private GameObject _body;
        private FirstTestState _first;

        [SetUp]
        public void CreateBody() {
            _body = new GameObject("body", typeof(Rigidbody), typeof(CapsuleCollider));
            _first = ScriptableObject.CreateInstance<FirstTestState>();
        }

        [TearDown]
        public void DestroyBody() {
            Object.DestroyImmediate(_first);
            Object.DestroyImmediate(_body);
        }

        [Test]
        public void BodyState_CustomImplementation() {
            var context = CreateContext();

            _first.InitializeWithContext(context);

            Assert.AreSame(context, _first.Body);
            Assert.AreSame(_body.GetComponent<Rigidbody>(), _first.Body.Rigidbody);
        }

        private BodyContext CreateContext() =>
                new(_body.transform, _body.GetComponent<Rigidbody>(), new ResizableCapsuleCollider(),
                    new ReferenceFrameBinding());
    }
}
