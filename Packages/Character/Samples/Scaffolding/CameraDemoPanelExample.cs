// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.Tooling;
using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Runtime UIToolkit panel showcasing the camera API: switch the live camera by name (the rig's cameras) and
    /// drive the live camera's input axes (sensitivity, invert) with sliders/toggles.
    /// Manages only its own sub-panel (never clears the root), so it coexists with the other demo panels on one
    /// shared UIDocument; add a <see cref="DemoCursorToggleExample"/> to free the cursor for clicks. Assign a Panel
    /// Settings, leave the Source Asset empty. Demo scaffolding.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class CameraDemoPanelExample : MonoBehaviour {
        [SerializeField,
         Tooltip("Controller the cinematic toggle and state highlights read. Auto-found if left empty.")]
        private PlayerControllerExample controller;

        [SerializeField] private string stateDrivenCamera = "CinematicCamera";

        private readonly List<(Button button, string cameraName)> _camButtons = new();
        private Button _cinematicButton;
        private bool _cinematicActive;
        private ICameraRig _rig;
        private UIDocument _document;
        private VisualElement _panel;
        private VisualElement _content;
        private Label _status;
        private bool _populated;

        private void Awake() {
            _document = GetComponent<UIDocument>();

            if (_document.panelSettings == null) {
                _document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                Log.Warn("CameraDemoPanelExample created a runtime PanelSettings; assign one for reliable styling.");
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

            SingletonManager.TryGetSingletonInstance<ICameraRig>(out var rig);

            if (!ReferenceEquals(rig, _rig)) {
                if (_rig != null)
                    _rig.CurrentChanged -= OnCurrentChanged;

                _rig = rig;

                if (_rig != null)
                    _rig.CurrentChanged += OnCurrentChanged;

                _populated = false;
            }

            if (controller != null && _rig != null) {
                if (!_populated) {
                    Populate();
                    RefreshHighlights();
                }

                var cinematic = controller.ActionStateMachine != null &&
                                controller.ActionStateMachine.IsInState<CinematicActionStateExample>();

                if (cinematic != _cinematicActive) {
                    _cinematicActive = cinematic;
                    RefreshHighlights();
                }
            }
            else if (_populated) {
                _content.Clear();
                _camButtons.Clear();
                _populated = false;
            }

            UpdateStatus();
        }

        private void OnDestroy() {
            if (_rig != null)
                _rig.CurrentChanged -= OnCurrentChanged;
        }

        private void OnCurrentChanged(string previous, string current) {
            _populated = false;
            RefreshHighlights();
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

            _panel.Add(MakeLabel("Camera API", 14f, Color.white, true));
            _status = MakeLabel(string.Empty, 10f, new Color(0.85f, 0.85f, 0.55f), false);
            _panel.Add(_status);

            _content = new VisualElement();
            _panel.Add(_content);
        }

        private void Populate() {
            _content.Clear();
            _camButtons.Clear();

            _content.Add(MakeLabel("Camera", 12f, new Color(0.6f, 0.8f, 1f), false));

            var row = new VisualElement {
                style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginBottom = 6f }
            };
            _content.Add(row);

            foreach (var cameraName in _rig.CameraNames) {
                if (string.IsNullOrEmpty(cameraName) || cameraName == stateDrivenCamera)
                    continue;

                var captured = cameraName;
                var button = new Button(() => Switch(captured)) { text = captured };
                StyleButton(button);
                row.Add(button);
                _camButtons.Add((button, captured));
            }

            _content.Add(MakeLabel("State", 12f, new Color(0.6f, 0.8f, 1f), false));

            var stateRow = new VisualElement {
                style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginBottom = 6f }
            };
            _content.Add(stateRow);

            _cinematicButton = new Button(ToggleCinematic) { text = "Cinematic View" };
            StyleButton(_cinematicButton);
            stateRow.Add(_cinematicButton);

            var input = LiveInputController;

            if (input != null) {
                _content.Add(MakeLabel("Look", 12f, new Color(0.6f, 0.8f, 1f), false));
                _content.Add(MakeSlider("Sensitivity X", 0f, 5f,
                    GetGainMagnitude(input, ExampleCameraInputReader.Source.LookX),
                    v => SetGainMagnitude(ExampleCameraInputReader.Source.LookX, v)));
                _content.Add(MakeSlider("Sensitivity Y", 0f, 5f,
                    GetGainMagnitude(input, ExampleCameraInputReader.Source.LookY),
                    v => SetGainMagnitude(ExampleCameraInputReader.Source.LookY, v)));
                _content.Add(MakeToggle("Invert Y", IsYInverted(input), SetYInverted));
            }

            _populated = true;
        }

        private ExampleCameraInputController LiveInputController => controller != null
                ? controller.GetComponent<ExampleCameraInputController>()
                : null;

        private static float GetGainMagnitude(ExampleCameraInputController input,
            ExampleCameraInputReader.Source source) {
            foreach (var axis in input.Controllers) {
                if (axis.Input.InputSource == source)
                    return Mathf.Abs(axis.Input.Gain);
            }

            return 0f;
        }

        private void SetGainMagnitude(ExampleCameraInputReader.Source source, float magnitude) {
            var input = LiveInputController;

            if (input == null)
                return;

            foreach (var axis in input.Controllers) {
                if (axis.Input.InputSource == source)
                    axis.Input.Gain = axis.Input.Gain < 0f
                            ? -magnitude
                            : magnitude;
            }
        }

        private static bool IsYInverted(ExampleCameraInputController input) {
            foreach (var axis in input.Controllers) {
                if (axis.Input.InputSource == ExampleCameraInputReader.Source.LookY)
                    return axis.Input.Gain > 0f;
            }

            return false;
        }

        private void SetYInverted(bool inverted) {
            var input = LiveInputController;

            if (input == null)
                return;

            foreach (var axis in input.Controllers) {
                if (axis.Input.InputSource != ExampleCameraInputReader.Source.LookY)
                    continue;

                var magnitude = Mathf.Abs(axis.Input.Gain);
                axis.Input.Gain = inverted
                        ? magnitude
                        : -magnitude;
            }
        }

        private void Switch(string cameraName) => _rig.Switch(cameraName);

        private void ToggleCinematic() {
            if (SingletonManager.TryGetSingletonInstance<ExampleInputManager>(out var input))
                input.PressCinematicToggle();
        }

        private void RefreshHighlights() {
            var current = _rig.Current;

            foreach (var (button, cameraName) in _camButtons)
                button.style.backgroundColor = cameraName == current
                        ? new Color(0.18f, 0.5f, 0.9f)
                        : new Color(0.22f, 0.22f, 0.26f);

            if (_cinematicButton != null && controller != null && controller.ActionStateMachine != null)
                _cinematicButton.style.backgroundColor = controller.ActionStateMachine.IsInState<CinematicActionStateExample>()
                        ? new Color(0.18f, 0.5f, 0.9f)
                        : new Color(0.22f, 0.22f, 0.26f);
        }

        private void UpdateStatus() {
            if (_status == null)
                return;

            _status.text = controller == null || _rig == null
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

        private static Slider MakeSlider(string label, float min, float max, float value, Action<float> onChange) {
            var slider = new Slider(label, min, max) { value = value };
            slider.focusable = false;
            slider.style.marginBottom = 2f;
            slider.labelElement.style.color = Color.white;
            slider.labelElement.style.minWidth = 90f;
            slider.RegisterValueChangedCallback(evt => onChange(evt.newValue));

            return slider;
        }

        private static Toggle MakeToggle(string label, bool value, Action<bool> onChange) {
            var toggle = new Toggle(label) { value = value };
            toggle.focusable = false;
            toggle.style.marginBottom = 2f;
            toggle.labelElement.style.color = Color.white;
            toggle.labelElement.style.minWidth = 90f;
            toggle.RegisterValueChangedCallback(evt => onChange(evt.newValue));

            return toggle;
        }

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
