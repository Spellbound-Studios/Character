// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Body {
    [Serializable]
    public class ResizableCapsuleCollider {
        [field: HideInInspector] public CapsuleCollider collider;

        [field: SerializeField] public DefaultColliderData DefaultColliderData { get; private set; } = new();

        [field: SerializeField] public CapsuleFloatData CapsuleFloatData { get; private set; } = new();

        public void Initialize(GameObject gameObject, Bounds? meshBounds) {
            if (collider != null)
                return;

            collider = gameObject.GetComponent<CapsuleCollider>();
            DefaultColliderData.Initialize(meshBounds);
        }

        public void CalculateCapsuleColliderDimensions() {
            collider.center =
                    new Vector3(0f, DefaultColliderData.Height * (1f + CapsuleFloatData.StepHeightPercentage) * 0.5f,
                        0f);

            SetCapsuleColliderRadius(DefaultColliderData.Radius);
            SetCapsuleColliderHeight((DefaultColliderData.Height - collider.center.y) * 2f);

            CapsuleFloatData.SetCalculatedFloatDistance(collider.center.y * collider.transform.lossyScale.y);
        }

        public GroundProbeResult ProbeGround(Vector3 direction, float distance, LayerMask layers) =>
                SupportProbeMath.ProbeGround(
                    collider.bounds.center,
                    direction,
                    distance,
                    CapsuleFloatData.ProbeRadius,
                    layers);

        public void SetCapsuleColliderRadius(float radius) => collider.radius = radius;
        public void SetCapsuleColliderHeight(float height) => collider.height = height;
    }
}
