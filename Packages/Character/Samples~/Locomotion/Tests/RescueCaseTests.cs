// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core.Ground;
using Spellbound.Core.Observations;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Samples.Locomotion.Tests {
    public class RescueCaseTests {
        private const int SettleSteps = 40;
        private const int StepsPerSecond = 50;
        private const int TwoSecondsOfSteps = 2 * StepsPerSecond;
        private const float GroundEdge = 20f;
        private const float BlockTop = 3f;
        private const float HoldTolerance = 0.001f;
        private const float MarkerTolerance = 0.01f;
        private const float RescueTolerance = 0.25f;
        private const float StandingTolerance = 0.05f;

        private static readonly Vector3 PitEdgeStart = new(18.5f, 0.05f, 0f);
        private static readonly Vector3 BlockCentre = new(-8f, 1.25f, 8f);
        private static readonly Vector3 BlockSize = new(3f, 3.5f, 3f);
        private static readonly Vector3 EmbeddedStart = new(-8f, 0.05f, 8f);
        private static readonly Vector3 HoldStart = new(4f, 2f, -6f);
        private static readonly Vector3 TimeoutStart = new(3f, 2f, 3f);
        private static readonly Vector3 SpawnMarkerPosition = new(-5f, 0.05f, -5f);

        private readonly List<Object> _created = new();

        private TestGym _gym;
        private ScriptedLocomotionInput _input;
        private LocomotionCharacter _prefab;
        private ContainmentSettings _settings;
        private GymContainmentSpace _space;
        private Transform _spawnMarker;

        [SetUp]
        public void CreateGym() {
            _prefab = TestGym.FindImportedPrefab();
            _settings = FindImportedSettings();
            Assert.IsNotNull(_prefab, "The imported LocomotionCharacter prefab was not found.");
            Assert.IsNotNull(_settings, "The imported GymContainmentSettings asset was not found.");

            _gym = new TestGym();
            _input = new ScriptedLocomotionInput();
            _space = new GymContainmentSpace();
            _spawnMarker = new GameObject("spawn marker").transform;
            _spawnMarker.position = SpawnMarkerPosition;
            _created.Add(_spawnMarker.gameObject);
            _gym.World.Space = _space;
            _gym.World.RescueTarget = new GymRescueTarget(_spawnMarker);
        }

        [TearDown]
        public void DestroyGym() {
            _gym.Dispose();

            foreach (var created in _created) {
                if (created != null)
                    Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        [UnityTest]
        public IEnumerator Character_WalkingOffIntoThePit_IsRescuedToTheLastSupportedPositionWithinTwoSeconds() {
            CreatePit();
            var character = CreateCharacter(_input, PitEdgeStart);
            var containment = character.GetComponent<Containment>();
            var body = character.GetComponent<Rigidbody>();

            yield return TestGym.WaitFixedSteps(SettleSteps);

            var standingHeight = body.position.y;
            _input.Move = Vector2.right;

            yield return TestGym.WaitFixedSteps(TwoSecondsOfSteps, () => RescueRecords().Count > 0);

            _input.Move = Vector2.zero;
            var records = RescueRecords();

            Assert.AreEqual(1, records.Count, "The character was not rescued from the pit within two seconds.");
            Assert.AreEqual(ContainmentReason.BelowFloor, ReasonOf(records[0]));
            Assert.AreEqual(RescueRung.LastSupported, containment.LastRescues[0].Step.Rung);
            Assert.Less(FromOf(records[0]).y, _settings.floorHeight);
            Assert.LessOrEqual(ToOf(records[0]).x, GroundEdge, $"The rescue went to {ToOf(records[0])}.");
            Assert.GreaterOrEqual(ToOf(records[0]).y, LowestRescueHeight(character, standingHeight),
                $"The rescue went to {ToOf(records[0])}.");
            Assert.Less(Vector3.Distance(ToOf(records[0]), body.position), RescueTolerance,
                $"The record names {ToOf(records[0])} but the character stands at {body.position}.");

            yield return TestGym.WaitFixedSteps(TwoSecondsOfSteps);

            AssertStandingOnTheGround(character, standingHeight);
        }

        [UnityTest]
        public IEnumerator Character_StartedInsideTheSolidBlock_IsRescuedToOpenAirAboveWithinTheEmbeddedChecks() {
            CreateSolidBlock();
            var character = CreateCharacter(_input, EmbeddedStart);
            var containment = character.GetComponent<Containment>();
            var capsule = character.GetComponent<CapsuleCollider>();
            var strides = _settings.embeddedChecksBeforeRescue * _settings.checkStride;

            yield return TestGym.WaitFixedSteps(strides + 1, () => RescueRecords().Count > 0);

            var records = RescueRecords();

            Assert.AreEqual(1, records.Count,
                $"The character was not rescued within {_settings.embeddedChecksBeforeRescue} strides.");
            Assert.AreEqual(ContainmentReason.Embedded, ReasonOf(records[0]));
            Assert.AreEqual(RescueRung.OpenAbove, containment.LastRescues[0].Step.Rung);
            Assert.Greater(ToOf(records[0]).y, BlockTop, $"The rescue went to {ToOf(records[0])}.");

            yield return new WaitForFixedUpdate();

            Assert.AreEqual(Occupancy.Empty, _space.Classify(capsule.bounds),
                $"The character's bounds {capsule.bounds} are still inside the block.");
        }

        [UnityTest]
        public IEnumerator Character_OverASpaceNotReadyForTwoSeconds_IsHeldStillThenFallsWhenReady() {
            _space.Ready = false;
            var character = CreateCharacter(_input, HoldStart);
            var gate = character.GetComponent<SupportGate>();
            var body = character.GetComponent<Rigidbody>();

            yield return TestGym.WaitFixedSteps(TwoSecondsOfSteps);

            Assert.IsTrue(gate.IsHeld, "The gate did not hold the character over a space that is not ready.");
            Assert.Less(Vector3.Distance(HoldStart, body.position), HoldTolerance,
                $"The held character moved from {HoldStart} to {body.position}.");

            _space.Ready = true;

            yield return TestGym.WaitFixedSteps(TwoSecondsOfSteps, () => character.Body.Support.HasHit);
            yield return TestGym.WaitFixedSteps(SettleSteps);

            Assert.IsFalse(gate.IsHeld, "The gate still holds the character after the space turned ready.");
            Assert.IsFalse(body.isKinematic, "The character's body stayed kinematic after the hold.");
            Assert.IsTrue(character.Body.Support.HasHit, $"The character did not land, it stands at {body.position}.");
            Assert.Less(body.position.y, HoldStart.y - 1f,
                $"The character did not fall, it stands at {body.position}.");
            Assert.AreEqual(0, RescueRecords().Count, "A hold inside the timeout recorded a rescue.");
        }

        [UnityTest]
        public IEnumerator Character_HeldPastTheTimeout_LandsAtTheSpawnMarker() {
            _space.Ready = false;
            var character = CreateCharacter(_input, TimeoutStart);
            var body = character.GetComponent<Rigidbody>();
            var timeoutSteps = Mathf.CeilToInt((_settings.supportHoldTimeoutSeconds + 1f) * StepsPerSecond);

            yield return TestGym.WaitFixedSteps(timeoutSteps, () => RescueRecords().Count > 0);

            var records = RescueRecords();

            Assert.AreEqual(1, records.Count, "The hold did not time out into a rescue.");
            Assert.AreEqual(ContainmentReason.SupportTimeout, ReasonOf(records[0]));
            Assert.Less(Vector3.Distance(TimeoutStart, FromOf(records[0])), HoldTolerance);
            Assert.Less(Vector3.Distance(SpawnMarkerPosition, ToOf(records[0])), MarkerTolerance);
            Assert.Less(Vector3.Distance(SpawnMarkerPosition, body.position), MarkerTolerance,
                $"The character stands at {body.position}, not at the spawn marker.");
        }

        [UnityTest]
        public IEnumerator Rescues_OfTwoCharactersInOneGym_ReadBackWithTheirReasonsSubjectsAndPositions() {
            CreatePit();
            CreateSolidBlock();
            var walker = CreateCharacter(_input, PitEdgeStart);
            var embedded = CreateCharacter(new ScriptedLocomotionInput(), EmbeddedStart);
            var walkerSubject = EntityId.ToULong(walker.GetComponent<Containment>().GetEntityId());
            var embeddedSubject = EntityId.ToULong(embedded.GetComponent<Containment>().GetEntityId());

            yield return TestGym.WaitFixedSteps(SettleSteps);

            var standingHeight = walker.GetComponent<Rigidbody>().position.y;
            _input.Move = Vector2.right;

            yield return TestGym.WaitFixedSteps(TwoSecondsOfSteps, () => RescueRecords().Count >= 2);

            _input.Move = Vector2.zero;
            var records = RescueRecords();

            Assert.AreEqual(2, records.Count, "Two rescues were expected in the gym's records.");

            var walkerRecord = records.Find(record => record.Subject == walkerSubject);
            var embeddedRecord = records.Find(record => record.Subject == embeddedSubject);

            Assert.AreEqual(walkerSubject, walkerRecord.Subject, "No record names the walking character.");
            Assert.AreEqual(embeddedSubject, embeddedRecord.Subject, "No record names the embedded character.");
            Assert.AreEqual(ContainmentReason.BelowFloor, ReasonOf(walkerRecord));
            Assert.AreEqual(ContainmentReason.Embedded, ReasonOf(embeddedRecord));
            Assert.Less(Vector3.Distance(EmbeddedStart, FromOf(embeddedRecord)), RescueTolerance);
            Assert.Greater(ToOf(embeddedRecord).y, BlockTop);
            Assert.Less(FromOf(walkerRecord).y, _settings.floorHeight);
            Assert.LessOrEqual(ToOf(walkerRecord).x, GroundEdge);
            Assert.GreaterOrEqual(ToOf(walkerRecord).y, LowestRescueHeight(walker, standingHeight),
                $"The walker's rescue went to {ToOf(walkerRecord)}.");

            yield return TestGym.WaitFixedSteps(TwoSecondsOfSteps);

            AssertStandingOnTheGround(walker, standingHeight);
        }

        private static float LowestRescueHeight(LocomotionCharacter character, float standingHeight) =>
                standingHeight - character.Body.Capsule.CapsuleFloatData.RideHeight;

        private static void AssertStandingOnTheGround(LocomotionCharacter character, float standingHeight) {
            var position = character.GetComponent<Rigidbody>().position;

            Assert.LessOrEqual(Mathf.Abs(position.y - standingHeight), StandingTolerance,
                $"The rescued character settled at {position}, not at the standing height {standingHeight}.");
            Assert.IsTrue(character.Body.Support.HasHit, $"The rescued character is not supported at {position}.");
        }

        private LocomotionCharacter CreateCharacter(ILocomotionInput input, Vector3 position) {
            var character = _gym.CreateCharacterFromPrefab(_prefab, input);
            var body = character.GetComponent<Rigidbody>();
            body.position = position;
            character.transform.position = position;
            _gym.World.Admit(character.gameObject);

            return character;
        }

        private void CreatePit() {
            var pitFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pitFloor.name = "pit floor";
            pitFloor.transform.position = new Vector3(GroundEdge + 6f, _settings.floorHeight - 2f, 0f);
            pitFloor.transform.localScale = new Vector3(12f, 1f, 40f);
            _created.Add(pitFloor);
        }

        private void CreateSolidBlock() {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = "solid block";
            Object.DestroyImmediate(block.GetComponent<Collider>());
            block.transform.position = BlockCentre;
            block.transform.localScale = BlockSize;
            _created.Add(block);

            var surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = "solid block surface";
            surface.GetComponent<Renderer>().enabled = false;
            surface.transform.position = new Vector3(BlockCentre.x, BlockTop - 0.1f, BlockCentre.z);
            surface.transform.localScale = new Vector3(BlockSize.x, 0.2f, BlockSize.z);
            _created.Add(surface);

            _space.Solids.Add(new Bounds(BlockCentre, BlockSize));
        }

        private List<RecordedOccurrence> RescueRecords() {
            var records = new List<RecordedOccurrence>();
            _gym.World.Observations.ReadRecords(records);
            records.RemoveAll(record => record.KeyHash != CharacterObservations.ContainmentRescue.Key.Hash);

            return records;
        }

        private static ContainmentReason ReasonOf(RecordedOccurrence record) => (ContainmentReason)record.Value(0);

        private static Vector3 FromOf(RecordedOccurrence record) =>
                new(FloatAt(record, 1), FloatAt(record, 2), FloatAt(record, 3));

        private static Vector3 ToOf(RecordedOccurrence record) =>
                new(FloatAt(record, 4), FloatAt(record, 5), FloatAt(record, 6));

        private static float FloatAt(RecordedOccurrence record, int index) =>
                BitConverter.Int32BitsToSingle((int)record.Value(index));

        private static ContainmentSettings FindImportedSettings() {
#if UNITY_EDITOR
            var guids = UnityEditor.AssetDatabase.FindAssets("GymContainmentSettings t:ContainmentSettings",
                new[] { "Assets" });

            foreach (var guid in guids) {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                if (path.EndsWith("/Locomotion/GymContainmentSettings.asset"))
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<ContainmentSettings>(path);
            }
#endif
            return null;
        }
    }
}
