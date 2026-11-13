// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEditor;
using UnityEngine;

namespace Spellbound.Character.Editor {
    public static class SnapToGroundMenu {
        private const string MenuPath = "Spellbound/Character/Snap Feet To Y=0";

        [MenuItem(MenuPath)]
        private static void SnapSelected() {
            foreach (var selected in Selection.gameObjects) {
                var renderers = selected.GetComponentsInChildren<Renderer>();

                if (renderers.Length == 0)
                    continue;

                var bounds = renderers[0].bounds;

                for (var i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                var selectedTransform = selected.transform;
                var parentY = selectedTransform.parent != null
                        ? selectedTransform.parent.position.y
                        : 0f;

                Undo.RecordObject(selectedTransform, "Snap Feet To Y=0");
                selectedTransform.position += new Vector3(0f, parentY - bounds.min.y, 0f);
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateSnapSelected() => Selection.gameObjects.Length > 0;
    }
}
