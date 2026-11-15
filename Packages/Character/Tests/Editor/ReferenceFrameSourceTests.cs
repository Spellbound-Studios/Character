// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Character.Tests;
using Spellbound.Core.ReferenceFrames;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class ReferenceFrameSourceTests {
        private GameObject _root;

        [SetUp]
        public void CreateRoot() {
            _root = new GameObject("reference frame source root");
        }

        [TearDown]
        public void DestroyRoot() {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void FromHierarchyPath_IsDeterministic() {
            var child = CreateChild(_root.transform, "Platform");
            var other = new GameObject("reference frame source root");

            try {
                var twin = CreateChild(other.transform, "Platform");

                Assert.AreEqual(ReferenceFrameIds.FromHierarchyPath(child),
                    ReferenceFrameIds.FromHierarchyPath(child));
                Assert.AreEqual(ReferenceFrameIds.FromHierarchyPath(child),
                    ReferenceFrameIds.FromHierarchyPath(twin));
            }
            finally {
                Object.DestroyImmediate(other);
            }
        }

        [Test]
        public void FromHierarchyPath_DiffersForTwoPaths() {
            var platform = CreateChild(_root.transform, "Platform");
            var elevator = CreateChild(_root.transform, "Elevator");
            var nested = CreateChild(platform, "Elevator");

            Assert.AreNotEqual(ReferenceFrameIds.FromHierarchyPath(platform),
                ReferenceFrameIds.FromHierarchyPath(elevator));
            Assert.AreNotEqual(ReferenceFrameIds.FromHierarchyPath(elevator),
                ReferenceFrameIds.FromHierarchyPath(nested));
        }

        [Test]
        public void FromHierarchyPath_IsNeverZero() {
            var unnamed = new GameObject(string.Empty);

            try {
                Assert.AreEqual(1ul, ReferenceFrameIds.FromHierarchyPath(unnamed.transform).Value);
                Assert.IsFalse(ReferenceFrameIds.FromHierarchyPath(_root.transform).IsWorld);
            }
            finally {
                Object.DestroyImmediate(unnamed);
            }
        }

        [Test]
        public void FromHierarchyPath_RefusesNull() {
            Assert.Throws<ArgumentNullException>(() => ReferenceFrameIds.FromHierarchyPath(null));
        }

        [Test]
        public void Source_WithAZeroId_RegistersUnderItsHierarchyPathId() {
            var world = _root.AddComponent<TestReferenceFrameWorld>();
            var source = CreateChild(_root.transform, "Platform").gameObject.AddComponent<TestReferenceFrameSource>();
            source.World = world;

            Enable(source);

            Assert.IsTrue(source.IsRegistered);
            Assert.AreEqual(ReferenceFrameIds.FromHierarchyPath(source.transform), source.Id);
            Assert.IsTrue(world.ReferenceFrames.Contains(source.Id));
        }

        [Test]
        public void Source_SettingItsWorldWhileRegistered_Throws() {
            var world = _root.AddComponent<TestReferenceFrameWorld>();
            var source = _root.AddComponent<TestReferenceFrameSource>();
            source.World = world;
            Enable(source);

            Assert.IsTrue(source.IsRegistered);
            Assert.Throws<InvalidOperationException>(() => source.World = world);
            Assert.Throws<InvalidOperationException>(() => source.Id = new ReferenceFrameId(7));
        }

        [Test]
        public void Source_WithNoWorld_LogsAnErrorAndStaysUnregistered() {
            var source = _root.AddComponent<TestReferenceFrameSource>();
            LogAssert.Expect(LogType.Error, new Regex(Regex.Escape(_root.name)));

            Enable(source);

            Assert.IsFalse(source.IsRegistered);
        }

        [Test]
        public void Source_FindsItsWorldInItsParents() {
            var world = _root.AddComponent<TestReferenceFrameWorld>();
            var source = CreateChild(_root.transform, "Platform").gameObject.AddComponent<TestReferenceFrameSource>();

            Enable(source);

            Assert.IsTrue(source.IsRegistered);
            Assert.AreEqual(1, world.ReferenceFrames.Count);
        }

        [Test]
        public void Source_OnDisable_LeavesItsWorld() {
            var world = _root.AddComponent<TestReferenceFrameWorld>();
            var source = _root.AddComponent<TestReferenceFrameSource>();
            source.Id = new ReferenceFrameId(5);
            Enable(source);

            Disable(source);

            Assert.IsFalse(source.IsRegistered);
            Assert.IsFalse(world.ReferenceFrames.Contains(new ReferenceFrameId(5)));
            Assert.DoesNotThrow(() => source.World = world);
        }

        private static Transform CreateChild(Transform parent, string name) {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);

            return child;
        }

        private static void Enable(ReferenceFrameSource source) {
            Invoke(source, "OnEnable");
        }

        private static void Disable(ReferenceFrameSource source) {
            Invoke(source, "OnDisable");
        }

        private static void Invoke(ReferenceFrameSource source, string message) {
            typeof(ReferenceFrameSource).GetMethod(message, BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(source, null);
        }
    }
}
