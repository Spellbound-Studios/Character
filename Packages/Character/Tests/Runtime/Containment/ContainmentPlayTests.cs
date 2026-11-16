// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core;
using Spellbound.Core.Observations;
using Spellbound.Core.Simulation;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Tests {
    public class ContainmentPlayTests {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private const float Tolerance = 0.001f;
        private const float FallTolerance = 0.05f;
        private const double TicksPerSecond = 50.0;

        private static readonly Vector3 StartPosition = new(0f, 5f, 0f);
        private static readonly Vector3 BelowFloorPosition = new(3f, -150f, 2f);
        private static readonly Vector3 FallbackPosition = new(12f, 3f, -4f);

        private GameObject _worldObject;
        private TestReferenceFrameWorld _world;
        private TestContainmentSpace _space;
        private TestSupportProbe _probe;
        private ContainmentSettings _settings;
        private ObservationContext _observations;

        [SetUp]
        public void CreateWorld() {
            _worldObject = new GameObject("containment play world");
            _world = _worldObject.AddComponent<TestReferenceFrameWorld>();
            _observations = CoreKit.Observations.CreateContext("containment play", null,
                TickRate.PerSecond(TicksPerSecond));
            _world.Observations = _observations;
            _space = new TestContainmentSpace();
            _probe = new TestSupportProbe();
            _settings = ScriptableObject.CreateInstance<ContainmentSettings>();
            _settings.checkStride = 1;
            _settings.fallbackAnchor = FallbackPosition;
        }

        [TearDown]
        public void DestroyWorld() {
            Object.Destroy(_worldObject);
            Object.Destroy(_settings);
            _observations.Dispose();
            _observations = null;
        }

        [UnityTest]
        public IEnumerator Containment_BelowTheFloor_RescuesToTheLastSupported() {
            _probe.Result = SupportUnder(StartPosition);
            var containment = CreateContainment(false);
            var body = containment.GetComponent<Rigidbody>();

            for (var step = 0; step < 3; step++)
                yield return Step();

            _probe.Result = GroundProbeResult.Miss;
            Teleport(body, BelowFloorPosition);

            for (var step = 0; step < 3; step++)
                yield return Step();

            Assert.AreEqual(1, containment.LastRescues.Count);
            Assert.AreEqual(RescueRung.LastSupported, containment.LastRescues[0].Step.Rung);
            Assert.AreEqual(ContainmentReason.BelowFloor, containment.LastRescues[0].Step.Reason);
            Assert.Less(Vector3.Distance(BelowFloorPosition, containment.LastRescues[0].From), Tolerance);
            Assert.Less(Vector3.Distance(StartPosition, body.position), Tolerance);
        }

        [UnityTest]
        public IEnumerator Containment_EmbeddedForFiveChecks_RescuesToOpenSpaceAbove() {
            var containment = CreateContainment(false);
            var body = containment.GetComponent<Rigidbody>();

            yield return Step();

            _space.Solids.Add(new Bounds(StartPosition, new Vector3(4f, 4f, 4f)));

            for (var step = 0; step < _settings.embeddedChecksBeforeRescue - 1; step++)
                yield return Step();

            Assert.AreEqual(0, containment.LastRescues.Count);
            Assert.AreEqual(StartPosition.y, body.position.y, Tolerance);

            yield return Step();

            Assert.AreEqual(1, containment.LastRescues.Count);
            Assert.AreEqual(RescueRung.OpenAbove, containment.LastRescues[0].Step.Rung);
            Assert.AreEqual(ContainmentReason.Embedded, containment.LastRescues[0].Step.Reason);
            Assert.AreEqual(8f, body.position.y, Tolerance);
        }

        [UnityTest]
        public IEnumerator Containment_EmbeddedForFourChecks_DoesNotRescue() {
            var containment = CreateContainment(false);
            var body = containment.GetComponent<Rigidbody>();
            var solid = new Bounds(StartPosition, new Vector3(4f, 4f, 4f));

            yield return Step();

            for (var round = 0; round < 2; round++) {
                _space.Solids.Add(solid);

                for (var step = 0; step < _settings.embeddedChecksBeforeRescue - 1; step++)
                    yield return Step();

                _space.Solids.Clear();

                yield return Step();
            }

            Assert.AreEqual(0, containment.LastRescues.Count);
            Assert.AreEqual(StartPosition.y, body.position.y, Tolerance);
        }

        [UnityTest]
        public IEnumerator Containment_InASustainedFall_RescuesToTheFallback() {
            _settings.hasFloor = false;
            _settings.sustainedFallSpeed = 1f;
            _settings.sustainedFallSeconds = 0.2f;
            var containment = CreateContainment(true);
            containment.RescueTarget = new TestRescueTarget { Pose = new Pose(FallbackPosition, Quaternion.identity) };
            var body = containment.GetComponent<Rigidbody>();

            for (var step = 0; step < 100 && containment.LastRescues.Count == 0; step++)
                yield return Step();

            Assert.AreEqual(1, containment.LastRescues.Count);
            Assert.AreEqual(RescueRung.Fallback, containment.LastRescues[0].Step.Rung);
            Assert.AreEqual(ContainmentReason.SustainedFall, containment.LastRescues[0].Step.Reason);
            Assert.Less(Vector3.Distance(FallbackPosition, body.position), FallTolerance);
            Assert.Less(containment.LastRescues[0].From.y, StartPosition.y - 0.1f);
        }

        [UnityTest]
        public IEnumerator Containment_PastTheGuardsRepairLimit_HoldsWithNoMove() {
            _settings.floorHeight = 1000f;
            var containment = CreateContainment(false);
            var body = containment.GetComponent<Rigidbody>();
            var repairs = CharacterObservations.ContainmentRepair.Defaults.Repairs;

            for (var step = 0; step < repairs + 2; step++)
                yield return Step();

            var heldAt = body.position;
            var rescues = containment.LastRescues.Count;

            for (var step = 0; step < 5; step++)
                yield return Step();

            Assert.AreEqual(repairs - 1, rescues);
            Assert.AreEqual(rescues, containment.LastRescues.Count);
            Assert.AreEqual(rescues, RescueRecords().Count);
            Assert.Less(Vector3.Distance(heldAt, body.position), Tolerance);
            Assert.Greater(body.position.y, StartPosition.y);
        }

        [UnityTest]
        public IEnumerator Containment_ASleepingBody_IsSkipped() {
            _settings.floorHeight = 1000f;
            var containment = CreateContainment(false);
            var body = containment.GetComponent<Rigidbody>();

            for (var step = 0; step < 10; step++) {
                body.Sleep();

                yield return Step();
            }

            Assert.AreEqual(0, containment.LastRescues.Count);
            Assert.Less(Vector3.Distance(StartPosition, body.position), Tolerance);
        }

        [UnityTest]
        public IEnumerator Containment_ABodyHeldByItsSupportGate_IsSkipped() {
            _settings.floorHeight = 1000f;
            var containment = CreateContainment(false, true);
            var gate = containment.GetComponent<SupportGate>();
            var body = containment.GetComponent<Rigidbody>();

            for (var step = 0; step < 10; step++)
                yield return Step();

            Assert.IsTrue(gate.IsHeld);
            Assert.AreEqual(0, containment.LastRescues.Count);
            Assert.Less(Vector3.Distance(StartPosition, body.position), Tolerance);
        }

        [UnityTest]
        public IEnumerator Containment_Rescue_RecordsTheReasonFromAndToReadBack() {
            _probe.Result = SupportUnder(StartPosition);
            var containment = CreateContainment(false);
            var body = containment.GetComponent<Rigidbody>();

            for (var step = 0; step < 3; step++)
                yield return Step();

            _probe.Result = GroundProbeResult.Miss;
            Teleport(body, BelowFloorPosition);

            for (var step = 0; step < 3; step++)
                yield return Step();

            var rescues = RescueRecords();
            Assert.AreEqual(1, rescues.Count);
            Assert.AreEqual(7, rescues[0].ValueCount);
            Assert.AreEqual((uint)ContainmentReason.BelowFloor, rescues[0].Value(0));
            Assert.AreEqual(BelowFloorPosition.x, FloatAt(rescues[0], 1), Tolerance);
            Assert.AreEqual(BelowFloorPosition.y, FloatAt(rescues[0], 2), Tolerance);
            Assert.AreEqual(BelowFloorPosition.z, FloatAt(rescues[0], 3), Tolerance);
            Assert.AreEqual(StartPosition.x, FloatAt(rescues[0], 4), Tolerance);
            Assert.AreEqual(StartPosition.y, FloatAt(rescues[0], 5), Tolerance);
            Assert.AreEqual(StartPosition.z, FloatAt(rescues[0], 6), Tolerance);
        }

        [UnityTest]
        public IEnumerator Containment_WithoutAWorld_LogsAnErrorOnceAndRescuesNothing() {
            var containment = CreateContainment(false);
            var bodyObject = containment.gameObject;
            var body = containment.GetComponent<Rigidbody>();
            bodyObject.SetActive(false);
            bodyObject.transform.SetParent(null, true);
            bodyObject.SetActive(true);

            LogAssert.Expect(LogType.Error, new Regex("found no reference frame world"));
            Teleport(body, BelowFloorPosition);

            for (var step = 0; step < 3; step++)
                yield return Step();

            Assert.AreEqual(0, containment.LastRescues.Count);
            Assert.Less(Vector3.Distance(BelowFloorPosition, body.position), Tolerance);
            Assert.AreEqual(0, RescueRecords().Count);

            Object.Destroy(bodyObject);
        }

        private Containment CreateContainment(bool useGravity, bool withHeldGate = false) {
            var bodyObject = new GameObject("contained body");
            bodyObject.SetActive(false);
            bodyObject.transform.SetParent(_worldObject.transform, false);
            bodyObject.transform.position = StartPosition;
            bodyObject.AddComponent<BoxCollider>();
            var body = bodyObject.AddComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.None;
            body.useGravity = useGravity;

            if (withHeldGate) {
                var gate = bodyObject.AddComponent<SupportGate>();
                typeof(SupportGate).GetField("settings", Private).SetValue(gate, _settings);
                gate.Space = new TestContainmentSpace { Ready = false };
            }

            var containment = bodyObject.AddComponent<Containment>();
            typeof(Containment).GetField("settings", Private).SetValue(containment, _settings);

            if (withHeldGate)
                typeof(Containment).GetField("skipKinematic", Private).SetValue(containment, false);

            containment.Space = _space;
            containment.SupportProbe = _probe;
            bodyObject.SetActive(true);

            return containment;
        }

        private static GroundProbeResult SupportUnder(Vector3 position) =>
                new(true, 0.5f, new Vector3(0f, 1f, 0f), position - new Vector3(0f, 0.5f, 0f));

        private static void Teleport(Rigidbody body, Vector3 position) {
            body.position = position;
            body.transform.position = position;
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
