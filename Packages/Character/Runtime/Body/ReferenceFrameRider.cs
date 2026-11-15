// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Character.Body {
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(-50)]
    public sealed class ReferenceFrameRider : MonoBehaviour {
        private const int HitCapacity = 8;

        [SerializeField] private int probeStride = 1;

        [SerializeField] private float probeLift = 0.25f;

        [SerializeField] private float probeMetres = 1f;

        [SerializeField] private LayerMask probeLayers = ~0;

        [SerializeField] private bool skipSleeping = true;

        [SerializeField] private bool skipKinematic = true;

        [SerializeField] private RidingSettings settings;

        [SerializeField] private MonoBehaviour world;

        [SerializeField] private ReferenceFrameBinding binding = new();

        private readonly RaycastHit[] _hits = new RaycastHit[HitCapacity];
        private Rigidbody _body;
        private IReferenceFrameWorld _resolvedWorld;
        private int _stepsSinceProbe;

        public ReferenceFrameBinding Binding => binding;

        public IReferenceFrameWorld World { get; set; }

        private void OnEnable() {
            _body = GetComponent<Rigidbody>();
            _stepsSinceProbe = 0;
            _resolvedWorld = ResolveWorld();

            if (_resolvedWorld == null)
                Log.Error($"[ReferenceFrameRider] '{name}' found no reference frame world and records no binds.",
                    this);
        }

        private void FixedUpdate() {
            var time = Time.fixedTimeAsDouble;

            if (ProbeDue() && !Skipped())
                Probe(time);

            binding.Update(time);
        }

        private bool ProbeDue() {
            _stepsSinceProbe++;

            if (_stepsSinceProbe < probeStride)
                return false;

            _stepsSinceProbe = 0;

            return true;
        }

        private bool Skipped() =>
                (skipKinematic && _body.isKinematic) || (skipSleeping && !binding.IsBound && _body.IsSleeping());

        private void Probe(double time) {
            var source = FindSourceBelow();

            if (source == null)
                return;

            if (ReferenceEquals(source, binding.BoundFrame)) {
                binding.Sight(time);

                return;
            }

            binding.Bind(source, new Pose(_body.position, _body.rotation), time);

            if (_resolvedWorld != null)
                CharacterObservations.ReferenceFrameBound.Record(_resolvedWorld.Observations,
                    _resolvedWorld.CurrentTick, EntityId.ToULong(GetEntityId()));
        }

        private ReferenceFrameSource FindSourceBelow() {
            var origin = _body.position + Vector3.up * probeLift;
            var layers = settings != null ? settings.rideableLayers : probeLayers;
            var count = gameObject.scene.GetPhysicsScene().Raycast(origin, Vector3.down, _hits, probeLift + probeMetres,
                layers, QueryTriggerInteraction.Ignore);

            Collider nearest = null;
            var nearestDistance = float.PositiveInfinity;

            for (var index = 0; index < count; index++) {
                var hit = _hits[index];

                if (hit.collider.attachedRigidbody == _body || hit.distance >= nearestDistance)
                    continue;

                nearest = hit.collider;
                nearestDistance = hit.distance;
            }

            return nearest != null ? RegisteredSourceAbove(nearest.transform) : null;
        }

        private static ReferenceFrameSource RegisteredSourceAbove(Transform start) {
            for (var current = start; current != null; current = current.parent) {
                if (current.TryGetComponent<ReferenceFrameSource>(out var source) && source.IsRegistered)
                    return source;
            }

            return null;
        }

        private IReferenceFrameWorld ResolveWorld() {
            if (World != null)
                return World;

            if (world != null && world is IReferenceFrameWorld serialized)
                return serialized;

            return GetComponentInParent<IReferenceFrameWorld>();
        }
    }
}
