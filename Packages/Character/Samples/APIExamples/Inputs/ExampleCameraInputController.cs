// Copyright 2026 Spellbound Studio Inc.

using System;
using Spellbound.Core.Tooling;
using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [Serializable]
    public sealed class ExampleCameraInputReader : IInputAxisReader {
        public enum Source {
            None,
            LookX,
            LookY,
            Zoom
        }

        [Tooltip("Which value from the ExampleInputManager drives this axis.")]
        public Source InputSource;

        [Tooltip("The input value is multiplied by this amount prior to processing. Negative inverts the input.")]
        public float Gain = 1f;

        private ExampleInputManager _input;

        public float GetValue(UnityEngine.Object context, IInputAxisOwner.AxisDescriptor.Hints hint) {
            if (_input == null && !SingletonManager.TryGetSingletonInstance(out _input))
                return 0f;

            var value = InputSource switch {
                Source.LookX => _input.LookDirection.x,
                Source.LookY => _input.LookDirection.y,
                Source.Zoom => Mathf.Clamp(_input.MouseWheelValue, -1f, 1f),
                _ => 0f
            };

            return Time.deltaTime > 0f
                    ? value * Gain / Time.deltaTime
                    : 0f;
        }
    }

    public class ExampleCameraInputController : InputAxisControllerBase<ExampleCameraInputReader> {
        private void Update() {
            if (Application.isPlaying)
                UpdateControllers();
        }

        protected override void InitializeControllerDefaultsForAxis(
            in IInputAxisOwner.AxisDescriptor axis, Controller controller) {
            if (axis.Name.Contains("Scale") || axis.Name.Contains("Zoom")) {
                controller.Input.InputSource = ExampleCameraInputReader.Source.Zoom;
                controller.Input.Gain = -0.2f;
            }
            else if (axis.Hint == IInputAxisOwner.AxisDescriptor.Hints.X) {
                controller.Input.InputSource = ExampleCameraInputReader.Source.LookX;
                controller.Input.Gain = 0.5f;
            }
            else if (axis.Hint == IInputAxisOwner.AxisDescriptor.Hints.Y) {
                controller.Input.InputSource = ExampleCameraInputReader.Source.LookY;
                controller.Input.Gain = -0.5f;
            }
        }
    }
}
