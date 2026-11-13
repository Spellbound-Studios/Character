// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class ResizableCapsuleColliderTests {
        private const float Tolerance = 1e-5f;

        [Test]
        public void Initialize_SizesTheCapsuleFromTheMeshBounds() {
            var body = new GameObject("capsule-body");

            try {
                body.AddComponent<CapsuleCollider>();
                var capsule = new ResizableCapsuleCollider();

                capsule.Initialize(body, new Bounds(new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 1.8f, 0.4f)));
                capsule.CalculateCapsuleColliderDimensions();

                Assert.That(capsule.DefaultColliderData.Height, Is.EqualTo(1.8f).Within(Tolerance));
                Assert.That(capsule.DefaultColliderData.Radius, Is.EqualTo(0.3f).Within(Tolerance));
                Assert.That(capsule.collider.center.y, Is.EqualTo(1.125f).Within(Tolerance));
                Assert.That(capsule.collider.radius, Is.EqualTo(0.3f).Within(Tolerance));
                Assert.That(capsule.collider.height, Is.EqualTo(1.3499999f).Within(Tolerance));
                Assert.That(capsule.CapsuleFloatData.RideHeight, Is.EqualTo(1.125f).Within(Tolerance));
            }
            finally {
                Object.DestroyImmediate(body);
            }
        }

        [Test]
        public void Initialize_FallsBackWithoutMeshBounds() {
            var data = new DefaultColliderData();

            data.Initialize(null);

            Assert.That(data.Height, Is.EqualTo(1.8f).Within(Tolerance));
            Assert.That(data.Radius, Is.EqualTo(0.3f).Within(Tolerance));
        }

        [Test]
        public void Initialize_KeepsTheDefaultLimitsAndFallbacks() {
            var data = new DefaultColliderData();

            data.Initialize(new Bounds(Vector3.zero, new Vector3(0.02f, 12f, 0.02f)));

            Assert.That(data.Height, Is.EqualTo(10f).Within(Tolerance));
            Assert.That(data.Radius, Is.EqualTo(0.05f).Within(Tolerance));
        }

        [Test]
        public void Initialize_ClampsToTheConfiguredLimitsAndFallbacks() {
            var data = new DefaultColliderData {
                MaximumHeight = 20f,
                MinimumRadius = 0.005f,
                FallbackHeight = 2.4f,
                FallbackRadius = 0.5f
            };

            data.Initialize(new Bounds(Vector3.zero, new Vector3(0.02f, 12f, 0.02f)));

            Assert.That(data.Height, Is.EqualTo(12f).Within(Tolerance));
            Assert.That(data.Radius, Is.EqualTo(0.01f).Within(Tolerance));

            data.Initialize(null);

            Assert.That(data.Height, Is.EqualTo(2.4f).Within(Tolerance));
            Assert.That(data.Radius, Is.EqualTo(0.5f).Within(Tolerance));
        }
    }
}
