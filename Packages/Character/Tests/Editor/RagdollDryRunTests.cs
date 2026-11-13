// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Spellbound.Character.Rig;

namespace Spellbound.Character.Editor.Tests {
    public class RagdollDryRunTests {
        private static readonly RagdollDryRunBone[] TwoBones = {
            new("Hips", "Hips", RagdollColliderShape.Box, 0.5f),
            new("Spine", "Hips/Spine", RagdollColliderShape.Capsule, 0.3f)
        };

        [Test]
        public void Bone_HoldsItsFields() {
            var bone = new RagdollDryRunBone("Head", "Hips/Spine/Head", RagdollColliderShape.Sphere, 0.08f);

            Assert.AreEqual("Head", bone.Name);
            Assert.AreEqual("Hips/Spine/Head", bone.Path);
            Assert.AreEqual(RagdollColliderShape.Sphere, bone.Shape);
            Assert.AreEqual(0.08f, bone.MassShare);
        }

        [Test]
        public void WouldBuild_WhenTheErrorIsEmpty() {
            var dryRun = new RagdollDryRun(null, TwoBones, Array.Empty<string>());

            Assert.AreEqual(string.Empty, dryRun.Error);
            Assert.IsTrue(dryRun.WouldBuild);
            Assert.AreEqual(2, dryRun.Bones.Count);
            Assert.IsEmpty(dryRun.Missing);
        }

        [Test]
        public void ToText_ListsEachBoneWithItsShapeShareAndPath() {
            var dryRun = new RagdollDryRun(string.Empty, TwoBones, Array.Empty<string>());

            Assert.AreEqual("would build 2 bones\nHips Box 0.50 Hips\nSpine Capsule 0.30 Hips/Spine", dryRun.ToText());
        }

        [Test]
        public void ToText_StartsWithTheErrorAndEndsWithTheMissingNames() {
            var dryRun = new RagdollDryRun("Bone 2 (Head) could not be resolved.", Array.Empty<RagdollDryRunBone>(),
                new[] { "Neck", "Head" });

            Assert.IsFalse(dryRun.WouldBuild);
            CollectionAssert.AreEqual(new[] { "Neck", "Head" }, dryRun.Missing);
            Assert.AreEqual("cannot build: Bone 2 (Head) could not be resolved.\nmissing: Neck, Head", dryRun.ToText());
        }

        [Test]
        public void ToText_WritesTheShareInTheInvariantCulture() {
            var culture = Thread.CurrentThread.CurrentCulture;

            try {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                var dryRun = new RagdollDryRun(string.Empty, TwoBones, Array.Empty<string>());

                StringAssert.Contains("Hips Box 0.50 Hips", dryRun.ToText());
            }
            finally {
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        [Test]
        public void DryRun_ThrowsOnANullList() {
            Assert.Throws<ArgumentNullException>(() => new RagdollDryRun(string.Empty, null, Array.Empty<string>()));
            Assert.Throws<ArgumentNullException>(() => new RagdollDryRun(string.Empty, TwoBones, null));
        }
    }
}
