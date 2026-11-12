// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// WARNING! Please ensure that your player visual, wherever it might be, has its feet at y=0. If you do not do this,
    /// your collider and visual may be mismatched, resulting in strange behavior or appearance.
    /// </summary>
    [Serializable]
    public class ResizableCapsuleCollider {
        [field: HideInInspector] public CapsuleCollider collider;

        /// <summary>The unmodified capsule dimensions derived from the player visual.</summary>
        [field: SerializeField] public DefaultColliderData DefaultColliderData { get; private set; } = new();

        /// <summary>Ride height, float-spring, and ground-probe settings.</summary>
        [field: SerializeField] public CapsuleFloatData CapsuleFloatData { get; private set; } = new();

        public void Initialize(GameObject go) {
            if (collider != null)
                return;

            collider = go.GetComponent<CapsuleCollider>();
            DefaultColliderData.Initialize(go);
        }

        /// <summary>
        /// Recalculates capsule dimensions and the calculated float distance from the current settings.
        /// </summary>
        public void CalculateCapsuleColliderDimensions() {
            collider.center =
                    new Vector3(0f, DefaultColliderData.Height * (1f + CapsuleFloatData.StepHeightPercentage) * 0.5f,
                        0f);

            SetCapsuleColliderRadius(DefaultColliderData.Radius);
            SetCapsuleColliderHeight((DefaultColliderData.Height - collider.center.y) * 2f);

            CapsuleFloatData.SetCalculatedFloatDistance(collider.center.y * collider.transform.lossyScale.y);
        }

        /// <summary>
        /// Spherecasts beneath the capsule along the given direction and distance using the configured probe radius.
        /// </summary>
        public GroundProbeResult ProbeGround(Vector3 direction, float distance, LayerMask layers) =>
                ControllerHelper.ProbeGround(
                    collider.bounds.center,
                    direction,
                    distance,
                    CapsuleFloatData.ProbeRadius,
                    layers);

        public void SetCapsuleColliderRadius(float r) => collider.radius = r;
        public void SetCapsuleColliderHeight(float h) => collider.height = h;
    }
}
