// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using Spellbound.Character.Rig;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Character.Editor {
    public sealed class BuildRagdollWindow : EditorWindow {
        private const string MenuPath = "Spellbound/Character/Build Ragdoll";

        private ObjectField _profileField;
        private Label _targetLabel;
        private Button _buildButton;

        [MenuItem(MenuPath)]
        private static void Open() {
            var window = GetWindow<BuildRagdollWindow>();
            window.titleContent = new GUIContent("Build Ragdoll");
            window.Show();
        }

        private void CreateGUI() {
            _profileField = new ObjectField("Ragdoll Profile") {
                objectType = typeof(RagdollProfile),
                allowSceneObjects = false
            };
            _profileField.RegisterValueChangedCallback(_ => Refresh());

            _targetLabel = new Label();
            _buildButton = new Button(BuildOnSelection) { text = "Build Ragdoll" };

            rootVisualElement.Add(_profileField);
            rootVisualElement.Add(_targetLabel);
            rootVisualElement.Add(_buildButton);
            Refresh();
        }

        private void OnSelectionChange() => Refresh();

        private void Refresh() {
            if (_targetLabel == null)
                return;

            var target = Selection.activeGameObject;
            _targetLabel.text = target != null
                    ? "Hierarchy: " + target.name
                    : "Select the root of a character's hierarchy.";
            _buildButton.SetEnabled(target != null && _profileField.value != null);
        }

        private void BuildOnSelection() {
            var target = Selection.activeGameObject;
            var profile = _profileField.value as RagdollProfile;

            if (target == null || profile == null)
                return;

            Undo.RegisterFullObjectHierarchyUndo(target, "Build Ragdoll");

            var bones = new List<RagdollBone>();
            var animator = target.GetComponentInChildren<Animator>();

            if (!CharacterKit.Rig.BuildRagdoll(target.transform, animator, profile, bones, out _))
                return;

            foreach (var bone in bones) {
                EditorUtility.SetDirty(bone.Body);
                EditorUtility.SetDirty(bone.Collider);

                if (bone.Joint != null)
                    EditorUtility.SetDirty(bone.Joint);
            }

            if (target.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(target.scene);
        }
    }
}
