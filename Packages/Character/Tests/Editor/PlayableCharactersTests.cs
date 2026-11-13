// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Tests;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class PlayableCharactersTests {
        private const string Folder = "Assets/CharacterPlayableTests";

        [SetUp]
        public void CreateFolder() {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets", "CharacterPlayableTests");
        }

        [TearDown]
        public void DeleteFolder() {
            AssetDatabase.DeleteAsset(Folder);
        }

        private static GameObject SavePrefab(GameObject instance, string name) {
            try {
                PrefabUtility.SaveAsPrefabAsset(instance, Folder + "/" + name + ".prefab");
            }
            finally {
                Object.DestroyImmediate(instance);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/" + name + ".prefab");
        }

        [Test]
        public void IsPlayable_TrueOnAPrefabWhoseRootCarriesTheMarker() {
            var instance = new GameObject("Hero");
            instance.AddComponent<PlayableTestCharacter>();

            var prefab = SavePrefab(instance, "Hero");

            Assert.IsNotNull(prefab);
            Assert.IsTrue(CharacterKit.Body.IsPlayable(prefab));
        }

        [Test]
        public void IsPlayable_FalseOnACrate() {
            var instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
            instance.name = "Crate";
            instance.AddComponent<NonPlayableTestBehaviour>();

            var prefab = SavePrefab(instance, "Crate");

            Assert.IsNotNull(prefab);
            Assert.IsFalse(CharacterKit.Body.IsPlayable(prefab));
        }

        [Test]
        public void IsPlayable_FalseWhenTheMarkerIsOnAChildOnly() {
            var instance = new GameObject("Mount");
            instance.AddComponent<NonPlayableTestBehaviour>();
            var rider = new GameObject("Rider");
            rider.transform.SetParent(instance.transform, false);
            rider.AddComponent<PlayableTestCharacter>();

            var prefab = SavePrefab(instance, "Mount");

            Assert.IsNotNull(prefab);
            Assert.IsNotNull(prefab.GetComponentInChildren<PlayableTestCharacter>());
            Assert.IsFalse(CharacterKit.Body.IsPlayable(prefab));
        }

        [Test]
        public void IsPlayable_FalseOnNothing() {
            Assert.IsFalse(CharacterKit.Body.IsPlayable(null));
        }
    }
}
