// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core;
using Spellbound.Core.Observations;
using Spellbound.Core.Simulation;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Tests {
    public class SupportGatePlayTests {
        private const float Tolerance = 0.001f;
        private const int HeldSteps = 50;
        private const double TicksPerSecond = 50.0;

        private static readonly Vector3 StartPosition = new(0f, 5f, 0f);
        private static readonly Vector3 FallbackPosition = new(12f, 3f, -4f);

        private GameObject _worldObject;
        private TestReferenceFrameWorld _world;
        private TestContainmentSpace _space;
        private ContainmentSettings _settings;
        private ObservationContext _observations;

        [SetUp]
        public void CreateWorld() {
            _worldObject = new GameObject("support gate play world");
            _world = _worldObject.AddComponent<TestReferenceFrameWorld>();
            _space = new TestContainmentSpace { Ready = false };
            _settings = ScriptableObject.CreateInstance<ContainmentSettings>();
        }

        [TearDown]
        public void DestroyWorld() {
            Object.Destroy(_worldObject);
            Object.Destroy(_settings);
            _observations?.Dispose();
            _observations = null;
        }

        [UnityTest]
        public IEnumerator Gate_OverANotReadySpace_HoldsTheBodyAtItsHeightWithGravityOff() {
            var gate = CreateGate(null);
            var body = gate.GetComponent<Rigidbody>();

            for (var step = 0; step < HeldSteps; step++)
                yield return Step();

            Assert.IsTrue(gate.IsHeld);
            Assert.IsTrue(body.isKinematic);
            Assert.IsFalse(body.useGravity);
            Assert.AreEqual(StartPosition.y, body.position.y, Tolerance);
            Assert.AreEqual(1, gate.Motion.Count);
            Assert.AreEqual(_settings.rescuePriority, gate.Motion.Active.Priority);
            Assert.IsFalse(gate.Motion.Active.NeedsSupport);
        }

        [UnityTest]
        public IEnumerator Gate_SpaceTurnsReady_RestoresTheBodyAndReleasesTheClaim() {
            var gate = CreateGate(null);
            var body = gate.GetComponent<Rigidbody>();

            for (var step = 0; step < 5; step++)
                yield return Step();

            Assert.IsTrue(gate.IsHeld);
            _space.Ready = true;

            for (var step = 0; step < 20; step++)
                yield return Step();

            Assert.IsFalse(gate.IsHeld);
            Assert.IsFalse(body.isKinematic);
            Assert.IsTrue(body.useGravity);
            Assert.Less(body.position.y, StartPosition.y - 0.1f);
            Assert.AreEqual(0, gate.Motion.Count);
            Assert.IsNull(gate.Motion.Active);
        }

        [UnityTest]
        public IEnumerator Gate_HeldPastTheTimeout_MovesTheBodyToTheFallbackAndRecordsOneRescue() {
            _observations = CoreKit.Observations.CreateContext("support gate timeout", null,
                TickRate.PerSecond(TicksPerSecond));
            _world.Observations = _observations;
            _settings.supportHoldTimeoutSeconds = 0.2f;
            var gate = CreateGate(null);
            gate.RescueTarget = new TestRescueTarget { Pose = new Pose(FallbackPosition, Quaternion.identity) };
            var body = gate.GetComponent<Rigidbody>();

            for (var step = 0; step < 30 && Vector3.Distance(body.position, FallbackPosition) > Tolerance; step++)
                yield return Step();

            for (var step = 0; step < 3; step++)
                yield return Step();

            Assert.Less(Vector3.Distance(FallbackPosition, body.position), Tolerance);
            Assert.Less(Vector3.Distance(FallbackPosition, gate.transform.position), Tolerance);
            Assert.IsTrue(gate.IsHeld);

            var rescues = RescueRecords();
            Assert.AreEqual(1, rescues.Count);
            Assert.AreEqual(7, rescues[0].ValueCount);
            Assert.AreEqual((uint)ContainmentReason.SupportTimeout, rescues[0].Value(0));
            Assert.AreEqual(StartPosition.y, FloatAt(rescues[0], 2), Tolerance);
            Assert.AreEqual(FallbackPosition.x, FloatAt(rescues[0], 4), Tolerance);
            Assert.AreEqual(FallbackPosition.y, FloatAt(rescues[0], 5), Tolerance);
            Assert.AreEqual(FallbackPosition.z, FloatAt(rescues[0], 6), Tolerance);
        }

        [UnityTest]
        public IEnumerator Gate_ActiveDriverNeedsNoSupport_NeverHolds() {
            var motion = new MotionClaims();
            motion.Claim(new TestMotionDriver(10, false));
            var gate = CreateGate(motion);
            var body = gate.GetComponent<Rigidbody>();

            for (var step = 0; step < 20; step++) {
                yield return Step();

                Assert.IsFalse(gate.IsHeld, $"held at step {step}");
            }

            Assert.IsFalse(body.isKinematic);
            Assert.IsTrue(body.useGravity);
            Assert.AreEqual(1, motion.Count);
        }

        [UnityTest]
        public IEnumerator Gate_ActiveDriverNeedsSupport_IsSuspendedWhileHeldAndResumedAfter() {
            var motion = new MotionClaims();
            var driver = new TestMotionDriver(10, true);
            motion.Claim(driver);
            var gate = CreateGate(motion);

            for (var step = 0; step < 5; step++)
                yield return Step();

            Assert.IsTrue(gate.IsHeld);
            Assert.AreNotSame(driver, motion.Active);
            Assert.AreEqual(1, driver.EndCount);
            _space.Ready = true;

            yield return Step();
            yield return Step();

            Assert.IsFalse(gate.IsHeld);
            Assert.AreSame(driver, motion.Active);
            Assert.AreEqual(2, driver.BeginCount);
            Assert.AreEqual(StartPosition.y, driver.Entered.WorldPose.position.y, 0.05f);
        }

        [UnityTest]
        public IEnumerator Gate_OnADiscardContext_HoldsAndReleasesWithoutError() {
            _world.Observations = ObservationContext.Discard;
            _settings.supportHoldTimeoutSeconds = 0f;
            var gate = CreateGate(null);
            var body = gate.GetComponent<Rigidbody>();

            for (var step = 0; step < 10; step++)
                yield return Step();

            Assert.IsTrue(gate.IsHeld);
            Assert.AreEqual(StartPosition.y, body.position.y, Tolerance);
            _space.Ready = true;

            yield return Step();
            yield return Step();

            Assert.IsFalse(gate.IsHeld);
            Assert.IsFalse(body.isKinematic);
            LogAssert.NoUnexpectedReceived();
        }

        private SupportGate CreateGate(MotionClaims motion) {
            var bodyObject = new GameObject("gated body");
            bodyObject.SetActive(false);
            bodyObject.transform.SetParent(_worldObject.transform, false);
            bodyObject.transform.position = StartPosition;
            bodyObject.AddComponent<BoxCollider>();
            var body = bodyObject.AddComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.None;
            var gate = bodyObject.AddComponent<SupportGate>();
            typeof(SupportGate).GetField("settings", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(gate, _settings);
            gate.Space = _space;

            if (motion != null)
                gate.Motion = motion;

            bodyObject.SetActive(true);

            return gate;
        }

        private IEnumerator Step() {
            _world.CurrentTick = new Tick(_world.CurrentTick.Value + 1);

            yield return new WaitForFixedUpdate();
        }

        private List<RecordedOccurrence> RescueRecords() {
            var records = new List<RecordedOccurrence>();
            _observations.ReadRecords(records);
            records.RemoveAll(record => record.KeyHash != CharacterObservations.ContainmentRescue.Key.Hash);

            return records;
        }

        private static float FloatAt(RecordedOccurrence record, int index) =>
                BitConverter.Int32BitsToSingle((int)record.Value(index));
    }
}
