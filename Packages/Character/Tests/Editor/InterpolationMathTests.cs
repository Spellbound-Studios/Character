// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class InterpolationMathTests {
        private const float Tolerance = 1e-5f;

        [Test]
        public void LerpWithSpeed_MovesByAtMostTheSpeedTimesTheStep() {
            Assert.That(InterpolationMath.LerpWithSpeed(0f, 10f, 2f, 0.5f), Is.EqualTo(1f).Within(Tolerance));
            Assert.That(InterpolationMath.LerpWithSpeed(10f, 0f, 2f, 0.5f), Is.EqualTo(9f).Within(Tolerance));
            Assert.That(InterpolationMath.LerpWithSpeed(0f, 0.25f, 2f, 0.5f), Is.EqualTo(0.25f).Within(Tolerance));
        }

        [Test]
        public void SmoothDampFloat_PassesTheMaximumSpeedAndTheStepToSmoothDamp() {
            var velocity = 0f;
            var expectedVelocity = 0f;

            var value = InterpolationMath.SmoothDampFloat(0f, 10f, ref velocity, 0.3f, 0.1f, 5f);
            var expected = Mathf.SmoothDamp(0f, 10f, ref expectedVelocity, 0.3f, 5f, 0.1f);

            Assert.That(value, Is.EqualTo(expected).Within(Tolerance));
            Assert.That(velocity, Is.EqualTo(expectedVelocity).Within(Tolerance));
            Assert.That(value, Is.LessThan(0.5f + Tolerance));
        }

        [Test]
        public void RemapValue_MapsAndClampsIntoTheTargetRange() {
            Assert.That(InterpolationMath.RemapValue(5f, 0f, 10f, 100f, 200f), Is.EqualTo(150f).Within(Tolerance));
            Assert.That(InterpolationMath.RemapValue(15f, 0f, 10f, 100f, 200f), Is.EqualTo(200f).Within(Tolerance));
            Assert.That(InterpolationMath.RemapValue(-5f, 0f, 10f, 100f, 200f), Is.EqualTo(100f).Within(Tolerance));
        }
    }
}
