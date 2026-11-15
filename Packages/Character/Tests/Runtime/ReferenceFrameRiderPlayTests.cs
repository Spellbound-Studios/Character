// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core;
using Spellbound.Core.Observations;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Character.Tests {
    public class ReferenceFrameRiderPlayTests {
        private const float Tolerance = 0.05f;
        private const float BodyHeight = 0.5f;
        private const float LiftMetres = 5f;

        private static readonly Vector3 PlatformVelocity = new(1f, 0f, 0f);

        private GameObject _worldObject;
        private TestReferenceFrameWorld _world;
        private ObservationContext _observations;

        [SetUp]
        public void CreateWorld() {
            _worldObject = new GameObject("reference frame rider play world");
            _world = _worldObject.AddComponent<TestReferenceFrameWorld>();
        }

        [TearDown]
        public void DestroyWorld() {
            Object.Destroy(_worldObject);
            _observations?.Dispose();
            _observations = null;
        }

        [UnityTest]
        public IEnumerator Rider_OnAMovingPlatform_BindsWithinTwoStepsAndReportsThePlatformVelocity() {
            var platform = MovingPlatformFixture.Create(_worldObject.transform, Vector3.zero, PlatformVelocity);

            yield return new WaitForFixedUpdate();

            var rider = CreateRider(platform, 0f);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(rider.Binding.IsBound);
            Assert.AreSame(platform.Source, rider.Binding.BoundFrame);
            var body = rider.GetComponent<Rigidbody>();
            AssertNear(PlatformVelocity, rider.Binding.WorldVelocityAt(body.position));
        }

        [UnityTest]
        public IEnumerator Rider_StandingStillForFiftySteps_StaysBound() {
            var platform = MovingPlatformFixture.Create(_worldObject.transform, Vector3.zero, Vector3.zero);
            var rider = CreateRider(platform, 0f);

            yield return new WaitForFixedUpdate();

            for (var step = 0; step < 50; step++) {
                Assert.IsTrue(rider.Binding.IsBound, $"unbound at step {step}");

                yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(rider.Binding.IsBound);
        }

        [UnityTest]
        public IEnumerator Rider_LiftedOffThePlatform_ReleasesAfterTheHoldAndNotBefore() {
            var platform = MovingPlatformFixture.Create(_worldObject.transform, Vector3.zero, Vector3.zero);
            var rider = CreateRider(platform, 0f);
            var body = rider.GetComponent<Rigidbody>();
            body.useGravity = false;

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(rider.Binding.IsBound);
            body.position += Vector3.up * LiftMetres;
            body.linearVelocity = Vector3.zero;
            var holdSteps = Mathf.RoundToInt((float)rider.Binding.HoldSeconds / Time.fixedDeltaTime);

            for (var step = 0; step < holdSteps - 1; step++) {
                yield return new WaitForFixedUpdate();

                Assert.IsTrue(rider.Binding.IsBound, $"released after {step + 1} of {holdSteps} hold steps");
            }

            for (var step = 0; step < 3; step++)
                yield return new WaitForFixedUpdate();

            Assert.IsFalse(rider.Binding.IsBound);
        }

        [UnityTest]
        public IEnumerator Rider_OnASleepingBody_DoesNotBindUntilTheBodyWakes() {
            var platform = MovingPlatformFixture.Create(_worldObject.transform, Vector3.zero, Vector3.zero);
            var rider = CreateRider(platform, 0.05f);
            var body = rider.GetComponent<Rigidbody>();
            body.useGravity = false;
            body.Sleep();

            for (var step = 0; step < 10; step++) {
                yield return new WaitForFixedUpdate();

                Assert.IsTrue(body.IsSleeping(), $"woke at step {step}");
                Assert.IsFalse(rider.Binding.IsBound, $"bound while asleep at step {step}");
            }

            body.WakeUp();

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(rider.Binding.IsBound);
        }

        [UnityTest]
        public IEnumerator Rider_OnAKinematicBody_DoesNotBindUntilTheBodyIsDynamic() {
            var platform = MovingPlatformFixture.Create(_worldObject.transform, Vector3.zero, Vector3.zero);
            var rider = CreateRider(platform, 0.05f);
            var body = rider.GetComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;

            for (var step = 0; step < 10; step++) {
                yield return new WaitForFixedUpdate();

                Assert.IsFalse(rider.Binding.IsBound, $"bound while kinematic at step {step}");
            }

            body.isKinematic = false;

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(rider.Binding.IsBound);
        }

        [UnityTest]
        public IEnumerator Rider_BindReleaseAndRebind_RecordsTheBoundOccurrenceOncePerBind() {
            _observations = CoreKit.Observations.CreateContext("reference frame rider play observations");
            _world.Observations = _observations;
            var platform = MovingPlatformFixture.Create(_worldObject.transform, Vector3.zero, Vector3.zero);
            var rider = CreateRider(platform, 0.05f);
            var body = rider.GetComponent<Rigidbody>();
            body.useGravity = false;
            rider.Binding.HoldSeconds = 0.1;

            for (var step = 0; step < 10; step++)
                yield return new WaitForFixedUpdate();

            Assert.IsTrue(rider.Binding.IsBound);
            Assert.AreEqual(1L, BoundCount());

            var standing = body.position;
            body.position = standing + Vector3.up * LiftMetres;

            for (var step = 0; step < 10; step++)
                yield return new WaitForFixedUpdate();

            Assert.IsFalse(rider.Binding.IsBound);
            Assert.AreEqual(1L, BoundCount());

            body.position = standing;

            for (var step = 0; step < 3; step++)
                yield return new WaitForFixedUpdate();

            Assert.IsTrue(rider.Binding.IsBound);
            Assert.AreEqual(2L, BoundCount());
        }

        [UnityTest]
        public IEnumerator Rider_WithAProbeStrideOfFour_BindsWithinFiveStepsAndNotWithinTwo() {
            var platform = MovingPlatformFixture.Create(_worldObject.transform, Vector3.zero, Vector3.zero);
            var rider = CreateRider(platform, 0.05f);
            rider.GetComponent<Rigidbody>().useGravity = false;
            typeof(ReferenceFrameRider).GetField("probeStride", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(rider, 4);

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.IsFalse(rider.Binding.IsBound);

            for (var step = 0; step < 3; step++)
                yield return new WaitForFixedUpdate();

            Assert.IsTrue(rider.Binding.IsBound);
        }

        private ReferenceFrameRider CreateRider(MovingPlatformFixture platform, float gap) {
            var rider = new GameObject("Rider");
            rider.transform.SetParent(_worldObject.transform, false);
            var platformPosition = platform.Body.position;
            var height = platform.TopHeight + BodyHeight * 0.5f + gap;
            rider.transform.position = new Vector3(platformPosition.x, height, platformPosition.z);
            rider.AddComponent<BoxCollider>().size = Vector3.one * BodyHeight;
            rider.AddComponent<Rigidbody>();

            return rider.AddComponent<ReferenceFrameRider>();
        }

        private long BoundCount() =>
                _observations.TryRead(CharacterObservations.ReferenceFrameBound.Key, out var reading)
                        ? reading.Count
                        : 0L;

        private static void AssertNear(Vector3 expected, Vector3 actual) {
            Assert.That(Vector3.Distance(expected, actual), Is.LessThanOrEqualTo(Tolerance),
                $"expected {expected} but was {actual}");
        }
    }
}
