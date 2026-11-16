// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Ground;
using Spellbound.Core.Logging;
using Spellbound.Core.Observations;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Core.Simulation;
using UnityEngine;

namespace Spellbound.Character.Body {
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(-30)]
    public sealed class Containment : MonoBehaviour {
        private const int RescueValueCount = 7;
        private const int RememberedRescueCount = 16;

        [SerializeField] private ContainmentSettings settings;

        [SerializeField] private MonoBehaviour world;

        [SerializeField] private bool skipSleeping = true;

        [SerializeField] private bool skipKinematic = true;

        private readonly List<(Vector3 From, RescueStep Step)> _lastRescues = new(RememberedRescueCount);

        private Rigidbody _body;
        private SupportGate _gate;
        private Collider[] _colliders = Array.Empty<Collider>();
        private PhysicsScene _physicsScene;
        private IReferenceFrameWorld _handedWorld;
        private IReferenceFrameWorld _resolvedWorld;
        private IContainmentSpace _space;
        private IRescueTarget _rescueTarget;
        private MotionClaims _motion;
        private IUpProvider _upProvider;
        private ISupportProbe _supportProbe;
        private RescueLadder _ladder;
        private Rescuer _rescuer;
        private int _stepsSinceCheck;
        private int _embeddedChecks;
        private bool _falling;
        private float _fallSeconds;
        private bool _missingSettingsReported;

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
            set {
                _space = value;
                _ladder = null;
            }
        }

        public IRescueTarget RescueTarget {
            get {
                if (_rescueTarget == null && settings != null)
                    _rescueTarget = new SettingsRescueTarget(settings);

                return _rescueTarget;
            }
            set {
                _rescueTarget = value;
                _ladder = null;
            }
        }

        public MotionClaims Motion {
            get => _motion ??= new MotionClaims();
            set => _motion = value;
        }

        public IMoveSource MoveSource { get; set; }

        public IUpProvider UpProvider {
            get => _upProvider ??= new ConstantUpProvider();
            set => _upProvider = value;
        }

        public ISupportProbe SupportProbe {
            get => _supportProbe ??= new PhysicsSupportProbe();
            set => _supportProbe = value;
        }

        public Vector3 Up {
            get {
                if (MoveSource != null) {
                    var desired = MoveSource.DesiredUp;

                    if (desired.sqrMagnitude > 0f)
                        return desired.normalized;
                }

                return UpProvider.UpAt(transform.position);
            }
        }

        public IReadOnlyList<(Vector3 From, RescueStep Step)> LastRescues => _lastRescues;

        private void OnEnable() {
            _body = GetComponent<Rigidbody>();
            TryGetComponent(out _gate);
            _colliders = GetComponentsInChildren<Collider>();
            _physicsScene = gameObject.scene.GetPhysicsScene();
            _stepsSinceCheck = 0;
            _embeddedChecks = 0;
            _falling = false;
            _fallSeconds = 0f;
            _missingSettingsReported = false;
            _resolvedWorld = ResolveWorld();
        }

        private void FixedUpdate() {
            if (settings == null) {
                ReportMissingSettings();

                return;
            }

            _stepsSinceCheck++;

            if (_stepsSinceCheck < settings.checkStride)
                return;

            _stepsSinceCheck = 0;
            Check();
        }

        internal bool IsSkipped() {
            if (skipSleeping && _body.IsSleeping())
                return true;

            if (skipKinematic && _body.isKinematic)
                return true;

            return _gate != null && _gate.IsHeld;
        }

        internal void Check() {
            if (IsSkipped() || settings == null)
                return;

            var worldUp = Up;
            var position = _body.position;
            var bounds = BodyBounds();
            var occupancy = Space.Classify(in bounds);
            var reason = Read(position, worldUp, occupancy);
            var context = Observations();
            var tick = CurrentTick();
            var subject = EntityId.ToULong(GetEntityId());

            if (reason == ContainmentReason.None && occupancy == Occupancy.Empty && IsSupported(in bounds, worldUp))
                Ladder().RememberSupported(position);

            var verdict = CharacterObservations.ContainmentRepair.Check(context, reason == ContainmentReason.None,
                subject, tick);

            if (verdict == GuardVerdict.Repair)
                Rescue(context, tick, subject, position, worldUp, bounds.extents, reason);
        }

        private ContainmentReason Read(Vector3 position, Vector3 worldUp, Occupancy occupancy) {
            var belowFloor = settings.hasFloor && Vector3.Dot(position, worldUp) < settings.floorHeight;

            _embeddedChecks = occupancy == Occupancy.Solid ? _embeddedChecks + 1 : 0;
            var embedded = _embeddedChecks >= settings.embeddedChecksBeforeRescue;

            var sustainedFall = ReadFall(worldUp);

            if (belowFloor)
                return ContainmentReason.BelowFloor;

            if (embedded)
                return ContainmentReason.Embedded;

            return sustainedFall ? ContainmentReason.SustainedFall : ContainmentReason.None;
        }

        private bool ReadFall(Vector3 worldUp) {
            var descentSpeed = -Vector3.Dot(_body.linearVelocity, worldUp);

            if (!(descentSpeed > settings.sustainedFallSpeed)) {
                _falling = false;
                _fallSeconds = 0f;

                return false;
            }

            if (_falling) {
                _fallSeconds += Time.fixedDeltaTime * settings.checkStride;
            }
            else {
                _falling = true;
                _fallSeconds = 0f;
            }

            return _fallSeconds >= settings.sustainedFallSeconds;
        }

        private bool IsSupported(in Bounds bounds, Vector3 worldUp) {
            var extents = bounds.extents;
            var halfHeight = Mathf.Abs(extents.x * worldUp.x) + Mathf.Abs(extents.y * worldUp.y) +
                             Mathf.Abs(extents.z * worldUp.z);
            var support = SupportProbe.Probe(_physicsScene, bounds.center, -worldUp,
                halfHeight + settings.supportReachMetres, 0f, settings.supportLayers);

            return support.HasHit;
        }

        private void Rescue(
            ObservationContext context, Tick tick, ulong subject, Vector3 from, Vector3 worldUp, Vector3 bodyExtents,
            ContainmentReason reason) {
            var ladder = Ladder();

            if (ladder == null)
                return;

            var step = ladder.Resolve(from, worldUp, bodyExtents, reason);
            _rescuer ??= new Rescuer(settings);
            _rescuer.Rescue(_body, Motion, in step, worldUp);
            _embeddedChecks = 0;
            _falling = false;
            _fallSeconds = 0f;
            Remember(from, in step);

            var to = step.Target.position;
            Span<uint> values = stackalloc uint[RescueValueCount];
            values[0] = (uint)reason;
            values[1] = FloatBits(from.x);
            values[2] = FloatBits(from.y);
            values[3] = FloatBits(from.z);
            values[4] = FloatBits(to.x);
            values[5] = FloatBits(to.y);
            values[6] = FloatBits(to.z);
            CharacterObservations.ContainmentRescue.Record(context, tick, subject, values);
        }

        private void Remember(Vector3 from, in RescueStep step) {
            if (_lastRescues.Count == RememberedRescueCount)
                _lastRescues.RemoveAt(0);

            _lastRescues.Add((from, step));
        }

        private RescueLadder Ladder() {
            if (_ladder != null)
                return _ladder;

            var target = RescueTarget;

            if (target == null)
                return null;

            _ladder = new RescueLadder(settings, Space, target);

            return _ladder;
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

        private ObservationContext Observations() => _resolvedWorld?.Observations ?? ObservationContext.Discard;

        private Tick CurrentTick() => _resolvedWorld?.CurrentTick ?? Tick.Zero;

        private static uint FloatBits(float value) => unchecked((uint)BitConverter.SingleToInt32Bits(value));

        private void ReportMissingSettings() {
            if (_missingSettingsReported)
                return;

            _missingSettingsReported = true;
            Log.Error($"[Containment] '{name}' has no containment settings and checks nothing.", this);
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
