// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    public enum AxisSpace {
        Local,
        World
    }

    public enum Axis {
        X,
        Y,
        Z
    }

    [DisallowMultipleComponent, RequireComponent(typeof(Collider)), RequireComponent(typeof(Rigidbody))]
    public class SwingingObstacle : MonoBehaviour {
        [Header("Target Filter"), SerializeField]
        private LayerMask targetLayers = ~0;

        [SerializeField] private float rehitCooldownSeconds = 0.15f;

        [Header("Impact Tuning"), SerializeField]
        private float impulseMultiplier = 10f;

        [SerializeField] private float minApproachSpeed = 0.5f;
        [SerializeField] private bool useForceAtPoint = true;

        [Header("Swing Setup"), SerializeField, Tooltip("Drag joint here.")]
        private Transform pivot;

        [SerializeField] private bool autoSetupHinge = true;
        [SerializeField] private AxisSpace axisSpace = AxisSpace.World;
        [SerializeField] private Axis swingAxis = Axis.Y;
        [SerializeField] private float initialAngularSpeedDeg;

        [Header("Rigidbody Defaults"), SerializeField]
        private float rbMass = 40f;

        [SerializeField] private float rbLinearDampening;
        [SerializeField] private float rbAngularDampening = 0.05f;
        [SerializeField] private int solverIterations = 12;
        [SerializeField] private int solverVelocityIterations = 12;

        [Header("Hinge Motor Settings"), SerializeField]
        private bool useMotor = true;

        [SerializeField] private float motorTargetVelocityDeg = 180f;
        [SerializeField] private float motorForce = 2000f;
        [SerializeField] private bool motorFreeSpin;

        private Rigidbody _rb;
        private Collider _col;
        private readonly Dictionary<Rigidbody, float> _lastHitTime = new();

        private void Awake() {
            _col = GetComponent<Collider>();
            _col.isTrigger = false;

            _rb = GetComponent<Rigidbody>();
            _rb.mass = rbMass;
            _rb.useGravity = true;
            _rb.linearDamping = rbLinearDampening;
            _rb.angularDamping = rbAngularDampening;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.constraints = RigidbodyConstraints.None;
            _rb.solverIterations = solverIterations;
            _rb.solverVelocityIterations = solverVelocityIterations;

            if (autoSetupHinge && pivot != null)
                EnsureHingeJoint();

            if (initialAngularSpeedDeg == 0f)
                return;

            var axisWorld = axisSpace == AxisSpace.World
                    ? AxisVectorWorld(swingAxis)
                    : transform.TransformDirection(AxisVectorLocal(swingAxis));
            _rb.angularVelocity = axisWorld.normalized * (initialAngularSpeedDeg * Mathf.Deg2Rad);

            _rb.sleepThreshold = 0f;

            var maxDeg = Mathf.Max(Mathf.Abs(initialAngularSpeedDeg), useMotor
                    ? Mathf.Abs(motorTargetVelocityDeg)
                    : 0f);

            if (maxDeg > 0f)
                _rb.maxAngularVelocity = Mathf.Max(_rb.maxAngularVelocity, maxDeg * Mathf.Deg2Rad + 1f);
        }

        private void FixedUpdate() {
            if (useMotor)
                return;

            if (Mathf.Approximately(initialAngularSpeedDeg, 0f))
                return;

            var axisWorld = axisSpace == AxisSpace.World
                    ? AxisVectorWorld(swingAxis)
                    : transform.TransformDirection(AxisVectorLocal(swingAxis));

            var axis = axisWorld.normalized;
            var target = initialAngularSpeedDeg * Mathf.Deg2Rad;
            var current = Vector3.Dot(_rb.angularVelocity, axis);
            var error = target - current;
            const float kP = 10f;
            _rb.AddTorque(axis * (kP * error), ForceMode.Acceleration);
        }

        private void EnsureHingeJoint() {
            var joint = GetComponent<HingeJoint>() ?? gameObject.AddComponent<HingeJoint>();
            joint.autoConfigureConnectedAnchor = false;

            var axisLocal = axisSpace == AxisSpace.World
                    ? transform.InverseTransformDirection(AxisVectorWorld(swingAxis))
                    : AxisVectorLocal(swingAxis);
            joint.axis = axisLocal.normalized;

            joint.anchor = transform.InverseTransformPoint(pivot.position);

            var pivotRb = pivot.GetComponent<Rigidbody>();

            if (pivotRb == null) {
                pivotRb = pivot.gameObject.AddComponent<Rigidbody>();
                pivotRb.isKinematic = true;
                pivotRb.useGravity = false;
                pivotRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }

            joint.connectedBody = pivotRb;
            joint.connectedAnchor = Vector3.zero;

            joint.useLimits = !(useMotor || Mathf.Abs(initialAngularSpeedDeg) > 0.01f);
            var limits = joint.limits;
            limits.min = -170f;
            limits.max = 170f;
            joint.limits = limits;
            joint.enablePreprocessing = false;

            if (!useMotor)
                return;

            var m = joint.motor;
            m.force = motorForce;
            m.targetVelocity = motorTargetVelocityDeg;
            m.freeSpin = motorFreeSpin;
            joint.motor = m;
            joint.useMotor = true;
        }

        private void OnCollisionEnter(Collision collision) {
            var otherRb = collision.rigidbody;

            if (otherRb == null)
                return;

            if ((targetLayers.value & (1 << otherRb.gameObject.layer)) == 0)
                return;

            if (_lastHitTime.TryGetValue(otherRb, out var tLast) && Time.time - tLast < rehitCooldownSeconds)
                return;

            var contact = collision.GetContact(0);
            var n = contact.normal;
            var p = contact.point;

            var vLog = _rb.GetPointVelocity(p);
            var vOther = otherRb.GetPointVelocity(p);
            var relative = vLog - vOther;

            var relativeSpeed = relative.magnitude;

            if (relativeSpeed <= minApproachSpeed)
                return;

            var deltaV = relativeSpeed * impulseMultiplier;

            if (useForceAtPoint)
                otherRb.AddForceAtPosition(n * deltaV, p, ForceMode.VelocityChange);
            else
                otherRb.AddForce(n * deltaV, ForceMode.VelocityChange);

            otherRb.WakeUp();

            _lastHitTime[otherRb] = Time.time;
        }

        private static Vector3 AxisVectorWorld(Axis a) =>
                a switch {
                    Axis.X => Vector3.right,
                    Axis.Y => Vector3.up,
                    _ => Vector3.forward
                };

        private static Vector3 AxisVectorLocal(Axis a) =>
                a switch {
                    Axis.X => Vector3.right,
                    Axis.Y => Vector3.up,
                    _ => Vector3.forward
                };
    }
}