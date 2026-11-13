// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class MovementMathTests {
        private const float Tolerance = 1e-5f;

        [Test]
        public void VelocityMath_InputDirectionRelativeToATurnedCamera() {
            var camera = new GameObject("velocity-math-camera");

            try {
                camera.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

                var direction =
                        VelocityMath.GetInputDirectionRelativeToCamera(new Vector2(1f, 1f), camera.transform,
                            Vector3.up);

                Assert.That(Vector3.Distance(new Vector3(0.7071067f, 0f, -0.7071069f), direction),
                    Is.LessThan(Tolerance));
            }
            finally {
                Object.DestroyImmediate(camera);
            }
        }

        [Test]
        public void RotationMath_RotatesTowardsADirectionByTheStep() {
            var rotation = RotationMath.RotateTowardsDirection(Quaternion.identity, Vector3.right, Vector3.up, 90f,
                0.5f);

            Assert.That(Quaternion.Angle(Quaternion.Euler(0f, 45f, 0f), rotation), Is.LessThan(1e-3f));
        }

        [Test]
        public void SlopeMath_ReachFactorOnASixtyDegreeSlope() {
            Assert.That(SlopeMath.GetSlopeReachFactor(60f, 80f), Is.EqualTo(2.00000024f).Within(Tolerance));
        }

        [Test]
        public void SlopeMath_ReachFactorIsCappedByTheMaximumReachFactor() {
            Assert.That(SlopeMath.GetSlopeReachFactor(85f, 89f), Is.EqualTo(4f).Within(Tolerance));
            Assert.That(SlopeMath.GetSlopeReachFactor(85f, 89f, 10f), Is.EqualTo(10f).Within(Tolerance));
            Assert.That(SlopeMath.GetSlopeReachFactor(60f, 80f, 10f), Is.EqualTo(2.00000024f).Within(Tolerance));
        }

        [Test]
        public void FloatSpringMath_SolvesTheSpringBelowRideHeight() {
            var force = FloatSpringMath.SolveFloatSpring(Vector3.down, 1.5f, 1f, new Vector3(0f, -2f, 0f), 200f,
                25f);

            Assert.That(Vector3.Distance(new Vector3(0f, -50f, 0f), force), Is.LessThan(Tolerance));
        }

        [Test]
        public void SupportProbeMath_ProbesTheGroundBelow() {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);

            try {
                ground.transform.position = new Vector3(1000f, 0f, 1000f);
                ground.transform.localScale = new Vector3(10f, 1f, 10f);
                Physics.SyncTransforms();

                var probe = SupportProbeMath.ProbeGround(new Vector3(1000f, 3f, 1000f), Vector3.down, 10f, 0.25f,
                    ~0);

                Assert.IsTrue(probe.HasHit);
                Assert.That(probe.Distance, Is.EqualTo(2.5f).Within(Tolerance));
                Assert.That(Vector3.Distance(Vector3.up, probe.Normal), Is.LessThan(Tolerance));
                Assert.That(Vector3.Distance(new Vector3(1000f, 0.5f, 1000f), probe.Point), Is.LessThan(1e-3f));
            }
            finally {
                Object.DestroyImmediate(ground);
            }
        }
    }
}
