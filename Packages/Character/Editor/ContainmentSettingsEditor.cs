// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Spellbound.Character.Editor {
    [CustomEditor(typeof(ContainmentSettings))]
    public sealed class ContainmentSettingsEditor : UnityEditor.Editor {
        private const string ScriptPropertyPath = "m_Script";
        private const string HasFloorPropertyPath = nameof(ContainmentSettings.hasFloor);
        private const string FloorHeightPropertyPath = nameof(ContainmentSettings.floorHeight);

        public override VisualElement CreateInspectorGUI() {
            var root = new VisualElement();
            PropertyField hasFloorField = null;
            PropertyField floorRow = null;
            var property = serializedObject.GetIterator();
            var enterChildren = true;

            while (property.NextVisible(enterChildren)) {
                enterChildren = false;

                if (property.propertyPath == ScriptPropertyPath)
                    continue;

                var field = new PropertyField(property);
                root.Add(field);

                if (property.propertyPath == HasFloorPropertyPath)
                    hasFloorField = field;
                else if (property.propertyPath == FloorHeightPropertyPath)
                    floorRow = field;
            }

            if (hasFloorField == null || floorRow == null)
                return root;

            ShowFloorRow(floorRow, serializedObject.FindProperty(HasFloorPropertyPath).boolValue);
            hasFloorField.RegisterValueChangeCallback(change =>
                ShowFloorRow(floorRow, change.changedProperty.boolValue));

            return root;
        }

        private static void ShowFloorRow(VisualElement floorRow, bool hasFloor) {
            floorRow.style.display = hasFloor ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
