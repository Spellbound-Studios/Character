// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class CharacterKitSocketTests {
        private Scene _previewScene;
        private Transform _root;
        private RigContract _contract;

        [SetUp]
        public void CreateFigure() {
            _previewScene = EditorSceneManager.NewPreviewScene();
            var root = new GameObject("Model");
            SceneManager.MoveGameObjectToScene(root, _previewScene);
            _root = root.transform;

            var hips = AddChild(_root, "Hips");
            AddChild(hips, "socket_hand");
            AddChild(hips, "Spine");

            _contract = ScriptableObject.CreateInstance<RigContract>();
            _contract.EditableSockets.Add(new RigSocket { Id = "hand", ParentBone = RigBoneReference.Path("Hips") });
            _contract.EditableSockets.Add(new RigSocket {
                Id = "back",
                ParentBone = RigBoneReference.Path("Hips/Spine")
            });
            _contract.EditableSockets.Add(new RigSocket {
                Id = "tail",
                ParentBone = RigBoneReference.Path("Hips/Tail")
            });
        }

        [TearDown]
        public void DestroyFigure() {
            Object.DestroyImmediate(_contract);
            EditorSceneManager.ClosePreviewScene(_previewScene);
        }

        [Test]
        public void ResolveSockets_GivesTheResolversTable() {
            var expected = SocketResolver.Resolve(_root, null, _contract);
            var actual = CharacterKit.Rig.ResolveSockets(_root, _contract);

            CollectionAssert.AreEqual(new[] { "hand", "back" }, expected.Ids);
            CollectionAssert.AreEqual(expected.Ids, actual.Ids);
            CollectionAssert.AreEqual(expected.Defaulted, actual.Defaulted);
            CollectionAssert.AreEqual(expected.Missing, actual.Missing);
            CollectionAssert.AreEqual(new[] { "back" }, actual.Defaulted);
            CollectionAssert.AreEqual(new[] { "tail" }, actual.Missing);
        }

        private static Transform AddChild(Transform parent, string name) {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);

            return child;
        }
    }
}
