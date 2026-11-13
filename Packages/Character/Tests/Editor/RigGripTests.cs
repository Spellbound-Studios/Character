// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Rig;

namespace Spellbound.Character.Editor.Tests {
    public class RigGripTests {
        [Test]
        public void NewGrip_DefaultsItsPattern() {
            var grip = new RigGrip();

            Assert.AreEqual("grip_{socket}", grip.NodeNamePattern);
            Assert.IsTrue(string.IsNullOrEmpty(grip.Id));
            Assert.IsTrue(string.IsNullOrEmpty(grip.SocketId));
        }

        [Test]
        public void NodeName_ReplacesTheSocketPlaceholder() {
            var grip = new RigGrip { Id = "handle", SocketId = "hand" };

            Assert.AreEqual("grip_hand", grip.NodeName);
        }

        [Test]
        public void NodeName_KeepsAPatternWithoutAPlaceholder() {
            var grip = new RigGrip { Id = "handle", SocketId = "hand", NodeNamePattern = "handle" };

            Assert.AreEqual("handle", grip.NodeName);
        }
    }
}
