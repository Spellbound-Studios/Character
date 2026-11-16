// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core.ReferenceFrames;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class RescuerTests {
        private static readonly Vector3 Upward = new(0f, 1f, 0f);
        private static readonly Vector3 Start = new(1f, -5f, 2f);
        private static readonly Vector3 Target = new(1f, 12f, 2f);

        private ContainmentSettings _settings;
        private GameObject _bodyObject;
        private Rigidbody _body;
        private MotionClaims _motion;
        private Rescuer _rescuer;

        [SetUp]
        public void CreateBody() {
            _settings = ScriptableObject.CreateInstance<ContainmentSettings>();
            _bodyObject = new GameObject("rescued body");
            _bodyObject.transform.position = Start;
            _body = _bodyObject.AddComponent<Rigidbody>();
            _body.useGravity = false;
            _motion = new MotionClaims();
            _rescuer = new Rescuer(_settings);
        }

        [TearDown]
        public void DestroyBody() {
            Object.DestroyImmediate(_bodyObject);
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void Constructor_RefusesMissingSettings() {
            Assert.Throws<ArgumentNullException>(() => new Rescuer(null));
        }

        [Test]
        public void Rescue_OutranksADriverOfPriority999_AndHandsItTheTargetOnResume() {
            var leaving = Handoff.AtRest(new Pose(Start, Quaternion.identity), Upward);
            var driver = new RecordingMotionDriver(999, leaving);
            _motion.Claim(driver);

            _rescuer.Rescue(_body, _motion, StepTo(Target), Upward);

            Assert.AreEqual(1, driver.EndCount);
            Assert.AreEqual(2, driver.BeginCount);
            Assert.AreSame(driver, _motion.Active);
            Assert.AreEqual(1, _motion.Count);
            Assert.Less(Vector3.Distance(Target, driver.LastEntered.WorldPose.position), 0.001f);
            Assert.AreEqual(Vector3.zero, driver.LastEntered.WorldVelocity);
            Assert.AreEqual(ReferenceFrameId.World, driver.LastEntered.BoundFrame);
            Assert.AreEqual(Upward, driver.LastEntered.Up);
        }

        [Test]
        public void Rescue_BodyWithoutADriver_IsStillMovedAndStopped() {
            _body.linearVelocity = new Vector3(0f, -30f, 4f);
            _body.angularVelocity = new Vector3(2f, 0f, 1f);

            _rescuer.Rescue(_body, _motion, StepTo(Target), Upward);

            Assert.Less(Vector3.Distance(Target, _bodyObject.transform.position), 0.001f);
            Assert.Less(Vector3.Distance(Target, _body.position), 0.001f);
            Assert.AreEqual(Vector3.zero, _body.linearVelocity);
            Assert.AreEqual(Vector3.zero, _body.angularVelocity);
            Assert.IsNull(_motion.Active);
            Assert.AreEqual(0, _motion.Count);
        }

        [Test]
        public void Rescue_UnderAHigherDriver_MovesTheBodyWithoutInterruptingIt() {
            var driver = new RecordingMotionDriver(2000, Handoff.AtRest(Pose.identity, Upward));
            _motion.Claim(driver);

            _rescuer.Rescue(_body, _motion, StepTo(Target), Upward);

            Assert.Less(Vector3.Distance(Target, _bodyObject.transform.position), 0.001f);
            Assert.AreEqual(0, driver.EndCount);
            Assert.AreEqual(1, driver.BeginCount);
            Assert.AreSame(driver, _motion.Active);
            Assert.AreEqual(1, _motion.Count);
        }

        [Test]
        public void Rescue_KeepsTheBodysFacing() {
            var facing = Quaternion.Euler(0f, 30f, 0f);
            _bodyObject.transform.rotation = facing;
            Physics.SyncTransforms();

            _rescuer.Rescue(_body, _motion, StepTo(Target), Upward);

            Assert.Less(Quaternion.Angle(facing, _bodyObject.transform.rotation), 0.01f);
            Assert.Less(Quaternion.Angle(facing, _body.rotation), 0.01f);
        }

        [Test]
        public void ClaimDriver_EndsAtTheTargetAtRestInTheWorldFrame() {
            var driver = new RescueClaimDriver();
            var target = new Pose(Target, Quaternion.Euler(0f, 90f, 0f));
            driver.Aim(_settings.rescuePriority, target, Upward);

            var handoff = driver.End();

            Assert.AreEqual(1000, driver.Priority);
            Assert.IsFalse(driver.NeedsSupport);
            Assert.AreEqual(target.position, handoff.WorldPose.position);
            Assert.AreEqual(target.rotation, handoff.WorldPose.rotation);
            Assert.AreEqual(Vector3.zero, handoff.WorldVelocity);
            Assert.AreEqual(ReferenceFrameId.World, handoff.BoundFrame);
            Assert.AreEqual(Upward, handoff.Up);
        }

        private static RescueStep StepTo(Vector3 position) =>
                new(new Pose(position, Quaternion.identity), RescueRung.OpenAbove, ContainmentReason.Embedded);
    }
}
