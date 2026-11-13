// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Character.Presentation;
using UnityEngine;

namespace Spellbound.Character.Presentation.Editor.Tests {
    public class MeshBoundsMathTests {
        private readonly List<Object> _created = new();

        [TearDown]
        public void DestroyCreated() {
            for (var i = _created.Count - 1; i >= 0; i--)
                Object.DestroyImmediate(_created[i]);

            _created.Clear();
        }

        [Test]
        public void FindSharedMeshBounds_PrefersTheSkinnedMeshOnTheObject() {
            var root = Track(new GameObject("body"));
            root.AddComponent<SkinnedMeshRenderer>().sharedMesh = MeshOfSize(1f);
            root.AddComponent<MeshFilter>().sharedMesh = MeshOfSize(3f);
            AddChild(root, "skin").AddComponent<SkinnedMeshRenderer>().sharedMesh = MeshOfSize(2f);
            AddChild(root, "prop").AddComponent<MeshFilter>().sharedMesh = MeshOfSize(4f);

            AssertSize(1f, MeshBoundsMath.FindSharedMeshBounds(root));
        }

        [Test]
        public void FindSharedMeshBounds_PrefersASkinnedMeshInTheChildrenOverAMeshFilterOnTheObject() {
            var root = Track(new GameObject("body"));
            root.AddComponent<MeshFilter>().sharedMesh = MeshOfSize(3f);
            AddChild(root, "skin").AddComponent<SkinnedMeshRenderer>().sharedMesh = MeshOfSize(2f);
            AddChild(root, "prop").AddComponent<MeshFilter>().sharedMesh = MeshOfSize(4f);

            AssertSize(2f, MeshBoundsMath.FindSharedMeshBounds(root));
        }

        [Test]
        public void FindSharedMeshBounds_PrefersTheMeshFilterOnTheObjectOverOneInTheChildren() {
            var root = Track(new GameObject("body"));
            root.AddComponent<MeshFilter>().sharedMesh = MeshOfSize(3f);
            AddChild(root, "prop").AddComponent<MeshFilter>().sharedMesh = MeshOfSize(4f);

            AssertSize(3f, MeshBoundsMath.FindSharedMeshBounds(root));
        }

        [Test]
        public void FindSharedMeshBounds_FallsBackToAMeshFilterInTheChildren() {
            var root = Track(new GameObject("body"));
            AddChild(root, "prop").AddComponent<MeshFilter>().sharedMesh = MeshOfSize(4f);

            AssertSize(4f, MeshBoundsMath.FindSharedMeshBounds(root));
        }

        [Test]
        public void FindSharedMeshBounds_ReadsTheSharedMeshNotTheWorldBounds() {
            var root = Track(new GameObject("body"));
            root.transform.localScale = new Vector3(5f, 5f, 5f);
            root.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            var filter = root.AddComponent<MeshFilter>();
            filter.sharedMesh = MeshOfSize(3f);
            root.AddComponent<MeshRenderer>();

            AssertSize(3f, MeshBoundsMath.FindSharedMeshBounds(root));
        }

        [Test]
        public void FindSharedMeshBounds_IsNullWithoutAMesh() {
            var root = Track(new GameObject("body"));
            AddChild(root, "empty").AddComponent<MeshFilter>();

            Assert.IsNull(MeshBoundsMath.FindSharedMeshBounds(root));
            Assert.IsNull(MeshBoundsMath.FindSharedMeshBounds(null));
        }

        [Test]
        public void FindSharedMeshBounds_SizesTheCapsuleAsTheMeshSearchDid() {
            var root = Track(new GameObject("body"));
            root.AddComponent<CapsuleCollider>();
            AddChild(root, "skin").AddComponent<SkinnedMeshRenderer>().sharedMesh =
                    MeshOf(new Bounds(new Vector3(0f, 0.9f, 0f), new Vector3(0.6f, 1.8f, 0.4f)));
            var capsule = new ResizableCapsuleCollider();

            capsule.Initialize(root, MeshBoundsMath.FindSharedMeshBounds(root));

            Assert.That(capsule.DefaultColliderData.Height, Is.EqualTo(1.8f).Within(1e-5f));
            Assert.That(capsule.DefaultColliderData.Radius, Is.EqualTo(0.3f).Within(1e-5f));
        }

        private GameObject Track(GameObject gameObject) {
            _created.Add(gameObject);

            return gameObject;
        }

        private static GameObject AddChild(GameObject parent, string childName) {
            var child = new GameObject(childName);
            child.transform.SetParent(parent.transform, false);

            return child;
        }

        private Mesh MeshOfSize(float size) => MeshOf(new Bounds(Vector3.zero, new Vector3(size, size, size)));

        private Mesh MeshOf(Bounds bounds) {
            var mesh = new Mesh { bounds = bounds };
            _created.Add(mesh);

            return mesh;
        }

        private static void AssertSize(float expected, Bounds? bounds) {
            Assert.IsTrue(bounds.HasValue);
            Assert.That(Vector3.Distance(new Vector3(expected, expected, expected), bounds.Value.size),
                Is.LessThan(1e-5f));
        }
    }
}
