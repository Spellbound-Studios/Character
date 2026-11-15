// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.ReferenceFrames;
using UnityEngine;

namespace Spellbound.Character.Body {
    [Serializable]
    public sealed class ReferenceFrameBinding {
        [SerializeField,
         Tooltip("Seconds a frame stays bound after its last sighting. Higher = rides out longer gaps in contact; " +
                 "lower = lets go sooner.")]
        private double holdSeconds = 0.5;

        private double _lastSighting;

        public double HoldSeconds {
            get => FiniteNonNegative(holdSeconds);
            set => holdSeconds = FiniteNonNegative(value);
        }

        public IReferenceFrame BoundFrame { get; private set; }

        public bool IsBound => BoundFrame != null;

        public ReferenceFramePose LocalPose { get; private set; }

        public void Bind(IReferenceFrame frame, Pose worldPose, double time) {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            BoundFrame = frame;
            LocalPose = ReferenceFramePose.FromWorld(worldPose, frame);
            _lastSighting = time;
        }

        public void Sight(double time) {
            if (IsBound && time > _lastSighting)
                _lastSighting = time;
        }

        public void Release() {
            BoundFrame = null;
            LocalPose = default;
        }

        public void Update(double time) {
            if (IsBound && (time < _lastSighting || time - _lastSighting > HoldSeconds))
                Release();
        }

        private static double FiniteNonNegative(double value) =>
                double.IsNaN(value) || double.IsInfinity(value) || value < 0.0
                        ? 0.0
                        : value;

        public Vector3 WorldVelocityAt(Vector3 point) {
            if (!IsBound)
                return Vector3.zero;

            return BoundFrame.VelocityAt(point);
        }

        public Quaternion Turn(Quaternion bodyRotation, double stepSeconds) {
            if (!IsBound || double.IsNaN(stepSeconds) || double.IsInfinity(stepSeconds) || stepSeconds < 0.0)
                return bodyRotation;

            var angularVelocity = BoundFrame.AngularVelocity;
            var radiansPerSecond = angularVelocity.magnitude;
            if (radiansPerSecond <= Vector3.kEpsilon)
                return bodyRotation;

            var degrees = (float)(radiansPerSecond * stepSeconds * Mathf.Rad2Deg);
            return Quaternion.AngleAxis(degrees, angularVelocity / radiansPerSecond) * bodyRotation;
        }

        public Vector3 WorldVelocity(Vector3 relative, Vector3 point) => relative + WorldVelocityAt(point);
    }
}
