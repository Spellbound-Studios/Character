// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;
using UnityEngine.InputSystem;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Shared demo helper: on the toggle key it frees/locks the cursor and pauses/resumes mouse-look (via the
    /// controller) so the UIToolkit demo panels can be clicked. One of these per demo HUD GameObject — the panels
    /// themselves carry no cursor logic, so several can share one UIDocument without redundancy. Demo scaffolding.
    /// </summary>
    public sealed class DemoCursorToggleExample : MonoBehaviour {
        [SerializeField,
         Tooltip("Controller whose mouse-look is paused while the cursor is free. Auto-found if left empty.")]
        private PlayerControllerExample controller;

        [SerializeField,
         Tooltip("Key that toggles between a free cursor (to click the panels) and locked mouse-look.")]
        private Key toggleKey = Key.Tab;

        private bool _cursorFree;

        private void Update() {
            if (Keyboard.current == null || !Keyboard.current[toggleKey].wasPressedThisFrame)
                return;

            if (controller == null)
                controller = FindAnyObjectByType<PlayerControllerExample>();

            _cursorFree = !_cursorFree;
            Cursor.lockState = _cursorFree
                    ? CursorLockMode.None
                    : CursorLockMode.Locked;
            Cursor.visible = _cursorFree;

            if (controller != null)
                controller.SetCameraFollowMouse(!_cursorFree);
        }
    }
}
