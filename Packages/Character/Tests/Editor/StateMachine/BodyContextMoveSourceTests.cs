// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.StateMachine.Editor.Tests {
    public class BodyContextMoveSourceTests {
        private const float Tolerance = 0.0001f;

        private GameObject _body;
        private BodyContext _context;

        [SetUp]
        public void CreateContext() {
            _body = new GameObject("body", typeof(Rigidbody), typeof(CapsuleCollider));
            _context = new BodyContext(_body.transform, _body.GetComponent<Rigidbody>(),
                new ResizableCapsuleCollider(), new ReferenceFrameBinding());
        }

        [TearDown]
        public void DestroyBody() {
            Object.DestroyImmediate(_body);
        }

        [Test]
        public void DesiredMoveAndUp_WithASource_ReadTheSource() {
            var source = new ContextMoveSource {
                DesiredMove = new Vector3(0.5f, 0f, -0.25f),
                DesiredUp = new Vector3(1f, 0f, 0f)
            };
            _context.DesiredMove = new Vector3(0f, 0f, 1f);
            _context.MoveSource = source;

            Assert.AreSame(source, _context.MoveSource);
            Assert.Less(Vector3.Distance(source.DesiredMove, _context.DesiredMove), Tolerance);
            Assert.Less(Vector3.Distance(source.DesiredUp, _context.DesiredUp), Tolerance);

            source.DesiredMove = new Vector3(-1f, 0f, 0f);

            Assert.Less(Vector3.Distance(source.DesiredMove, _context.DesiredMove), Tolerance);
        }

        [Test]
        public void DesiredMoveAndUp_WithoutASource_FallBackToTheWrittenMoveAndWorldUp() {
            _context.DesiredMove = new Vector3(0f, 0f, 1f);

            Assert.IsNull(_context.MoveSource);
            Assert.Less(Vector3.Distance(new Vector3(0f, 0f, 1f), _context.DesiredMove), Tolerance);
            Assert.Less(Vector3.Distance(new Vector3(0f, 1f, 0f), _context.DesiredUp), Tolerance);
        }

        private sealed class ContextMoveSource : IMoveSource {
            public Vector3 DesiredMove { get; set; }

            public Vector3 DesiredUp { get; set; }
        }
    }
}
