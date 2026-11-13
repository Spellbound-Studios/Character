// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEngine;

namespace Spellbound.Character.Tests {
    public class SocketSpawnTests {
        private const float Tolerance = 1e-4f;

        private GameObject _template;
        private GameObject _spawned;
        private RigContract _contract;

        [SetUp]
        public void CreateTemplate() {
            _template = new GameObject("Character");
            _template.AddComponent<PlayableTestCharacter>();
            _template.AddComponent<SocketTableHolder>();
            _template.transform.SetPositionAndRotation(new Vector3(3f, 0f, -2f), Quaternion.Euler(0f, 45f, 0f));

            var modelRoot = AddChild(_template.transform, "Model", Vector3.zero);
            var hips = AddChild(modelRoot, "Hips", new Vector3(0f, 1f, 0f));
            AddChild(hips, "socket_Hand", new Vector3(0.4f, 0.2f, 0.1f));
            AddChild(hips, "Spine", new Vector3(0f, 0.3f, 0f));

            _contract = ScriptableObject.CreateInstance<RigContract>();
            _contract.EditableSockets.Add(new RigSocket {
                Id = "Hand",
                ParentBone = RigBoneReference.Path("Hips")
            });
            _contract.EditableSockets.Add(new RigSocket {
                Id = "Back",
                ParentBone = RigBoneReference.Path("Hips/Spine"),
                DefaultLocalPosition = new Vector3(0f, 0f, -0.2f)
            });
        }

        [TearDown]
        public void DestroyFigures() {
            if (_spawned != null)
                Object.DestroyImmediate(_spawned);

            Object.DestroyImmediate(_template);
            Object.DestroyImmediate(_contract);
        }

        [Test]
        public void Spawn_ResolvesTheSocketsOnceById() {
            var table = Spawn(out _, out _);

            Assert.IsTrue(table.TryGet("Hand", out var hand));
            Assert.IsTrue(hand.HasNode);
            Assert.AreEqual("socket_Hand", hand.Node.name);
            Assert.IsTrue(table.TryGet("Back", out var back));
            Assert.IsFalse(back.HasNode);
            Assert.AreEqual("Spine", back.Parent.name);
            CollectionAssert.AreEqual(new[] { "Back" }, table.Defaulted);
            Assert.IsTrue(table.IsComplete);
        }

        [Test]
        public void Spawn_AddsNothingToTheModelRoot() {
            Spawn(out var modelRoot, out _);

            var templateModelRoot = _template.transform.Find("Model");
            var templateComponents = templateModelRoot.GetComponents<Component>();
            var spawnedComponents = modelRoot.GetComponents<Component>();

            Assert.AreEqual(templateComponents.Length, spawnedComponents.Length);

            for (var i = 0; i < templateComponents.Length; i++)
                Assert.AreEqual(templateComponents[i].GetType(), spawnedComponents[i].GetType());

            Assert.AreEqual(1, spawnedComponents.Length);
            Assert.IsInstanceOf<Transform>(spawnedComponents[0]);
            Assert.AreEqual(0, modelRoot.GetComponentsInChildren<MonoBehaviour>(true).Length);
        }

        [Test]
        public void Spawn_TableFollowsTheBonesItHolds() {
            var table = Spawn(out var modelRoot, out var holder);

            Assert.AreSame(table, holder.Sockets);
            Assert.IsTrue(holder.Sockets.TryGet("Hand", out var hand));

            var before = hand.PoseIn(modelRoot);
            var offset = new Vector3(0.1f, -0.25f, 0.3f);
            modelRoot.Find("Hips").localPosition += offset;
            var after = hand.PoseIn(modelRoot);

            Assert.AreEqual(before.position.x + offset.x, after.position.x, Tolerance);
            Assert.AreEqual(before.position.y + offset.y, after.position.y, Tolerance);
            Assert.AreEqual(before.position.z + offset.z, after.position.z, Tolerance);
        }

        private SocketTable Spawn(out Transform modelRoot, out SocketTableHolder holder) {
            _spawned = Object.Instantiate(_template);
            modelRoot = _spawned.transform.Find("Model");
            holder = _spawned.GetComponent<SocketTableHolder>();
            holder.Hold(CharacterKit.Rig.ResolveSockets(modelRoot, _contract));

            return holder.Sockets;
        }

        private static Transform AddChild(Transform parent, string name, Vector3 localPosition) {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = localPosition;

            return child;
        }
    }
}
