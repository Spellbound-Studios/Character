// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;
using UnityEngine.InputSystem;

namespace Spellbound.Character.Samples.Locomotion {
    public sealed class InputSystemLocomotionInput : MonoBehaviour, ILocomotionInput {
        [SerializeField, Tooltip("A Vector2 action: x strafes, y moves forward.")]
        private InputActionReference move;

        [SerializeField, Tooltip("A button held to run.")]
        private InputActionReference run;

        [SerializeField, Tooltip("A button pressed to jump.")]
        private InputActionReference jump;

        public Vector2 Move => move != null ? move.action.ReadValue<Vector2>() : Vector2.zero;

        public bool Run => run != null && run.action.IsPressed();

        public bool JumpPressed => jump != null && jump.action.WasPressedThisFrame();

        private void OnEnable() {
            Enable(move);
            Enable(run);
            Enable(jump);
        }

        private void OnDisable() {
            Disable(move);
            Disable(run);
            Disable(jump);
        }

        private static void Enable(InputActionReference reference) {
            if (reference != null)
                reference.action.Enable();
        }

        private static void Disable(InputActionReference reference) {
            if (reference != null)
                reference.action.Disable();
        }
    }
}
