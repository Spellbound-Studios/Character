// Copyright 2025 Spellbound Studio Inc.

using System;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Controller {
    [Serializable]
    public class DefaultColliderData {
        [Header("Collider Configuration")]
        [field: SerializeField,
         Tooltip("Capsule height. Auto-measured from the mesh unless Override Height is on; drives ride height and the ceiling probe.")]
        public float Height { get; private set; }

        [field: SerializeField,
         Tooltip("Capsule radius. Auto-measured from the mesh unless Override Radius is on. Wider = fatter body that fits through fewer gaps.")]
        public float Radius { get; private set; }

        // If the user doesn't want to automatically calculate based on mesh bounds then set to true.
        [Header("Override Settings")]
        [field: SerializeField,
         Tooltip("Use the manual Height above instead of auto-measuring it from the mesh.")]
        public bool OverrideHeight { get; private set; }

        [field: SerializeField,
         Tooltip("Use the manual Radius above instead of auto-measuring it from the mesh.")]
        public bool OverrideRadius { get; private set; }

        private const float FallbackHeight = 1.8f;
        private const float FallbackRadius = 0.3f;

        [Header("Log Suppression")]
        [field: SerializeField,
         Tooltip("Silence console logs about collider auto-sizing. On by default to keep the console clean.")]
        public bool SuppressConsole { get; private set; } = true;

        public void Initialize(GameObject go) {
            Bounds bounds = default;
            var foundMesh = false;
            var meshSource = "";

            var smr = go.GetComponent<SkinnedMeshRenderer>();

            if (smr != null && smr.sharedMesh != null) {
                bounds = smr.sharedMesh.bounds;
                foundMesh = true;
                meshSource = "SkinnedMeshRenderer";
            }
            else {
                smr = go.GetComponentInChildren<SkinnedMeshRenderer>();

                if (smr != null && smr.sharedMesh != null) {
                    bounds = smr.sharedMesh.bounds;
                    foundMesh = true;
                    meshSource = "SkinnedMeshRenderer (child)";
                }
                else {
                    var meshFilter = go.GetComponent<MeshFilter>();

                    if (meshFilter != null && meshFilter.sharedMesh != null) {
                        bounds = meshFilter.sharedMesh.bounds;
                        foundMesh = true;
                        meshSource = "MeshFilter";
                    }
                    else {
                        meshFilter = go.GetComponentInChildren<MeshFilter>();

                        if (meshFilter != null && meshFilter.sharedMesh != null) {
                            bounds = meshFilter.sharedMesh.bounds;
                            foundMesh = true;
                            meshSource = "MeshFilter (child)";
                        }
                    }
                }
            }

            if (!foundMesh) {
                if (!SuppressConsole) {
                    Log.Warn($"No mesh found on {go.name} or its children. " +
                                     $"Using fallback values for non-overridden values.");
                }

                SetFallbackValues();

                return;
            }

            if (!SuppressConsole)
                Log.Info($"Using mesh bounds from {meshSource} on {go.name}");

            if (!OverrideHeight) {
                Height = bounds.size.y;
                ValidateAndClampHeight();
            }

            if (!OverrideRadius) {
                var meshRadius = Mathf.Max(bounds.size.x, bounds.size.z) * 0.5f;
                Radius = meshRadius;
                ValidateAndClampRadius();
            }

            ValidateConfiguration();
        }

        private void SetFallbackValues() {
            if (!OverrideHeight) {
                Height = FallbackHeight;
                ValidateAndClampHeight();
            }

            if (!OverrideRadius) {
                Radius = FallbackRadius;
                ValidateAndClampRadius();
            }
        }

        private void ValidateAndClampHeight() {
            if (Height <= 0f) {
                Log.Warn($"Calculated height {Height} is invalid. Using fallback height {FallbackHeight}.");
                Height = FallbackHeight;
            }

            // Reasonable limits for character height
            Height = Mathf.Clamp(Height, 0.1f, 10f);
        }

        private void ValidateAndClampRadius() {
            if (Radius <= 0f) {
                Log.Warn($"Calculated radius {Radius} is invalid. Using fallback radius {FallbackRadius}.");
                Radius = FallbackRadius;
            }

            Radius = Mathf.Clamp(Radius, 0.05f, 200f);
        }

        private void ValidateConfiguration() {
            if (!SuppressConsole)
                Log.Info($"DefaultColliderData initialized: Height={Height:F2}, Radius={Radius:F2}");
        }

        /// <summary>
        /// Call this when values are changed in inspector to re-validate
        /// </summary>
        public void HandleValidation() {
            ValidateAndClampHeight();
            ValidateAndClampRadius();
            ValidateConfiguration();
        }
    }
}