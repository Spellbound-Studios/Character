// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Tooling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Spellbound.Controller {
    public class CameraProximityBodyHider : MonoBehaviour {
        [SerializeField, Range(0.1f, 3f),
         Tooltip("Distance from the camera to the nearest point on the character's body below which the body is " +
                 "hidden (shadows kept). Higher = hides sooner; lower = lets the camera hug the body before hiding.")]
        private float hideDistance = 0.5f;

        [SerializeField,
         Tooltip("Collider the camera distance is measured against. Falls back to the first collider found on this " +
                 "object or its children.")]
        private Collider bodyCollider;

        private const float ExitBuffer = 0.1f;

        private readonly List<(Renderer renderer, ShadowCastingMode mode)> _hiddenRenderers = new();

        private ICameraRig _rig;
        private bool _hidden;

        public float HideDistance {
            get => hideDistance;
            set => hideDistance = value;
        }

        private void OnEnable() {
            if (bodyCollider == null)
                bodyCollider = GetComponentInChildren<Collider>();
        }

        private void OnDisable() {
            if (_hidden)
                Show();
        }

        private void LateUpdate() {
            if (_rig == null && !SingletonManager.TryGetSingletonInstance(out _rig))
                return;

            var cameraTransform = _rig.CurrentCameraTransform;

            if (cameraTransform == null)
                return;

            var cameraPosition = cameraTransform.position;
            var bodyPoint = bodyCollider != null
                    ? bodyCollider.ClosestPoint(cameraPosition)
                    : transform.position;
            var distance = Vector3.Distance(cameraPosition, bodyPoint);

            if (!_hidden && distance < hideDistance)
                Hide();
            else if (_hidden && distance > hideDistance + ExitBuffer)
                Show();
        }

        private void Hide() {
            _hidden = true;
            _hiddenRenderers.Clear();

            foreach (var bodyRenderer in GetComponentsInChildren<Renderer>(true)) {
                _hiddenRenderers.Add((bodyRenderer, bodyRenderer.shadowCastingMode));
                bodyRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }

        private void Show() {
            _hidden = false;

            foreach (var (bodyRenderer, mode) in _hiddenRenderers) {
                if (bodyRenderer != null)
                    bodyRenderer.shadowCastingMode = mode;
            }

            _hiddenRenderers.Clear();
        }
    }
}
