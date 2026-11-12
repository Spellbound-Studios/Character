// Copyright 2025 Spellbound Studio Inc.

using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    public class Launcher : MonoBehaviour {
        [SerializeField] private ForceMode forceMode = ForceMode.Impulse;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float offsetFromSurface = 0.25f;
        [SerializeField] private float explosiveForce = 10f;
        [SerializeField] private float explosiveRadius = 5f;

        private void Awake() {
            var col = GetComponent<Collider>();

            if (col.isTrigger) {
                Log.Warn(
                    "Launcher: Collider is a trigger. Collision normals are unavailable. Set Is Trigger = false.");
            }
        }

        private void OnCollisionEnter(Collision collision) {
            var rb = collision.rigidbody;

            if (rb == null)
                return;

            if ((layerMask.value & (1 << rb.gameObject.layer)) == 0)
                return;

            var contact = collision.GetContact(0);
            var n = contact.normal;
            var p = contact.point;
            var explosionPos = p - n * offsetFromSurface;

            rb.AddExplosionForce(explosiveForce, explosionPos, explosiveRadius, 0f, forceMode);
        }
    }
}