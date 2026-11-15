// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class ReferenceFrameBindingTurnTests {
        private const float Tolerance = 1e-4f;
        private const float AngleTolerance = 1e-2f;

        private static ReferenceFrameBinding BoundTo(Vector3 linearVelocity, Vector3 angularVelocity) {
            var frame = new TurningReferenceFrame(9, new Pose(Vector3.zero, Quaternion.identity), linearVelocity,
                angularVelocity);
            var binding = new ReferenceFrameBinding();
            binding.Bind(frame, new Pose(Vector3.zero, Quaternion.identity), 0.0);
            return binding;
        }

        [Test]
        public void Turn_AdvancesTheYawByTheStepsShareOfTheFramesTurn() {
            var binding = BoundTo(Vector3.zero, new Vector3(0f, Mathf.PI, 0f));

            var turned = binding.Turn(Quaternion.Euler(0f, 10f, 0f), 0.5);

            Assert.That(Quaternion.Angle(Quaternion.Euler(0f, 100f, 0f), turned), Is.LessThan(AngleTolerance));
        }

        [Test]
        public void Turn_AdvancesAPitchedBodyAboutTheFramesAxis() {
            var binding = BoundTo(Vector3.zero, new Vector3(0f, Mathf.PI, 0f));
            var pitched = Quaternion.Euler(30f, 0f, 0f);

            var turned = binding.Turn(pitched, 0.5);

            Assert.That(Quaternion.Angle(Quaternion.Euler(0f, 90f, 0f) * pitched, turned),
                Is.LessThan(AngleTolerance));
        }

        [Test]
        public void Turn_LeavesTheRotationWhenTheFrameDoesNotTurn() {
            var binding = BoundTo(new Vector3(1f, 0f, 0f), Vector3.zero);
            var rotation = Quaternion.Euler(0f, 45f, 0f);

            Assert.AreEqual(rotation, binding.Turn(rotation, 0.5));
        }

        [Test]
        public void Turn_LeavesTheRotationWhenUnbound() {
            var rotation = Quaternion.Euler(0f, 45f, 0f);

            Assert.AreEqual(rotation, new ReferenceFrameBinding().Turn(rotation, 0.5));
        }

        [TestCase(-0.5)]
        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        public void Turn_LeavesTheRotationForAStepThatIsNotAFiniteNonNegativeNumber(double stepSeconds) {
            var binding = BoundTo(Vector3.zero, new Vector3(0f, Mathf.PI, 0f));
            var rotation = Quaternion.Euler(0f, 45f, 0f);

            Assert.AreEqual(rotation, binding.Turn(rotation, stepSeconds));
        }

        [Test]
        public void WorldVelocity_AddsTheFramesVelocityAtThePoint() {
            var binding = BoundTo(new Vector3(1f, 0f, 0f), Vector3.zero);

            var velocity = binding.WorldVelocity(new Vector3(0f, 0f, 2f), new Vector3(3f, 0f, 4f));

            Assert.That(Vector3.Distance(new Vector3(1f, 0f, 2f), velocity), Is.LessThan(Tolerance));
        }

        [Test]
        public void WorldVelocity_AddsTheTurningFramesVelocityAtThePoint() {
            var binding = BoundTo(new Vector3(1f, 0f, 0f), new Vector3(0f, 2f, 0f));

            var velocity = binding.WorldVelocity(new Vector3(0f, 0f, 2f), new Vector3(0f, 0f, 3f));

            Assert.That(Vector3.Distance(new Vector3(7f, 0f, 2f), velocity), Is.LessThan(Tolerance));
        }

        [Test]
        public void WorldVelocity_IsTheRelativeWhenUnbound() {
            var relative = new Vector3(0f, 0f, 2f);

            Assert.AreEqual(relative, new ReferenceFrameBinding().WorldVelocity(relative, Vector3.one));
        }
    }
}
