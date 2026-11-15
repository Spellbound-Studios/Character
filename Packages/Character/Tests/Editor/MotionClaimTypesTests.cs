// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Character.Tests;
using Spellbound.Core.ReferenceFrames;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class MotionClaimTypesTests {
        [Test]
        public void Handoff_AtRest_HasZeroVelocityAndTheWorldFrame() {
            var pose = new Pose(new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 45f, 0f));

            var handoff = Handoff.AtRest(pose, Vector3.up);

            Assert.AreEqual(pose.position, handoff.WorldPose.position);
            Assert.AreEqual(pose.rotation, handoff.WorldPose.rotation);
            Assert.AreEqual(Vector3.zero, handoff.WorldVelocity);
            Assert.AreEqual(ReferenceFrameId.World, handoff.BoundFrame);
            Assert.AreEqual(Vector3.up, handoff.Up);
        }

        [Test]
        public void Handoff_KeepsWhatItIsGiven() {
            var pose = new Pose(new Vector3(4f, 0f, 0f), Quaternion.identity);

            var handoff = new Handoff(pose, new Vector3(0f, 0f, 2f), new ReferenceFrameId(9), Vector3.forward);

            Assert.AreEqual(pose.position, handoff.WorldPose.position);
            Assert.AreEqual(new Vector3(0f, 0f, 2f), handoff.WorldVelocity);
            Assert.AreEqual(new ReferenceFrameId(9), handoff.BoundFrame);
            Assert.AreEqual(Vector3.forward, handoff.Up);
        }

        [Test]
        public void MotionClaims_Fresh_HasNoActiveDriverAndNoClaims() {
            var claims = new MotionClaims();

            Assert.IsNull(claims.Active);
            Assert.AreEqual(0, claims.Count);
        }

        [Test]
        public void MotionClaims_Claim_RefusesAMissingDriver() {
            Assert.Throws<ArgumentNullException>(() => new MotionClaims().Claim(null));
        }

        [Test]
        public void MotionClaims_ReleaseTakesTheClaimInternally() {
            var release = typeof(MotionClaims).GetMethod("Release", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(release);
            Assert.IsTrue(release.IsAssembly);
            Assert.AreEqual(typeof(MotionClaim).MakeByRefType(), release.GetParameters()[0].ParameterType);
        }

        [Test]
        public void MotionClaim_Default_HoldsNoDriverIsNotActiveAndReleasesNothing() {
            var claim = default(MotionClaim);

            Assert.IsNull(claim.Driver);
            Assert.IsFalse(claim.IsActive);
            Assert.DoesNotThrow(() => claim.Release());
        }

        [Test]
        public void MotionDriver_HasAPrioritySupportNeedBeginAndEnd() {
            var driver = typeof(IMotionDriver);

            Assert.AreEqual(typeof(int), driver.GetProperty(nameof(IMotionDriver.Priority)).PropertyType);
            Assert.AreEqual(typeof(bool), driver.GetProperty(nameof(IMotionDriver.NeedsSupport)).PropertyType);
            Assert.AreEqual(typeof(Handoff).MakeByRefType(),
                driver.GetMethod(nameof(IMotionDriver.Begin)).GetParameters()[0].ParameterType);
            Assert.IsTrue(driver.GetMethod(nameof(IMotionDriver.Begin)).GetParameters()[0].IsIn);
            Assert.AreEqual(typeof(Handoff), driver.GetMethod(nameof(IMotionDriver.End)).ReturnType);
        }

        [Test]
        public void IMotionDriver_CustomImplementation() {
            var leaving = Handoff.AtRest(new Pose(Vector3.one, Quaternion.identity), Vector3.up);
            var custom = new TestMotionDriver(3, true) { Leaving = leaving };
            IMotionDriver driver = custom;
            var entering = new Handoff(Pose.identity, Vector3.right, new ReferenceFrameId(5), Vector3.up);

            driver.Begin(in entering);
            var ended = driver.End();

            Assert.AreEqual(3, driver.Priority);
            Assert.IsTrue(driver.NeedsSupport);
            Assert.AreEqual(new ReferenceFrameId(5), custom.Entered.BoundFrame);
            Assert.AreEqual(Vector3.right, custom.Entered.WorldVelocity);
            Assert.AreEqual(Vector3.one, ended.WorldPose.position);
            Assert.AreEqual(1, custom.BeginCount);
            Assert.AreEqual(1, custom.EndCount);
        }
    }
}
