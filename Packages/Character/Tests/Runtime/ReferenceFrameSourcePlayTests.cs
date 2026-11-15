// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core.ReferenceFrames;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Character.Tests {
    public class ReferenceFrameSourcePlayTests {
        private const int Steps = 10;
        private const float Tolerance = 0.05f;

        private GameObject _worldObject;
        private TestReferenceFrameWorld _world;

        [SetUp]
        public void CreateWorld() {
            _worldObject = new GameObject("reference frame play world");
            _world = _worldObject.AddComponent<TestReferenceFrameWorld>();
        }

        [TearDown]
        public void DestroyWorld() {
            Object.Destroy(_worldObject);
        }

        [UnityTest]
        public IEnumerator Source_UnderAWorld_RegistersOnEnableAndUnregistersOnDisable() {
            var source = CreateChild("Platform").AddComponent<TransformReferenceFrameSource>();

            yield return null;

            Assert.IsTrue(source.IsRegistered);
            Assert.IsTrue(_world.ReferenceFrames.Contains(source.Id));

            var id = source.Id;
            source.enabled = false;

            yield return null;

            Assert.IsFalse(source.IsRegistered);
            Assert.IsFalse(_world.ReferenceFrames.Contains(id));
        }

        [UnityTest]
        public IEnumerator TwoSources_WithTheSameId_TheSecondLogsAndStaysUnregistered() {
            var first = CreateInactiveChild("First").AddComponent<TransformReferenceFrameSource>();
            var second = CreateInactiveChild("Second").AddComponent<TransformReferenceFrameSource>();
            first.Id = new ReferenceFrameId(42);
            second.Id = new ReferenceFrameId(42);
            first.gameObject.SetActive(true);
            LogAssert.Expect(LogType.Error, new Regex("Second"));

            second.gameObject.SetActive(true);

            yield return null;

            Assert.IsTrue(first.IsRegistered);
            Assert.IsFalse(second.IsRegistered);
            Assert.IsTrue(_world.ReferenceFrames.TryGet(new ReferenceFrameId(42), out var registered));
            Assert.AreSame(first, registered);
        }

        [UnityTest]
        public IEnumerator Source_WithItsWorldInCode_RegistersThereAndNotInItsParent() {
            var elsewhere = new GameObject("elsewhere").AddComponent<TestReferenceFrameWorld>();

            try {
                var source = CreateInactiveChild("Platform").AddComponent<TransformReferenceFrameSource>();
                source.World = elsewhere;
                source.gameObject.SetActive(true);

                yield return null;

                Assert.IsTrue(elsewhere.ReferenceFrames.Contains(source.Id));
                Assert.AreEqual(0, _world.ReferenceFrames.Count);
            }
            finally {
                Object.Destroy(elsewhere.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator RigidbodySource_KinematicBodyMovedByMovePosition_ReportsItsVelocity() {
            var platform = CreateChild("Platform");
            var body = platform.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            var source = platform.AddComponent<RigidbodyReferenceFrameSource>();
            var velocity = new Vector3(1f, 0f, 0f);

            yield return new WaitForFixedUpdate();

            for (var step = 0; step < Steps; step++) {
                body.MovePosition(body.position + velocity * Time.fixedDeltaTime);

                yield return new WaitForFixedUpdate();
            }

            AssertNear(velocity, source.VelocityAt(body.position + Vector3.up));
            AssertNear(Vector3.zero, source.AngularVelocity);
            AssertNear(body.position, source.WorldPose.position);
        }

        [UnityTest]
        public IEnumerator RigidbodySource_KinematicBodyMovedThroughMove_ReportsTheCommandedVelocityInTheSameStep() {
            var platform = CreateChild("Platform");
            var body = CreateKinematicBody(platform);
            var source = platform.AddComponent<RigidbodyReferenceFrameSource>();
            var velocity = new Vector3(1f, 0f, 0f);
            const float degreesPerSecond = 90f;
            var spin = new Vector3(0f, degreesPerSecond * Mathf.Deg2Rad, 0f);

            yield return new WaitForFixedUpdate();

            var stepSeconds = Time.fixedDeltaTime;
            var turn = Quaternion.AngleAxis(degreesPerSecond * stepSeconds, Vector3.up);
            source.Move(new Pose(body.position + velocity * stepSeconds, turn * body.rotation));

            AssertNear(velocity + Vector3.Cross(spin, Vector3.forward),
                source.VelocityAt(body.position + Vector3.forward));
            AssertNear(spin, source.AngularVelocity);
            AssertNear(Vector3.zero, body.GetPointVelocity(body.position + Vector3.forward));
        }

        [UnityTest]
        public IEnumerator RigidbodySource_KinematicBodyInAStepWithNoMove_ReportsTheBodysVelocity() {
            var platform = CreateChild("Platform");
            var body = CreateKinematicBody(platform);
            var source = platform.AddComponent<RigidbodyReferenceFrameSource>();
            var velocity = new Vector3(1f, 0f, 0f);

            yield return new WaitForFixedUpdate();

            source.Move(new Pose(body.position + velocity * Time.fixedDeltaTime, body.rotation));

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            var point = body.position + Vector3.up;
            AssertNear(body.GetPointVelocity(point), source.VelocityAt(point));
            AssertNear(body.angularVelocity, source.AngularVelocity);
        }

        [UnityTest]
        public IEnumerator RigidbodySource_SpinningBody_ReportsItsAngularVelocity() {
            var platform = CreateChild("Spinner");
            var body = platform.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.angularDamping = 0f;
            var source = platform.AddComponent<RigidbodyReferenceFrameSource>();
            var spin = new Vector3(0f, 1f, 0f);
            body.angularVelocity = spin;

            yield return new WaitForFixedUpdate();

            AssertNear(spin, source.AngularVelocity);
            AssertNear(body.GetPointVelocity(body.position + Vector3.forward),
                source.VelocityAt(body.position + Vector3.forward));
        }

        [UnityTest]
        public IEnumerator TransformSource_TranslatedTransform_ReportsItsVelocity() {
            var platform = CreateChild("Platform");
            var source = platform.AddComponent<TransformReferenceFrameSource>();
            var velocity = new Vector3(1f, 0f, 0f);

            yield return new WaitForFixedUpdate();

            for (var step = 0; step < Steps; step++) {
                platform.transform.position += velocity * Time.fixedDeltaTime;

                yield return new WaitForFixedUpdate();
            }

            AssertNear(velocity, source.VelocityAt(platform.transform.position + Vector3.up));
            AssertNear(Vector3.zero, source.AngularVelocity);
        }

        [UnityTest]
        public IEnumerator TransformSource_RotatedTransform_ReportsItsAngularVelocity() {
            var platform = CreateChild("Turntable");
            var source = platform.AddComponent<TransformReferenceFrameSource>();
            const float degreesPerSecond = 90f;

            yield return new WaitForFixedUpdate();

            for (var step = 0; step < Steps; step++) {
                platform.transform.rotation =
                        Quaternion.AngleAxis(degreesPerSecond * Time.fixedDeltaTime, Vector3.up) *
                        platform.transform.rotation;

                yield return new WaitForFixedUpdate();
            }

            var spin = new Vector3(0f, degreesPerSecond * Mathf.Deg2Rad, 0f);
            AssertNear(spin, source.AngularVelocity);
            AssertNear(Vector3.Cross(spin, Vector3.forward),
                source.VelocityAt(platform.transform.position + Vector3.forward));
        }

        private GameObject CreateChild(string name) {
            var child = new GameObject(name);
            child.transform.SetParent(_worldObject.transform, false);

            return child;
        }

        private static Rigidbody CreateKinematicBody(GameObject platform) {
            var body = platform.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            return body;
        }

        private GameObject CreateInactiveChild(string name) {
            var child = CreateChild(name);
            child.SetActive(false);

            return child;
        }

        private static void AssertNear(Vector3 expected, Vector3 actual) {
            Assert.That(Vector3.Distance(expected, actual), Is.LessThanOrEqualTo(Tolerance),
                $"expected {expected} but was {actual}");
        }
    }
}
