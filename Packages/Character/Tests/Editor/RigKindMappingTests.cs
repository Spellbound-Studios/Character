// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEditor;

namespace Spellbound.Character.Editor.Tests {
    public class RigKindMappingTests {
        [Test]
        public void ToAnimationType_Humanoid_IsHuman() {
            Assert.AreEqual(ModelImporterAnimationType.Human, RigKindMapping.ToAnimationType(RigKind.Humanoid));
        }

        [Test]
        public void ToAnimationType_Generic_IsGeneric() {
            Assert.AreEqual(ModelImporterAnimationType.Generic, RigKindMapping.ToAnimationType(RigKind.Generic));
        }

        [Test]
        public void TryFromAnimationType_Human_IsHumanoid() {
            Assert.IsTrue(RigKindMapping.TryFromAnimationType(ModelImporterAnimationType.Human, out var kind));
            Assert.AreEqual(RigKind.Humanoid, kind);
        }

        [Test]
        public void TryFromAnimationType_Generic_IsGeneric() {
            Assert.IsTrue(RigKindMapping.TryFromAnimationType(ModelImporterAnimationType.Generic, out var kind));
            Assert.AreEqual(RigKind.Generic, kind);
        }

        [TestCase(ModelImporterAnimationType.Legacy)]
        [TestCase(ModelImporterAnimationType.None)]
        public void TryFromAnimationType_LegacyAndNone_AreRefused(ModelImporterAnimationType type) {
            Assert.IsFalse(RigKindMapping.TryFromAnimationType(type, out _));
        }
    }
}
