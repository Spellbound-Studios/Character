// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core.Ground;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class ContainmentSeamTests {
        private static readonly Bounds BodyBounds = new(new Vector3(1f, 2f, 3f), new Vector3(0.5f, 2f, 0.5f));

        [Test]
        public void PhysicsContainmentSpace_IsAlwaysReadyAndEmpty() {
            IContainmentSpace space = new PhysicsContainmentSpace();

            Assert.IsTrue(space.IsReady(BodyBounds));
            Assert.AreEqual(Occupancy.Empty, space.Classify(BodyBounds));
        }

        [Test]
        public void SettingsRescueTarget_ReturnsTheAnchorUpright() {
            var settings = ScriptableObject.CreateInstance<ContainmentSettings>();
            settings.fallbackAnchor = new Vector3(4f, 50f, -6f);
            IRescueTarget target = new SettingsRescueTarget(settings);

            var pose = target.Fallback(new Vector3(100f, -300f, 7f));

            Assert.AreEqual(settings.fallbackAnchor, pose.position);
            Assert.AreEqual(Quaternion.identity, pose.rotation);
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void SettingsRescueTarget_RefusesMissingSettings() {
            Assert.Throws<ArgumentNullException>(() => new SettingsRescueTarget(null));
        }

        [Test]
        public void RescueStep_CarriesItsTargetRungAndReason() {
            var target = new Pose(new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 90f, 0f));

            var step = new RescueStep(target, RescueRung.OpenAbove, ContainmentReason.Embedded);

            Assert.AreEqual(target, step.Target);
            Assert.AreEqual(RescueRung.OpenAbove, step.Rung);
            Assert.AreEqual(ContainmentReason.Embedded, step.Reason);
        }

        [Test]
        public void ContainmentReason_NumbersAreStable() {
            Assert.AreEqual(typeof(byte), Enum.GetUnderlyingType(typeof(ContainmentReason)));
            Assert.AreEqual(0, (byte)ContainmentReason.None);
            Assert.AreEqual(1, (byte)ContainmentReason.BelowFloor);
            Assert.AreEqual(2, (byte)ContainmentReason.Embedded);
            Assert.AreEqual(3, (byte)ContainmentReason.SustainedFall);
            Assert.AreEqual(4, (byte)ContainmentReason.SupportTimeout);
        }

        [Test]
        public void RescueRung_IsOrderedFromLastSupportedToFallback() {
            Assert.AreEqual(typeof(byte), Enum.GetUnderlyingType(typeof(RescueRung)));
            Assert.AreEqual(0, (byte)RescueRung.LastSupported);
            Assert.AreEqual(1, (byte)RescueRung.OpenAbove);
            Assert.AreEqual(2, (byte)RescueRung.Fallback);
        }
    }
}
