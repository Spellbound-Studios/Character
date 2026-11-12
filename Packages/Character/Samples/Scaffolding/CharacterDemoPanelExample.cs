// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Runtime UIToolkit panel that swaps the visible character at runtime — one button per visual on the spawned
    /// player's <see cref="CharacterVisualSwitcherExample"/>. Proves one humanoid <see cref="AnimationCollection"/>
    /// retargets across every humanoid model. Manages only its own sub-panel (never clears the root), so it coexists
    /// with the other demo panels on one shared UIDocument. Assign a Panel Settings, leave the Source Asset empty.
    /// Demo scaffolding.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class CharacterDemoPanelExample : MonoBehaviour {
        private readonly List<(Button button, string label)> _buttons = new();
        private UIDocument _document;
        private CharacterVisualSwitcherExample _switcher;
        private VisualElement _panel;
        private VisualElement _content;
        private Label _status;
        private bool _populated;

        private void Awake() {
            _document = GetComponent<UIDocument>();

            if (_document.panelSettings == null) {
                _document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                Log.Warn("CharacterDemoPanelExample created a runtime PanelSettings; assign one for reliable styling.");
            }
        }

        private void Update() {
            EnsurePanelAttached();

            if (_panel == null)
                return;

            if (_switcher == null) {
                _switcher = FindAnyObjectByType<CharacterVisualSwitcherExample>();
                _populated = false;
            }

            if (_switcher != null) {
                if (!_populated)
                    Populate();

                RefreshHighlights();
            }
            else if (_populated) {
                _content.Clear();
                _buttons.Clear();
                _populated = false;
            }

            UpdateStatus();
        }

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

            _panel.Add(MakeLabel("Character", 14f, Color.white, true));
            _status = MakeLabel(string.Empty, 10f, new Color(0.85f, 0.85f, 0.55f), false);
            _panel.Add(_status);

            _content = new VisualElement();
            _panel.Add(_content);
        }

        private void Populate() {
            _content.Clear();
            _buttons.Clear();

            var row = new VisualElement {
                style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap }
            };
            _content.Add(row);

            foreach (var visual in _switcher.Visuals) {
                if (string.IsNullOrEmpty(visual.label))
                    continue;

                var captured = visual.label;
                var button = new Button(() => Select(captured)) { text = captured };
                StyleButton(button);
                row.Add(button);
                _buttons.Add((button, captured));
            }

            _populated = true;
        }

        private void Select(string label) {
            _switcher.Show(label);
            RefreshHighlights();
        }

        private void RefreshHighlights() {
            var current = _switcher.Current;

            foreach (var (button, label) in _buttons)
                button.style.backgroundColor = label == current
                        ? new Color(0.18f, 0.5f, 0.9f)
                        : new Color(0.22f, 0.22f, 0.26f);
        }

        private void UpdateStatus() {
            if (_status == null)
                return;

            _status.text = _switcher == null
                    ? "Waiting for a spawned character…"
                    : string.Empty;
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
