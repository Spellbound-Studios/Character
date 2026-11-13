// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Tests;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Character.Presentation.Editor.Tests {
    public class MeasureCapsuleFromMeshMenuTests {
        private const string Folder = "Assets/CharacterMeasureCapsuleTests";
        private const string MenuPath = "Spellbound/Character/Measure Capsule From Mesh";

        private Object _previousSelection;

        [SetUp]
        public void CreateFolder() {
            _previousSelection = Selection.activeObject;

            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets", "CharacterMeasureCapsuleTests");
        }

        [TearDown]
        public void DeleteFolder() {
            Selection.activeObject = _previousSelection;
            AssetDatabase.DeleteAsset(Folder);
        }

        [Test]
        public void MeasureCapsuleFromMesh_WritesTheSelectedMeshIntoTheNearestCapsuleAboveAndKeepsAnOverride() {
            var mesh = new Mesh {
                vertices = new[] { new Vector3(-0.4f, 0f, -0.2f), new Vector3(0.4f, 1.6f, 0.2f) }
            };
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, Folder + "/Body.asset");
            var decoyMesh = new Mesh { vertices = new[] { Vector3.zero, new Vector3(3f, 3f, 3f) } };
            decoyMesh.RecalculateBounds();
            AssetDatabase.CreateAsset(decoyMesh, Folder + "/Decoy.asset");

            var root = new GameObject("measured-character");
            var holder = new SerializedObject(root.AddComponent<CapsuleHolder>());
            holder.FindProperty("capsule.<DefaultColliderData>k__BackingField.<Radius>k__BackingField").floatValue =
                    0.1f;
            holder.FindProperty("capsule.<DefaultColliderData>k__BackingField.<OverrideRadius>k__BackingField")
                    .boolValue = true;
            holder.ApplyModifiedPropertiesWithoutUndo();
            var decoy = new GameObject("decoy");
            decoy.transform.SetParent(root.transform, false);
            decoy.AddComponent<MeshFilter>().sharedMesh = decoyMesh;
            var model = new GameObject("model");
            model.transform.SetParent(root.transform, false);
            model.AddComponent<MeshFilter>().sharedMesh = mesh;
            var prefabPath = Folder + "/MeasuredCharacter.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath).transform.Find("model")
                    .gameObject;

            Assert.IsTrue(EditorApplication.ExecuteMenuItem(MenuPath));

            var contents = PrefabUtility.LoadPrefabContents(prefabPath);

            try {
                var data = contents.GetComponent<CapsuleHolder>().Capsule.DefaultColliderData;

                Assert.IsTrue(data.OverrideHeight);
                Assert.IsTrue(data.OverrideRadius);
                Assert.That(data.Height, Is.EqualTo(1.6f).Within(1e-5f));
                Assert.That(data.Radius, Is.EqualTo(0.1f).Within(1e-5f));
            }
            finally {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
    }
}
