// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using Spellbound.Core.Logging;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Spellbound.Character.Presentation.Editor {
    public static class MeasureCapsuleFromMeshMenu {
        private const string MenuPath = "Spellbound/Character/Measure Capsule From Mesh";
        private const string DefaultColliderDataField = "<DefaultColliderData>k__BackingField";
        private const string HeightField = "<Height>k__BackingField";
        private const string RadiusField = "<Radius>k__BackingField";
        private const string OverrideHeightField = "<OverrideHeight>k__BackingField";
        private const string OverrideRadiusField = "<OverrideRadius>k__BackingField";

        [MenuItem(MenuPath)]
        private static void MeasureSelected() {
            var selected = Selection.activeGameObject;
            var meshBounds = MeshBoundsMath.FindSharedMeshBounds(selected);

            if (!meshBounds.HasValue) {
                Log.Error($"{selected.name} holds no mesh to measure the capsule from.", selected);

                return;
            }

            for (var current = selected.transform; current != null; current = current.parent) {
                var measured = 0;

                foreach (var component in current.GetComponents<MonoBehaviour>()) {
                    if (component != null)
                        measured += Measure(component, meshBounds.Value);
                }

                if (measured == 0)
                    continue;

                Save(current.gameObject);

                return;
            }

            Log.Error($"Neither {selected.name} nor a parent holds a {nameof(ResizableCapsuleCollider)} to measure.",
                selected);
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateMeasureSelected() {
            var selected = Selection.activeGameObject;

            return selected != null &&
                   (PrefabUtility.IsPartOfPrefabAsset(selected) || PrefabStageUtility.GetPrefabStage(selected) != null);
        }

        private static int Measure(MonoBehaviour component, Bounds meshBounds) {
            var serializedComponent = new SerializedObject(component);
            var property = serializedComponent.GetIterator();
            var measured = 0;

            while (property.Next(true)) {
                if (property.propertyType != SerializedPropertyType.Generic ||
                    property.type != nameof(ResizableCapsuleCollider))
                    continue;

                var data = property.FindPropertyRelative(DefaultColliderDataField);
                var measuring = (DefaultColliderData)data.boxedValue;
                measuring.Initialize(meshBounds);

                data.FindPropertyRelative(HeightField).floatValue = measuring.Height;
                data.FindPropertyRelative(RadiusField).floatValue = measuring.Radius;
                data.FindPropertyRelative(OverrideHeightField).boolValue = true;
                data.FindPropertyRelative(OverrideRadiusField).boolValue = true;
                measured++;
            }

            if (measured > 0) {
                serializedComponent.ApplyModifiedProperties();
                Undo.SetCurrentGroupName("Measure Capsule From Mesh");
            }

            return measured;
        }

        private static void Save(GameObject holder) {
            if (PrefabUtility.IsPartOfPrefabAsset(holder)) {
                PrefabUtility.SavePrefabAsset(holder.transform.root.gameObject);

                return;
            }

            var stage = PrefabStageUtility.GetPrefabStage(holder);

            if (stage != null)
                PrefabUtility.SaveAsPrefabAsset(stage.prefabContentsRoot, stage.assetPath);
        }
    }
}
