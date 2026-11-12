// Copyright 2025 Spellbound Studio Inc.

using System;
using Spellbound.Core.Tooling;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// The event naming pattern should be: [Context][Action][Suffix]
    /// </summary>
    public class ExampleInputManager : MonoBehaviour, ExampleInputActions.IPlayerInputActions,
            ExampleInputActions.IConsoleInputActions {
        private ExampleInputActions _inputActions;

        // Player Input Events
        public event Action OnJumpPressed = delegate { };
        public event Action OnInteractPressed = delegate { };
        public event Action OnCinematicTogglePressed = delegate { };
        public event Action<Vector2> OnMouseWheelInput = delegate { };
        
        public bool IsSprinting { get; private set; }

        // Console Input Events
        public event Action OnPreviousCommandPressed = delegate { };
        public event Action OnNextCommandPressed = delegate { };
        public event Action OnForceClosePressed = delegate { };
        public event Action OnToggleClosePressed = delegate { };

        #region Unity Lifecycle

        private void Awake() {
            SingletonManager.RegisterSingleton(this);

            _inputActions = new ExampleInputActions();
            _inputActions.PlayerInput.SetCallbacks(this);
            _inputActions.ConsoleInput.SetCallbacks(this);
        }

        private void OnEnable() {
            if (_inputActions == null) {
                _inputActions = new ExampleInputActions();
                _inputActions.PlayerInput.SetCallbacks(this);
            }

            _inputActions.Enable();
        }

        private void OnDisable() => _inputActions?.Disable();

        private void OnDestroy() {
            SingletonManager.UnregisterSingleton<ExampleInputManager>();
            _inputActions?.Dispose();
        }

        #endregion

        #region Swapping Input Maps

        public void SwitchToPlayerInput() {
            _inputActions.ConsoleInput.Disable();
            _inputActions.PlayerInput.Enable();
        }

        public void SwitchToConsoleInput() {
            _inputActions.PlayerInput.Disable();
            _inputActions.ConsoleInput.Enable();
        }

        public void EnablePlayerInput() => _inputActions.PlayerInput.Enable();

        public void DisablePlayerInput() => _inputActions.PlayerInput.Disable();

        public void PressCinematicToggle() => OnCinematicTogglePressed.Invoke();

        #endregion

        #region PlayerInputs

        public bool CameraInputSuppressed { get; set; }

        public Vector3 Direction => _inputActions.PlayerInput.Movement.ReadValue<Vector2>();

        public Vector3 LookDirection => CameraInputSuppressed
                ? Vector3.zero
                : (Vector3)_inputActions.PlayerInput.LookDirection.ReadValue<Vector2>();

        public float MouseWheelValue => CameraInputSuppressed
                ? 0f
                : _inputActions.PlayerInput.MouseWheel.ReadValue<Vector2>().y;

        public bool IsAiming => Mouse.current != null && Mouse.current.rightButton.isPressed;

        public void OnMovement(InputAction.CallbackContext context) { }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.performed) 
                OnJumpPressed.Invoke();
        }

        public void OnLookDirection(InputAction.CallbackContext context) { }

        public void OnMouseWheel(InputAction.CallbackContext context) =>
                OnMouseWheelInput.Invoke(context.ReadValue<Vector2>());

        public void OnInteract(InputAction.CallbackContext context) {
            if (context.performed) 
                OnInteractPressed.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context) {
            if (context.performed)
                IsSprinting = true;
            else if (context.canceled)
                IsSprinting = false;
        }

        #endregion

        #region ConsoleInputs

        public void OnClose(InputAction.CallbackContext context) {
            if (context.performed)
                OnToggleClosePressed.Invoke();
        }

        public void OnForceClose(InputAction.CallbackContext context) {
            if (context.performed)
                OnForceClosePressed.Invoke();
        }

        public void OnPageUp(InputAction.CallbackContext context) {
            if (context.performed)
                OnPreviousCommandPressed.Invoke();
        }

        public void OnPageDown(InputAction.CallbackContext context) {
            if (context.performed)
                OnNextCommandPressed.Invoke();
        }

        #endregion
    }
}