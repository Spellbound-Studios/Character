// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    public class ObjectThrower : MonoBehaviour {
        [SerializeField] private ExampleInputManager exampleInputManager;
        [SerializeField] private float throwSpeed = 15f;
        [SerializeField] private float timeUntilDestroy = 10f;
        [SerializeField] private float spawnForwardOffset = 0.5f;
        private readonly List<GameObject> _templates = new();
        private int _nextIndex;

        private void Awake() {
            if (!exampleInputManager)
                Log.Error("ObjectThrow is missing playerInputActionsSO. Please drag and drop it.");

            CollectChildTemplates();
        }

        private void OnEnable() {
            if (exampleInputManager != null)
                exampleInputManager.OnInteractPressed += HandleEPressed;
        }

        private void OnDisable() {
            if (exampleInputManager != null)
                exampleInputManager.OnInteractPressed -= HandleEPressed;
        }

        private void CollectChildTemplates() {
            _templates.Clear();

            foreach (Transform child in transform) {
                if (child == null || !child.gameObject.activeSelf)
                    continue;

                _templates.Add(child.gameObject);
            }

            if (_templates.Count == 0) {
                Log.Warn(
                    "PlaneObjectThrower: No child templates found. Add children under this object to throw.");
            }
        }

        private void HandleEPressed() {
            if (_templates.Count == 0)
                return;

            var template = _templates[_nextIndex];
            _nextIndex = (_nextIndex + 1) % _templates.Count;

            LaunchTemplate(template);
        }

        private void LaunchTemplate(GameObject template) {
            if (template == null)
                return;

            var dir = transform.up.normalized;
            var spawnPos = transform.position + dir * spawnForwardOffset;
            var rotation = Quaternion.LookRotation(dir, Vector3.up);

            var go = Instantiate(template, spawnPos, rotation);

            if (!go.TryGetComponent<Rigidbody>(out var rb)) {
                rb = go.AddComponent<Rigidbody>();
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                Log.Warn("PlaneObjectThrower: Template had no Rigidbody; added one at runtime.");
            }

            rb.AddForce(dir * throwSpeed, ForceMode.VelocityChange);

            Destroy(go, timeUntilDestroy);
        }
    }
}