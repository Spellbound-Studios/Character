// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Logging;
using Spellbound.Core.Observations;
using Spellbound.Core.Simulation;
using UnityEngine;

namespace Spellbound.Character.Body {
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(-45)]
    public sealed class SupportGate : MonoBehaviour {
        private const int RescueValueCount = 7;

        [SerializeField] private ContainmentSettings settings;

        [SerializeField] private MonoBehaviour world;

        private Rigidbody _body;
        private Collider[] _colliders = Array.Empty<Collider>();
        private HoldDriver _holdDriver;
        private MotionClaim _holdClaim;
        private IReferenceFrameWorld _handedWorld;
        private IReferenceFrameWorld _resolvedWorld;
        private IContainmentSpace _space;
        private IRescueTarget _rescueTarget;
        private MotionClaims _motion;
        private bool _savedKinematic;
        private bool _savedGravity;
        private bool _missingSettingsReported;
        private bool _missingWorldReported;

        public IReferenceFrameWorld World {
            get => _handedWorld;
            set {
                _handedWorld = value;

                if (isActiveAndEnabled)
                    _resolvedWorld = ResolveWorld();
            }
        }

        public IContainmentSpace Space {
            get => _space ??= new PhysicsContainmentSpace();
            set => _space = value;
        }

        public IRescueTarget RescueTarget {
            get {
                if (_rescueTarget == null && settings != null)
                    _rescueTarget = new SettingsRescueTarget(settings);

                return _rescueTarget;
            }
            set => _rescueTarget = value;
        }

        public MotionClaims Motion {
            get => _motion ??= new MotionClaims();
            set => _motion = value;
        }

        public bool IsHeld { get; private set; }

        public Tick HeldSince { get; private set; }

        private void OnEnable() {
            _body = GetComponent<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>();
            _holdDriver ??= new HoldDriver(_body);
            _missingSettingsReported = false;
            _missingWorldReported = false;
            _resolvedWorld = ResolveWorld();
        }

        private void OnDisable() {
            if (IsHeld)
                Release();
        }

        private void FixedUpdate() {
            if (settings == null) {
                ReportMissingSettings();

                return;
            }

            if (_resolvedWorld == null)
                ReportMissingWorld();

            var bounds = BodyBounds();
            var ready = Space.IsReady(in bounds);

            if (!IsHeld) {
                if (!ready && ActiveDriverAllowsHold())
                    Hold(CurrentTick());

                return;
            }

            var context = Observations();
            var tick = CurrentTick();
            var heldSeconds = HeldSeconds(context, tick);
            var verdict = CharacterObservations.SupportHeld.Check(context,
                ready || heldSeconds < settings.supportHoldTimeoutSeconds, EntityId.ToULong(GetEntityId()), tick);

            if (ready) {
                Release();

                return;
            }

            if (verdict == GuardVerdict.Repair)
                MoveToFallback(context, tick);
        }

        private bool ActiveDriverAllowsHold() {
            var active = Motion.Active;

            return active == null || active.NeedsSupport;
        }

        private void Hold(Tick tick) {
            _savedKinematic = _body.isKinematic;
            _savedGravity = _body.useGravity;

            if (!_body.isKinematic) {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }

            _body.isKinematic = true;
            _body.useGravity = false;
            _holdDriver.Priority = settings.rescuePriority;
            _holdClaim = Motion.Claim(_holdDriver);
            HeldSince = tick;
            IsHeld = true;
        }

        private void Release() {
            _body.isKinematic = _savedKinematic;
            _body.useGravity = _savedGravity;
            _holdClaim.Release();
            _holdClaim = default;
            IsHeld = false;
        }

        private void MoveToFallback(ObservationContext context, Tick tick) {
            var target = RescueTarget;

            if (target == null)
                return;

            var from = _body.position;
            var to = target.Fallback(from).position;
            _body.position = to;
            transform.position = to;
            HeldSince = tick;

            Span<uint> values = stackalloc uint[RescueValueCount];
            values[0] = (uint)ContainmentReason.SupportTimeout;
            values[1] = FloatBits(from.x);
            values[2] = FloatBits(from.y);
            values[3] = FloatBits(from.z);
            values[4] = FloatBits(to.x);
            values[5] = FloatBits(to.y);
            values[6] = FloatBits(to.z);
            CharacterObservations.ContainmentRescue.Record(context, tick, EntityId.ToULong(GetEntityId()), values);
        }

        private Bounds BodyBounds() {
            var hasBounds = false;
            var bounds = new Bounds(_body.position, Vector3.zero);

            foreach (var bodyCollider in _colliders) {
                if (bodyCollider == null || !bodyCollider.enabled)
                    continue;

                if (hasBounds) {
                    bounds.Encapsulate(bodyCollider.bounds);
                }
                else {
                    bounds = bodyCollider.bounds;
                    hasBounds = true;
                }
            }

            return bounds;
        }

        private double HeldSeconds(ObservationContext context, Tick tick) =>
                tick > HeldSince ? context.Rate.ToSeconds(HeldSince.Until(tick)) : 0d;

        private ObservationContext Observations() => _resolvedWorld?.Observations ?? ObservationContext.Discard;

        private Tick CurrentTick() => _resolvedWorld?.CurrentTick ?? Tick.Zero;

        private static uint FloatBits(float value) => unchecked((uint)BitConverter.SingleToInt32Bits(value));

        private void ReportMissingSettings() {
            if (_missingSettingsReported)
                return;

            _missingSettingsReported = true;
            Log.Error($"[SupportGate] '{name}' has no containment settings and holds nothing.", this);
        }

        private void ReportMissingWorld() {
            if (_missingWorldReported)
                return;

            _missingWorldReported = true;
            Log.Error($"[SupportGate] '{name}' found no reference frame world and never times a hold out.", this);
        }

        private IReferenceFrameWorld ResolveWorld() {
            if (_handedWorld != null)
                return _handedWorld;

            if (world != null && world is IReferenceFrameWorld serialized)
                return serialized;

            return GetComponentInParent<IReferenceFrameWorld>();
        }
    }
}
