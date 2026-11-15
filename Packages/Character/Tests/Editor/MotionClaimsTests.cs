// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Spellbound.Character.Body;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Spellbound.Character.Editor.Tests {
    public class MotionClaimsTests {
        private static Handoff Leaving(float position) =>
                new(new Pose(new Vector3(position, 0f, 0f), Quaternion.identity), new Vector3(0f, 0f, position),
                    default, Vector3.up);

        private static void AssertSameHandoff(Handoff expected, Handoff actual) {
            Assert.AreEqual(expected.WorldPose.position, actual.WorldPose.position);
            Assert.AreEqual(expected.WorldVelocity, actual.WorldVelocity);
            Assert.AreEqual(expected.BoundFrame, actual.BoundFrame);
            Assert.AreEqual(expected.Up, actual.Up);
        }

        [Test]
        public void Claim_OneDriver_IsActiveAndBeginsAtRest() {
            var claims = new MotionClaims();
            var driver = new RecordingMotionDriver(1, Leaving(1f));

            var claim = claims.Claim(driver);

            Assert.AreSame(driver, claims.Active);
            Assert.AreEqual(1, claims.Count);
            Assert.AreSame(driver, claim.Driver);
            Assert.IsTrue(claim.IsActive);
            Assert.AreEqual(1, driver.BeginCount);
            Assert.AreEqual(0, driver.EndCount);
            AssertSameHandoff(Handoff.AtRest(default, Vector3.up), driver.LastEntered);
        }

        [Test]
        public void Claim_HigherPriority_TakesOverWithTheLowerHandoff() {
            var claims = new MotionClaims();
            var lower = new RecordingMotionDriver(1, Leaving(1f));
            var higher = new RecordingMotionDriver(5, Leaving(5f));

            var lowerClaim = claims.Claim(lower);
            var higherClaim = claims.Claim(higher);

            Assert.AreSame(higher, claims.Active);
            Assert.AreEqual(2, claims.Count);
            Assert.IsTrue(higherClaim.IsActive);
            Assert.IsFalse(lowerClaim.IsActive);
            Assert.AreEqual(1, lower.EndCount);
            Assert.AreEqual(1, higher.BeginCount);
            AssertSameHandoff(Leaving(1f), higher.LastEntered);
        }

        [Test]
        public void Claim_LowerPriority_IsSuspendedWithoutCalls() {
            var claims = new MotionClaims();
            var higher = new RecordingMotionDriver(5, Leaving(5f));
            var lower = new RecordingMotionDriver(1, Leaving(1f));

            claims.Claim(higher);
            claims.Claim(lower);

            Assert.AreSame(higher, claims.Active);
            Assert.AreEqual(2, claims.Count);
            Assert.AreEqual(0, higher.EndCount);
            Assert.AreEqual(0, lower.BeginCount);
            Assert.AreEqual(0, lower.EndCount);
        }

        [Test]
        public void Release_TheHigher_ResumesTheLowerWithTheHigherHandoff() {
            var claims = new MotionClaims();
            var lower = new RecordingMotionDriver(1, Leaving(1f));
            var higher = new RecordingMotionDriver(5, Leaving(5f));
            var lowerClaim = claims.Claim(lower);
            var higherClaim = claims.Claim(higher);

            higherClaim.Release();

            Assert.AreSame(lower, claims.Active);
            Assert.AreEqual(1, claims.Count);
            Assert.IsTrue(lowerClaim.IsActive);
            Assert.AreEqual(1, higher.EndCount);
            Assert.AreEqual(2, lower.BeginCount);
            AssertSameHandoff(Leaving(5f), lower.LastEntered);
        }

        [Test]
        public void Release_TheActive_ResumesTheHighestRemaining() {
            var claims = new MotionClaims();
            var lowest = new RecordingMotionDriver(1, Leaving(1f));
            var middle = new RecordingMotionDriver(3, Leaving(3f));
            var highest = new RecordingMotionDriver(5, Leaving(5f));
            claims.Claim(lowest);
            var highestClaim = claims.Claim(highest);
            claims.Claim(middle);

            highestClaim.Release();

            Assert.AreSame(middle, claims.Active);
            Assert.AreEqual(1, middle.BeginCount);
            Assert.AreEqual(1, lowest.BeginCount);
            AssertSameHandoff(Leaving(5f), middle.LastEntered);
        }

        [Test]
        public void Claim_EqualPriority_KeepsTheFirst() {
            var claims = new MotionClaims();
            var first = new RecordingMotionDriver(2, Leaving(1f));
            var second = new RecordingMotionDriver(2, Leaving(2f));

            claims.Claim(first);
            claims.Claim(second);

            Assert.AreSame(first, claims.Active);
            Assert.AreEqual(2, claims.Count);
            Assert.AreEqual(0, first.EndCount);
            Assert.AreEqual(0, second.BeginCount);
        }

        [Test]
        public void Release_ASuspendedClaim_CallsNothing() {
            var claims = new MotionClaims();
            var lower = new RecordingMotionDriver(1, Leaving(1f));
            var higher = new RecordingMotionDriver(5, Leaving(5f));
            var lowerClaim = claims.Claim(lower);
            claims.Claim(higher);

            lowerClaim.Release();

            Assert.AreSame(higher, claims.Active);
            Assert.AreEqual(1, claims.Count);
            Assert.AreEqual(1, lower.BeginCount);
            Assert.AreEqual(1, lower.EndCount);
            Assert.AreEqual(1, higher.BeginCount);
            Assert.AreEqual(0, higher.EndCount);
        }

        [Test]
        public void Release_TheLast_LeavesNoActiveDriver() {
            var claims = new MotionClaims();
            var driver = new RecordingMotionDriver(1, Leaving(1f));
            var claim = claims.Claim(driver);

            claim.Release();

            Assert.IsNull(claims.Active);
            Assert.AreEqual(0, claims.Count);
            Assert.IsFalse(claim.IsActive);
            Assert.AreEqual(1, driver.EndCount);
        }

        [Test]
        public void Release_Twice_LogsAnErrorAndChangesNothing() {
            var claims = new MotionClaims();
            var lower = new RecordingMotionDriver(1, Leaving(1f));
            var higher = new RecordingMotionDriver(5, Leaving(5f));
            claims.Claim(lower);
            var higherClaim = claims.Claim(higher);
            higherClaim.Release();

            LogAssert.Expect(LogType.Error, new Regex("MotionClaims"));
            higherClaim.Release();

            Assert.AreSame(lower, claims.Active);
            Assert.AreEqual(1, claims.Count);
            Assert.AreEqual(2, lower.BeginCount);
            Assert.AreEqual(1, lower.EndCount);
            Assert.AreEqual(1, higher.BeginCount);
            Assert.AreEqual(1, higher.EndCount);
        }

        [Test]
        public void Claim_TheSameDriverTwice_Throws() {
            var claims = new MotionClaims();
            var driver = new RecordingMotionDriver(1, Leaving(1f));
            claims.Claim(driver);

            Assert.Throws<InvalidOperationException>(() => claims.Claim(driver));
            Assert.AreEqual(1, claims.Count);
            Assert.AreEqual(1, driver.BeginCount);
        }

        [Test]
        public void ClaimAndRelease_NoAllocation() {
            var claims = new MotionClaims();
            var lower = new RecordingMotionDriver(1, Leaving(1f));
            var higher = new RecordingMotionDriver(5, Leaving(5f));
            claims.Claim(lower).Release();
            claims.Claim(lower);
            claims.Claim(higher).Release();

            Assert.That(() => {
                var claim = claims.Claim(higher);
                claim.Release();
            }, Is.Not.AllocatingGCMemory());
        }
    }
}
