// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Character.Samples.Locomotion.Tests {
    public class RideTests {
        private const float FollowTolerance = 0.02f;
        private const float DriftTolerance = 0.25f;
        private const int SettleSteps = 40;
        private const int RideSteps = 100;
        private const int CrossingSteps = 200;

        private static readonly Vector3 PlatformPosition = new(0f, 1f, 0f);
        private static readonly Vector3 PlatformRun = new(12f, 0f, 0f);

        private TestGym _gym;
        private ScriptedLocomotionInput _input;

        [SetUp]
        public void CreateGym() {
            _gym = new TestGym();
            _input = new ScriptedLocomotionInput();
        }

        [TearDown]
        public void DestroyGym() => _gym.Dispose();

        [UnityTest]
        public IEnumerator Character_StandingStillOnTheMovingPlatform_FollowsItWithinTwoCentimetres() {
            var platform = _gym.CreatePlatform(PlatformPosition, PlatformRun);
            var character = _gym.CreateCharacter(_input, StandingPoint(0f));

            yield return SettleOn(character);

            var characterStart = character.Body.Rigidbody.position;
            var platformStart = platform.Body.position;

            yield return TestGym.WaitFixedSteps(RideSteps);

            var characterTravel = Planar(character.Body.Rigidbody.position - characterStart);
            var platformTravel = Planar(platform.Body.position - platformStart);

            Assert.Greater(platformTravel.magnitude, 2f, "The platform did not move.");
            Assert.LessOrEqual(Vector3.Distance(characterTravel, platformTravel), FollowTolerance,
                $"The character travelled {characterTravel} while the platform travelled {platformTravel}.");
        }

        [UnityTest]
        public IEnumerator Character_WalkingAcrossTheMovingPlatform_ReachesTheFarEdge() {
            var platform = _gym.CreatePlatform(PlatformPosition, PlatformRun);
            var nearEdge = -TestGym.PlatformSize.z * 0.5f + 0.5f;
            var farEdge = TestGym.PlatformSize.z * 0.5f - 0.5f;
            var character = _gym.CreateCharacter(_input, StandingPoint(nearEdge));

            yield return SettleOn(character);

            var startOffset = Planar(character.Body.Rigidbody.position - platform.Body.position);
            _input.Move = Vector2.up;

            yield return TestGym.WaitFixedSteps(CrossingSteps,
                () => character.Body.Rigidbody.position.z - platform.Body.position.z >= farEdge);

            var offset = Planar(character.Body.Rigidbody.position - platform.Body.position);

            Assert.GreaterOrEqual(offset.z, farEdge, $"The character stopped at {offset.z} on the platform.");
            Assert.LessOrEqual(Mathf.Abs(offset.x - startOffset.x), DriftTolerance,
                $"The character drifted {offset.x - startOffset.x} across the platform's run.");
            Assert.IsTrue(character.Machine.IsInState<WalkState>(), TestGym.Describe("WalkState", character));
        }

        private static Vector3 StandingPoint(float depth) =>
                PlatformPosition + new Vector3(0f, TestGym.PlatformSize.y * 0.5f + 0.05f, depth);

        private static IEnumerator SettleOn(LocomotionCharacter character) {
            yield return TestGym.WaitFixedSteps(SettleSteps);

            Assert.IsTrue(character.Machine.IsInState<WalkState>(),
                "The character did not stand on the platform: " + TestGym.Describe("WalkState", character));
        }

        private static Vector3 Planar(Vector3 vector) => new(vector.x, 0f, vector.z);
    }
}
