// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class RigSocketTests {
        private const float Tolerance = 1e-5f;

        [Test]
        public void NewSocket_DefaultsItsPatternAndPose() {
            var socket = new RigSocket();

            Assert.AreEqual("socket_{id}", socket.NodeNamePattern);
            Assert.IsTrue(socket.ParentBone.IsEmpty);
            Assert.AreEqual(Vector3.zero, socket.DefaultLocalPosition);
            Assert.AreEqual(Vector3.zero, socket.DefaultLocalEulerAngles);
        }

        [Test]
        public void NodeName_ReplacesTheIdPlaceholder() {
            var socket = new RigSocket { Id = "hand", ParentBone = RigBoneReference.Human(HumanBodyBones.RightHand) };

            Assert.AreEqual("socket_hand", socket.NodeName);

            socket.NodeNamePattern = "{id}_mount_{id}";

            Assert.AreEqual("hand_mount_hand", socket.NodeName);
        }

        [Test]
        public void NodeName_KeepsAPatternWithoutAPlaceholder() {
            var socket = new RigSocket { Id = "hand", NodeNamePattern = "attach" };

            Assert.AreEqual("attach", socket.NodeName);
        }

        [Test]
        public void DefaultLocalPose_ComesFromThePositionAndEulerAngles() {
            var socket = new RigSocket {
                DefaultLocalPosition = new Vector3(0.1f, 0.2f, 0.3f),
                DefaultLocalEulerAngles = new Vector3(0f, 90f, 0f)
            };

            var pose = socket.DefaultLocalPose;

            Assert.That(Vector3.Distance(new Vector3(0.1f, 0.2f, 0.3f), pose.position), Is.LessThan(Tolerance));
            Assert.That(Quaternion.Angle(Quaternion.Euler(0f, 90f, 0f), pose.rotation), Is.LessThan(0.01f));
        }
    }
}
