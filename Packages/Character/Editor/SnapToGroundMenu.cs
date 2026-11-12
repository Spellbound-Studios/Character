// Copyright 2026 Spellbound Studio Inc.

using UnityEditor;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// Authoring helper: lifts each selected GameObject so the bottom of its combined renderer bounds sits at its
    /// parent's origin (local y = 0) — i.e. feet on the ground, which is the contract the floating-capsule controller
    /// assumes for visuals. Operates on the whole selection in one undo step. Edit-time only; the capsule mesh
    /// represents the hovering body and should be placed by hand, not snapped.
    /// </summary>
    public static class SnapToGroundMenu {
        private const string MenuPath = "Spellbound/Controller/Snap Feet To Y=0";

        [MenuItem(MenuPath)]
        private static void SnapSelected() {
            foreach (var go in Selection.gameObjects) {
                var renderers = go.GetComponentsInChildren<Renderer>();

                if (renderers.Length == 0)
                    continue;

                var bounds = renderers[0].bounds;

                for (var i = 1; i < renderers.Length; i++)
                    bounds.Encapsulate(renderers[i].bounds);

                var t = go.transform;
                var parentY = t.parent != null
                        ? t.parent.position.y
                        : 0f;

                Undo.RecordObject(t, "Snap Feet To Y=0");
                t.position += new Vector3(0f, parentY - bounds.min.y, 0f);
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateSnapSelected() => Selection.gameObjects.Length > 0;
    }
}
