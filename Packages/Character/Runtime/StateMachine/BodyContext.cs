// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.StateMachine {
    public class BodyContext {
        public BodyContext(
            Transform transform, Rigidbody rigidbody, ResizableCapsuleCollider capsule,
            ReferenceFrameBinding referenceFrame) {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));

            if (rigidbody == null)
                throw new ArgumentNullException(nameof(rigidbody));

            Transform = transform;
            Rigidbody = rigidbody;
            Capsule = capsule ?? throw new ArgumentNullException(nameof(capsule));
            ReferenceFrame = referenceFrame ?? throw new ArgumentNullException(nameof(referenceFrame));
        }

        public Transform Transform { get; }

        public Rigidbody Rigidbody { get; }

        public ResizableCapsuleCollider Capsule { get; }

        public ReferenceFrameBinding ReferenceFrame { get; }

        public GroundProbeResult Support { get; set; } = GroundProbeResult.Miss;

        public Vector3 DesiredMove { get; set; }

        public double ElapsedFixedTime { get; set; }
    }
}
