// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Runtime UIToolkit panel that lists each locomotion slot and the state variants registered for it (found in
    /// <see cref="StateRegistry"/>) and applies a chosen one to the live machine via ApplyVariant, so you can swap
    /// states and feel them without leaving play mode. Manages only its own sub-panel (never clears the root), so it
    /// coexists with the other demo panels on one shared UIDocument; add a <see cref="DemoCursorToggleExample"/> to
    /// free the cursor for clicks. Assign a Panel Settings, leave the Source Asset empty. Demo scaffolding.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class VariantSwitcherExample : MonoBehaviour {
        [SerializeField,
         Tooltip("Controller whose loco state machine this panel swaps variants on. Auto-found (incl. the spawned " +
                 "instance) when left empty.")]
        private PlayerControllerExample controller;

        private readonly List<(Button button, LocoStateTypes slot, Type type)> _buttons = new();
        private UIDocument _document;
        private VisualElement _slotContainer;
        private Label _status;
        private VisualElement _panel;
        private bool _populated;

        private void Awake() {
            _document = GetComponent<UIDocument>();

            if (_document.panelSettings == null) {
                _document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                Log.Warn("VariantSwitcherExample created a runtime PanelSettings; assign one for reliable styling.");
            }
        }

        private void Update() {
            EnsurePanelAttached();

            if (_panel == null)
                return;

            if (controller == null) {
                controller = FindAnyObjectByType<PlayerControllerExample>();
                _populated = false;
            }

            if (controller != null && controller.LocoStateMachine != null) {
                if (!_populated)
                    PopulateVariants();

                RefreshHighlights();
            }
            else if (_populated) {
                _slotContainer.Clear();
                _buttons.Clear();
                _populated = false;
            }

            UpdateStatus();
        }

        // UIDocument can recreate its rootVisualElement after startup; rebuild our own sub-panel when it detaches.
        // We never clear the root, so other demo panels sharing this UIDocument are left untouched.
        private void EnsurePanelAttached() {
            var column = DemoPanelLayout.GetColumn(_document.rootVisualElement);

            if (column == null || (_panel != null && _panel.parent == column))
                return;

            BuildChrome();
            _populated = false;
        }

        private void BuildChrome() {
            _panel = DemoPanelLayout.MakePanel();
            DemoPanelLayout.GetColumn(_document.rootVisualElement).Add(_panel);

            _panel.Add(MakeLabel("State Variants", 14f, Color.white, true));
            _status = MakeLabel(string.Empty, 10f, new Color(0.85f, 0.85f, 0.55f), false);
            _panel.Add(_status);

            _slotContainer = new VisualElement();
            _panel.Add(_slotContainer);
        }

        /// <summary>
        /// The variant filling each slot when the character appears defines that slot's type family; every
        /// registered state assignable to it becomes a swap option for the slot.
        /// </summary>
        private void PopulateVariants() {
            _slotContainer.Clear();
            _buttons.Clear();

            foreach (LocoStateTypes slot in Enum.GetValues(typeof(LocoStateTypes))) {
                var current = controller.LocoStateMachine.GetCurrentVariant(slot);

                if (current == null)
                    continue;

                var slotType = current.GetType();
                _slotContainer.Add(MakeLabel(slot.ToString(), 12f, new Color(0.6f, 0.8f, 1f), false));

                var row = new VisualElement {
                    style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginBottom = 6f }
                };
                _slotContainer.Add(row);

                foreach (var state in StateRegistry.All) {
                    if (state == null || !slotType.IsInstanceOfType(state))
                        continue;

                    var variant = state;
                    var button = new Button(() => ApplyVariant(slot, variant)) { text = variant.name };
                    StyleButton(button);
                    row.Add(button);
                    _buttons.Add((button, slot, variant.GetType()));
                }
            }

            _populated = true;
        }

        private void ApplyVariant(LocoStateTypes slot, BaseSoState variant) {
            controller.LocoStateMachine.ApplyVariant(slot, variant);
            RefreshHighlights();
        }

        private void RefreshHighlights() {
            foreach (var (button, slot, type) in _buttons) {
                var current = controller.LocoStateMachine.GetCurrentVariant(slot);

                button.style.backgroundColor = current != null && current.GetType() == type
                        ? new Color(0.18f, 0.5f, 0.9f)
                        : new Color(0.22f, 0.22f, 0.26f);
            }
        }

        private void UpdateStatus() {
            if (_status == null)
                return;

            if (controller == null || controller.LocoStateMachine == null)
                _status.text = "Waiting for a spawned character…";
            else if (_buttons.Count == 0)
                _status.text = "No states found under Resources/States.";
            else
                _status.text = string.Empty;
        }

        private static Label MakeLabel(string text, float size, Color color, bool header) =>
                new(text) {
                    style = {
                        fontSize = size, color = color,
                        unityFontStyleAndWeight = header
                                ? FontStyle.Bold
                                : FontStyle.Normal,
                        marginTop = header
                                ? 0f
                                : 4f,
                        marginBottom = 2f
                    }
                };

        private static void StyleButton(Button button) {
            button.focusable = false;
            button.style.marginRight = 4f;
            button.style.marginTop = 2f;
            button.style.paddingLeft = 6f;
            button.style.paddingRight = 6f;
            button.style.color = Color.white;
            button.style.fontSize = 11f;
        }
    }
}
