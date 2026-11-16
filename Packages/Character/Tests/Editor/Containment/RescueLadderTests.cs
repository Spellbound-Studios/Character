// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Character.Tests;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class RescueLadderTests {
        private static readonly Vector3 Upward = new(0f, 1f, 0f);
        private static readonly Vector3 Sideways = new(1f, 0f, 0f);
        private static readonly Vector3 BodyExtents = new(0.5f, 0.5f, 0.5f);
        private static readonly Pose FallbackPose = new(new Vector3(40f, 60f, -20f), Quaternion.identity);

        private ContainmentSettings _settings;
        private TestContainmentSpace _space;
        private TestRescueTarget _fallback;
        private RescueLadder _ladder;

        [SetUp]
        public void CreateLadder() {
            _settings = ScriptableObject.CreateInstance<ContainmentSettings>();
            _space = new TestContainmentSpace();
            _fallback = new TestRescueTarget { Pose = FallbackPose };
            _ladder = new RescueLadder(_settings, _space, _fallback);
        }

        [TearDown]
        public void DestroySettings() {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void Constructor_RefusesMissingArguments() {
            Assert.Throws<ArgumentNullException>(() => new RescueLadder(null, _space, _fallback));
            Assert.Throws<ArgumentNullException>(() => new RescueLadder(_settings, null, _fallback));
            Assert.Throws<ArgumentNullException>(() => new RescueLadder(_settings, _space, null));
        }

        [Test]
        public void Resolve_LastSupportedIsEmpty_ReturnsTheLastSupportedPosition() {
            var supported = new Vector3(3f, 10f, 4f);
            _space.Solids.Add(new Bounds(Vector3.zero, new Vector3(10f, 4f, 10f)));
            _ladder.RememberSupported(supported);

            var step = _ladder.Resolve(Vector3.zero, Upward, BodyExtents, ContainmentReason.Embedded);

            Assert.AreEqual(RescueRung.LastSupported, step.Rung);
            Assert.AreEqual(ContainmentReason.Embedded, step.Reason);
            AssertNear(supported, step.Target.position);
        }

        [Test]
        public void Resolve_LastSupportedIsSolid_ClimbsToOpenAirAboveInSteps() {
            _space.Solids.Add(new Bounds(new Vector3(0f, 0.5f, 0f), new Vector3(10f, 3f, 10f)));
            _ladder.RememberSupported(new Vector3(0f, 1f, 0f));

            var step = _ladder.Resolve(Vector3.zero, Upward, BodyExtents, ContainmentReason.Embedded);

            Assert.AreEqual(RescueRung.OpenAbove, step.Rung);
            AssertNear(new Vector3(0f, 3f, 0f), step.Target.position);
        }

        [Test]
        public void Resolve_NothingRemembered_ClimbsAlongTheUpItIsGiven() {
            _space.Solids.Add(new Bounds(Vector3.zero, new Vector3(10f, 10f, 10f)));

            var step = _ladder.Resolve(Vector3.zero, Sideways, BodyExtents, ContainmentReason.BelowFloor);

            Assert.AreEqual(RescueRung.OpenAbove, step.Rung);
            AssertNear(new Vector3(6f, 0f, 0f), step.Target.position);
        }

        [Test]
        public void Resolve_SearchesInTheSettingsStepSize() {
            _settings.openSearchStepMetres = 1.5f;
            _space.Solids.Add(new Bounds(new Vector3(0f, 0.8f, 0f), new Vector3(10f, 3.6f, 10f)));

            var step = _ladder.Resolve(Vector3.zero, Upward, BodyExtents, ContainmentReason.Embedded);

            Assert.AreEqual(RescueRung.OpenAbove, step.Rung);
            AssertNear(new Vector3(0f, 4.5f, 0f), step.Target.position);
        }

        [Test]
        public void Resolve_OpenAirExactlyAtTheRange_IsFound() {
            _settings.maxOpenSearchMetres = 3f;
            _space.Solids.Add(new Bounds(new Vector3(0f, 0.5f, 0f), new Vector3(10f, 3f, 10f)));

            var step = _ladder.Resolve(Vector3.zero, Upward, BodyExtents, ContainmentReason.Embedded);

            Assert.AreEqual(RescueRung.OpenAbove, step.Rung);
            AssertNear(new Vector3(0f, 3f, 0f), step.Target.position);
        }

        [Test]
        public void Resolve_NoOpenAirWithinTheRange_FallsBackToTheAnchor() {
            _settings.maxOpenSearchMetres = 2f;
            _space.Solids.Add(new Bounds(Vector3.zero, new Vector3(10f, 40f, 10f)));
#if SPELLBOUND_LOG_WARNING
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("RescueLadder"));
#endif

            var step = _ladder.Resolve(Vector3.zero, Upward, BodyExtents, ContainmentReason.Embedded);

            Assert.AreEqual(RescueRung.Fallback, step.Rung);
            Assert.AreEqual(ContainmentReason.Embedded, step.Reason);
            AssertNear(FallbackPose.position, step.Target.position);
        }

        [Test]
        public void Resolve_SustainedFall_GoesStraightToTheFallbackWithoutAWarning() {
            _ladder.RememberSupported(new Vector3(3f, 10f, 4f));

            var step = _ladder.Resolve(Vector3.zero, Upward, BodyExtents, ContainmentReason.SustainedFall);

            Assert.AreEqual(RescueRung.Fallback, step.Rung);
            Assert.AreEqual(ContainmentReason.SustainedFall, step.Reason);
            AssertNear(FallbackPose.position, step.Target.position);
            LogAssert.NoUnexpectedReceived();
        }

        private static void AssertNear(Vector3 expected, Vector3 actual) {
            Assert.Less(Vector3.Distance(expected, actual), 0.001f, $"expected {expected}, got {actual}");
        }
    }
}
