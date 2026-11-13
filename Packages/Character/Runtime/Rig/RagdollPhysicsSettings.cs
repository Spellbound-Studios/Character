// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Rig {
    [Serializable]
    public class RagdollPhysicsSettings {
        public int SolverIterations = 8;
        public int SolverVelocityIterations = 2;
        public float LinearDamping = 0.05f;
        public float AngularDamping = 0.5f;
        public float MaxAngularVelocity = 20f;
        public float MaxLinearVelocity = 30f;
        public float MaxDepenetrationVelocity = 5f;
        public RigidbodyInterpolation Interpolation = RigidbodyInterpolation.Interpolate;
        public CollisionDetectionMode CollisionDetection = CollisionDetectionMode.ContinuousSpeculative;
        public LayerMask ExcludeLayers;
        public bool IgnoreSelfCollision = true;
        public bool EnableProjection = true;
        public float ProjectionDistance = 0.05f;
        public float ProjectionAngle = 30f;
        public bool EnablePreprocessing = true;
    }
}
