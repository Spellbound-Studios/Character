// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core.ReferenceFrames;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class ReferenceFrameBindingTests {
        private const float Tolerance = 1e-4f;

        private static TurningReferenceFrame Platform() =>
                new(7, new Pose(new Vector3(10f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f)), Vector3.zero, Vector3.zero);

        private static Pose Standing(Vector3 position) => new(position, Quaternion.identity);

        [Test]
        public void HoldSecondsDefaultsToHalfASecond() {
            Assert.AreEqual(0.5, new ReferenceFrameBinding().HoldSeconds);
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(double.NegativeInfinity)]
        [TestCase(-1.0)]
        public void HoldSeconds_RejectsAValueThatIsNotAFiniteNonNegativeNumber(double value) {
            var binding = new ReferenceFrameBinding { HoldSeconds = value };

            Assert.AreEqual(0.0, binding.HoldSeconds);
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(-1.0)]
        public void Update_ReadsASerializedHoldThatIsNotAFiniteNonNegativeNumberAsZero(double serialized) {
            var holder = ScriptableObject.CreateInstance<ReferenceFrameBindingHolder>();

            try {
                var serializedHolder = new SerializedObject(holder);
                serializedHolder.FindProperty("binding.holdSeconds").doubleValue = serialized;
                serializedHolder.ApplyModifiedPropertiesWithoutUndo();
                var binding = holder.Binding;

                binding.Bind(Platform(), Standing(Vector3.zero), 3.0);
                binding.Update(3.0);

                Assert.IsTrue(binding.IsBound);

                binding.Update(3.1);

                Assert.IsFalse(binding.IsBound);
            }
            finally {
                Object.DestroyImmediate(holder);
            }
        }

        [Test]
        public void Bind_KeepsTheFrameThroughAGapShorterThanTheHold() {
            var frame = Platform();
            var binding = new ReferenceFrameBinding();

            binding.Bind(frame, Standing(new Vector3(10f, 1f, 2f)), 3.0);
            binding.Update(3.4);

            Assert.IsTrue(binding.IsBound);
            Assert.AreSame(frame, binding.BoundFrame);
        }

        [Test]
        public void Update_KeepsTheFrameAtExactlyTheHold() {
            var binding = new ReferenceFrameBinding();

            binding.Bind(Platform(), Standing(Vector3.zero), 3.0);
            binding.Update(3.5);

            Assert.IsTrue(binding.IsBound);
        }

        [Test]
        public void Update_ReleasesTheFrameAfterAGapLongerThanTheHold() {
            var binding = new ReferenceFrameBinding();

            binding.Bind(Platform(), Standing(new Vector3(10f, 1f, 2f)), 3.0);
            binding.Update(3.6);

            Assert.IsFalse(binding.IsBound);
            Assert.IsNull(binding.BoundFrame);
        }

        [Test]
        public void Update_ReleasesTheFrameWhenTheTimeIsEarlierThanTheLastSighting() {
            var binding = new ReferenceFrameBinding();

            binding.Bind(Platform(), Standing(Vector3.zero), 100.0);
            binding.Sight(1.0);
            binding.Update(5.0);

            Assert.IsFalse(binding.IsBound);
        }

        [Test]
        public void Sight_RenewsTheHold() {
            var binding = new ReferenceFrameBinding();

            binding.Bind(Platform(), Standing(new Vector3(10f, 1f, 2f)), 3.0);
            binding.Sight(3.4);
            binding.Update(3.8);

            Assert.IsTrue(binding.IsBound);

            binding.Update(4.0);

            Assert.IsFalse(binding.IsBound);
        }

        [Test]
        public void HoldSeconds_LongerHoldKeepsTheFrameThroughALongerGap() {
            var binding = new ReferenceFrameBinding { HoldSeconds = 2.0 };

            binding.Bind(Platform(), Standing(Vector3.zero), 0.0);
            binding.Update(1.5);

            Assert.IsTrue(binding.IsBound);
        }

        [Test]
        public void Release_UnbindsAtOnce() {
            var binding = new ReferenceFrameBinding();

            binding.Bind(Platform(), Standing(Vector3.zero), 0.0);
            binding.Release();

            Assert.IsFalse(binding.IsBound);
        }

        [Test]
        public void LocalPose_RoundTripsThroughReferenceFramePose() {
            var frame = Platform();
            var binding = new ReferenceFrameBinding();
            var worldPose = new Pose(new Vector3(10f, 1f, 2f), Quaternion.Euler(0f, 30f, 0f));

            binding.Bind(frame, worldPose, 0.0);
            var roundTrip = binding.LocalPose.ToWorld(frame);

            Assert.AreEqual(frame.Id, binding.LocalPose.Frame);
            Assert.That(Vector3.Distance(new Vector3(-2f, 1f, 0f), binding.LocalPose.Position), Is.LessThan(Tolerance));
            Assert.That(Quaternion.Angle(Quaternion.Euler(0f, -60f, 0f), binding.LocalPose.Rotation),
                Is.LessThan(1e-2f));
            Assert.That(Vector3.Distance(worldPose.position, roundTrip.position), Is.LessThan(Tolerance));
            Assert.That(Quaternion.Angle(worldPose.rotation, roundTrip.rotation), Is.LessThan(1e-2f));
        }

        [Test]
        public void WorldVelocityAt_GivesTheVelocityOfAPointOnATurningFrame() {
            var frame = new TurningReferenceFrame(3, new Pose(Vector3.zero, Quaternion.identity),
                new Vector3(1f, 0f, 0f), new Vector3(0f, 2f, 0f));
            var binding = new ReferenceFrameBinding();

            binding.Bind(frame, Standing(new Vector3(0f, 0f, 3f)), 0.0);
            var velocity = binding.WorldVelocityAt(new Vector3(0f, 0f, 3f));

            Assert.That(Vector3.Distance(new Vector3(7f, 0f, 0f), velocity), Is.LessThan(Tolerance));
        }

        [Test]
        public void WorldVelocityAt_AsksTheFrameForAVelocityThatIsNotARigidTurn() {
            var frame = new ShearingReferenceFrame(5, new Pose(new Vector3(2f, 0f, 0f), Quaternion.identity),
                new Vector3(0f, 0f, 1.5f));
            var binding = new ReferenceFrameBinding();

            binding.Bind(frame, Standing(new Vector3(6f, 0f, 0f)), 0.0);
            var velocity = binding.WorldVelocityAt(new Vector3(6f, 0f, 0f));

            Assert.That(Vector3.Distance(new Vector3(0f, 0f, 6f), velocity), Is.LessThan(Tolerance));
        }

        [Test]
        public void WorldVelocityAt_IsZeroWhenUnbound() {
            Assert.AreEqual(Vector3.zero, new ReferenceFrameBinding().WorldVelocityAt(Vector3.one));
        }

        [Test]
        public void ReferenceFrameBinding_CustomImplementation() {
            var frame = new TurningReferenceFrame(42, new Pose(new Vector3(0f, 5f, 0f), Quaternion.identity),
                new Vector3(0f, 0f, 4f), Vector3.zero);
            var binding = new ReferenceFrameBinding();

            binding.Bind(frame, Standing(new Vector3(1f, 6f, 0f)), 0.0);

            Assert.AreEqual(new ReferenceFrameId(42), binding.BoundFrame.Id);
            Assert.That(Vector3.Distance(new Vector3(1f, 1f, 0f), binding.LocalPose.Position), Is.LessThan(Tolerance));
            Assert.That(Vector3.Distance(new Vector3(0f, 0f, 4f), binding.WorldVelocityAt(new Vector3(1f, 6f, 0f))),
                Is.LessThan(Tolerance));
        }
    }
}
