// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using Spellbound.Character.Body;
using Spellbound.Core.Authoring;
using Spellbound.Core.Authoring.Editor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spellbound.Character.Editor {
    [RegisteredDefinitionCheck(Name)]
    public sealed class RideableColliderCheck : IDefinitionCheck<RidingSettings> {
        public const string Name = "character.rideable-collider-without-frame";

        private const string Hint = "Add RigidbodyReferenceFrameSource or TransformReferenceFrameSource to the moving "
                                    + "thing, or take its layer out of the rideable layers.";

        public void Check(RidingSettings definition, ValidationReport report) {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            foreach (var path in CollidersWithoutFrame(definition)) {
                report.Warning(Name, path + " is on a rideable layer and no reference frame source is above it.",
                    nameof(RidingSettings.rideableLayers), Hint);
            }
        }

        public IReadOnlyList<string> PreviewLines(RidingSettings definition) {
            return CollidersWithoutFrame(definition);
        }

        private static List<string> CollidersWithoutFrame(RidingSettings definition) {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            var paths = new List<string>();
            var rideableLayers = definition.rideableLayers.value;

            for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++) {
                var scene = SceneManager.GetSceneAt(sceneIndex);

                if (!scene.isLoaded)
                    continue;

                foreach (var root in scene.GetRootGameObjects()) {
                    foreach (var collider in root.GetComponentsInChildren<Collider>(true)) {
                        if (collider.isTrigger || (rideableLayers & (1 << collider.gameObject.layer)) == 0)
                            continue;

                        if (collider.GetComponentInParent<ReferenceFrameSource>(true) != null)
                            continue;

                        paths.Add(scene.name + "/" + HierarchyPath(collider.transform));
                    }
                }
            }

            return paths;
        }

        private static string HierarchyPath(Transform transform) {
            var path = new StringBuilder(transform.name);

            for (var parent = transform.parent; parent != null; parent = parent.parent) {
                path.Insert(0, '/').Insert(0, parent.name);
            }

            return path.ToString();
        }
    }
}
