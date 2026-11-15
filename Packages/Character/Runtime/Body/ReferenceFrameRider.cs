// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(-50)]
    public sealed class ReferenceFrameRider : MonoBehaviour {
        [SerializeField] private int probeStride = 1;

        [SerializeField] private float probeLift = 0.25f;

        [SerializeField] private float probeMetres = 1f;

        [SerializeField] private LayerMask probeLayers = ~0;

        [SerializeField] private bool skipSleeping = true;

        [SerializeField] private bool skipKinematic = true;

        [SerializeField] private RidingSettings settings;

        [SerializeField] private MonoBehaviour world;

        [SerializeField] private ReferenceFrameBinding binding = new();

        public ReferenceFrameBinding Binding => binding;

        public IReferenceFrameWorld World { get; set; }
    }
}
