// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.ReferenceFrames;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public sealed class ShearingReferenceFrame : IReferenceFrame {
        public ShearingReferenceFrame(ulong id, Pose worldPose, Vector3 velocityPerMetre) {
            Id = new ReferenceFrameId(id);
            WorldPose = worldPose;
            VelocityPerMetre = velocityPerMetre;
        }

        public ReferenceFrameId Id { get; }

        public Pose WorldPose { get; }

        public Vector3 VelocityPerMetre { get; }

        public Vector3 AngularVelocity => Vector3.zero;

        public Vector3 VelocityAt(Vector3 worldPoint) => VelocityPerMetre * (worldPoint.x - WorldPose.position.x);
    }
}
