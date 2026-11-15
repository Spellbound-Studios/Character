// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.StateMachine.Editor.Tests {
    public class BodyContextMotionTests {
        private GameObject _body;

        [SetUp]
        public void CreateBody() {
            _body = new GameObject("body", typeof(Rigidbody), typeof(CapsuleCollider));
        }

        [TearDown]
        public void DestroyBody() {
            Object.DestroyImmediate(_body);
        }

        [Test]
        public void Motion_HoldsItsOwnClaimsWithNoActiveDriver() {
            var context = new BodyContext(_body.transform, _body.GetComponent<Rigidbody>(),
                new ResizableCapsuleCollider(), new ReferenceFrameBinding());

            Assert.IsNotNull(context.Motion);
            Assert.AreSame(context.Motion, context.Motion);
            Assert.IsNull(context.Motion.Active);
            Assert.AreEqual(0, context.Motion.Count);
        }
    }
}
