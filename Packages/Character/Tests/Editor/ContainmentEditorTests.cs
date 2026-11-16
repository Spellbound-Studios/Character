// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Character.Body;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class ContainmentEditorTests {
        private ContainmentSettings _settings;
        private UnityEditor.Editor _editor;

        [SetUp]
        public void CreateEditor() {
            _settings = ScriptableObject.CreateInstance<ContainmentSettings>();
            _editor = UnityEditor.Editor.CreateEditor(_settings);
        }

        [TearDown]
        public void DestroyEditor() {
            Object.DestroyImmediate(_editor);
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void CreateInspectorGUI_ShowsOnePropertyFieldPerSerializedField() {
            Assert.IsInstanceOf<ContainmentSettingsEditor>(_editor);

            var root = _editor.CreateInspectorGUI();
            var shown = root.Query<PropertyField>().ToList().Select(field => field.bindingPath).ToList();

            CollectionAssert.AreEqual(SerializedFieldPaths(_editor.serializedObject), shown);
        }

        [Test]
        public void HasFloor_Toggled_HidesAndShowsTheFloorRow() {
            SetHasFloor(false);
            var withoutFloor = FloorRow(_editor.CreateInspectorGUI());

            SetHasFloor(true);
            var withFloor = FloorRow(_editor.CreateInspectorGUI());

            Assert.AreEqual(DisplayStyle.None, withoutFloor.style.display.value);
            Assert.AreEqual(DisplayStyle.Flex, withFloor.style.display.value);
        }

        [Test]
        public void ContainmentGizmos_DrawsEveryContainmentSelectedOrNot() {
            var drawers = typeof(ContainmentGizmos)
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttribute<DrawGizmo>() != null)
                .ToList();

            Assert.AreEqual(1, drawers.Count);

            var drawer = drawers[0];
            var attribute = drawer.GetCustomAttribute<DrawGizmo>();
            var parameters = drawer.GetParameters();

            Assert.AreEqual(GizmoType.Selected | GizmoType.NonSelected, attribute.drawOptions);
            Assert.AreEqual(typeof(Containment), parameters[0].ParameterType);
            Assert.AreEqual(typeof(GizmoType), parameters[1].ParameterType);
        }

        private void SetHasFloor(bool hasFloor) {
            var serializedObject = _editor.serializedObject;
            serializedObject.Update();
            serializedObject.FindProperty(nameof(ContainmentSettings.hasFloor)).boolValue = hasFloor;
            serializedObject.ApplyModifiedProperties();
        }

        private static PropertyField FloorRow(VisualElement root) {
            return root.Query<PropertyField>().ToList()
                .Single(field => field.bindingPath == nameof(ContainmentSettings.floorHeight));
        }

        private static List<string> SerializedFieldPaths(SerializedObject serializedObject) {
            var paths = new List<string>();
            var property = serializedObject.GetIterator();
            var enterChildren = true;

            while (property.NextVisible(enterChildren)) {
                enterChildren = false;

                if (property.propertyPath != "m_Script")
                    paths.Add(property.propertyPath);
            }

            return paths;
        }
    }
}
