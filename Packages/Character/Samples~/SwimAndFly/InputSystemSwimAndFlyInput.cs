// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;
using UnityEngine.InputSystem;

namespace Spellbound.Character.Samples.SwimAndFly {
    public sealed class InputSystemSwimAndFlyInput : MonoBehaviour, ISwimAndFlyInput {
        [SerializeField, Tooltip("A Vector2 action: x strafes, y moves forward.")]
        private InputAction move = MoveAction();

        [SerializeField, Tooltip("A button held to fly.")]
        private InputAction fly = FlyAction();

        public Vector2 Move => move.ReadValue<Vector2>();

        public bool FlyHeld => fly.IsPressed();

        private void OnEnable() {
            move.Enable();
            fly.Enable();
        }

        private void OnDisable() {
            move.Disable();
            fly.Disable();
        }

        private static InputAction MoveAction() {
            var action = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            action.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            action.AddBinding("<Gamepad>/leftStick");

            return action;
        }

        private static InputAction FlyAction() {
            var action = new InputAction("Fly", InputActionType.Button, "<Keyboard>/space");
            action.AddBinding("<Gamepad>/buttonSouth");

            return action;
        }
    }
}
