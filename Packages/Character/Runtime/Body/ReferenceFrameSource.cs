// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.ReferenceFrames;
using UnityEngine;

namespace Spellbound.Character.Body {
    [DefaultExecutionOrder(-100)]
    public abstract class ReferenceFrameSource : MonoBehaviour, IReferenceFrame {
        [SerializeField] private ulong referenceFrameId;

        [SerializeField] private MonoBehaviour world;

        public IReferenceFrameWorld World { get; set; }

        public ReferenceFrameId Id => new(referenceFrameId);

        public bool IsRegistered { get; private set; }

        public abstract Pose WorldPose { get; }

        public abstract Vector3 AngularVelocity { get; }

        public abstract Vector3 VelocityAt(Vector3 worldPoint);

        protected virtual void OnEnable() {
        }

        protected virtual void OnDisable() {
        }
    }
}
