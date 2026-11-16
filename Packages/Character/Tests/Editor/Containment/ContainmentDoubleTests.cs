// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Character.Tests;
using Spellbound.Core.Ground;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class ContainmentDoubleTests {
        [Test]
        public void TestContainmentSpace_AnswersItsReadiness() {
            var space = new TestContainmentSpace { Ready = false };
            IContainmentSpace seam = space;

            Assert.IsFalse(seam.IsReady(new Bounds(Vector3.zero, Vector3.one)));

            space.Ready = true;

            Assert.IsTrue(seam.IsReady(new Bounds(Vector3.zero, Vector3.one)));
        }

        [Test]
        public void TestContainmentSpace_ClassifiesAgainstItsSolids() {
            var space = new TestContainmentSpace();
            space.Solids.Add(new Bounds(Vector3.zero, new Vector3(10f, 10f, 10f)));
            IContainmentSpace seam = space;

            Assert.AreEqual(Occupancy.Solid, seam.Classify(new Bounds(Vector3.zero, Vector3.one)));
            Assert.AreEqual(Occupancy.Mixed, seam.Classify(new Bounds(new Vector3(5f, 0f, 0f), Vector3.one)));
            Assert.AreEqual(Occupancy.Empty, seam.Classify(new Bounds(new Vector3(20f, 0f, 0f), Vector3.one)));
        }

        [Test]
        public void TestRescueTarget_ReturnsItsPose() {
            var pose = new Pose(new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 45f, 0f));
            IRescueTarget seam = new TestRescueTarget { Pose = pose };

            Assert.AreEqual(pose, seam.Fallback(Vector3.zero));
        }

        [Test]
        public void TestMoveSource_ReturnsItsMoveAndUp() {
            IMoveSource seam = new TestMoveSource {
                DesiredMove = new Vector3(1f, 0f, 2f),
                DesiredUp = new Vector3(0f, 0f, 1f)
            };

            Assert.AreEqual(new Vector3(1f, 0f, 2f), seam.DesiredMove);
            Assert.AreEqual(new Vector3(0f, 0f, 1f), seam.DesiredUp);
        }

        [Test]
        public void TestSupportProbe_ReturnsItsResult() {
            var result = new GroundProbeResult(true, 0.4f, Vector3.up, new Vector3(0f, -0.4f, 0f));
            ISupportProbe seam = new TestSupportProbe { Result = result };

            var probed = seam.Probe(default, Vector3.zero, Vector3.down, 1f, 0.25f, ~0);

            Assert.IsTrue(probed.HasHit);
            Assert.AreEqual(result.Distance, probed.Distance);
            Assert.AreEqual(result.Point, probed.Point);
        }

        [Test]
        public void TestSupportProbe_MissesByDefault() {
            ISupportProbe seam = new TestSupportProbe();

            Assert.IsFalse(seam.Probe(default, Vector3.zero, Vector3.down, 1f, 0.25f, ~0).HasHit);
        }
    }
}
