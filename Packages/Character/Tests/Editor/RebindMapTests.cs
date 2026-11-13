// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class RebindMapTests {
        private GameObject _bone;

        [SetUp]
        public void CreateBone() {
            _bone = new GameObject("Spine");
        }

        [TearDown]
        public void DestroyBone() {
            Object.DestroyImmediate(_bone);
        }

        [Test]
        public void Map_HoldsTheBonesAndTheMissingNames() {
            var bones = new Dictionary<string, Transform> { ["Spine"] = _bone.transform };

            var map = new RebindMap(bones, new[] { "Tail" });

            Assert.AreSame(_bone.transform, map.Bones["Spine"]);
            CollectionAssert.AreEqual(new[] { "Tail" }, map.Missing);
            Assert.IsFalse(map.IsComplete);
        }

        [Test]
        public void Map_IsCompleteWithNoMissingName() {
            var map = new RebindMap(new Dictionary<string, Transform> { ["Spine"] = _bone.transform },
                Array.Empty<string>());

            Assert.IsTrue(map.IsComplete);
            Assert.AreEqual(1, map.Bones.Count);
        }

        [Test]
        public void Map_CopiesItsInputs() {
            var bones = new Dictionary<string, Transform> { ["Spine"] = _bone.transform };
            var missing = new List<string> { "Tail" };

            var map = new RebindMap(bones, missing);
            bones.Clear();
            missing.Clear();

            Assert.AreEqual(1, map.Bones.Count);
            CollectionAssert.AreEqual(new[] { "Tail" }, map.Missing);
        }

        [Test]
        public void Map_ThrowsOnANullInput() {
            Assert.Throws<ArgumentNullException>(() => new RebindMap(null, Array.Empty<string>()));
            Assert.Throws<ArgumentNullException>(() => new RebindMap(new Dictionary<string, Transform>(), null));
        }
    }
}
