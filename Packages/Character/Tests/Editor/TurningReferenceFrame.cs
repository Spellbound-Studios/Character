// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.ReferenceFrames;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public sealed class TurningReferenceFrame : IReferenceFrame {
        public TurningReferenceFrame(ulong id, Pose worldPose, Vector3 linearVelocity, Vector3 angularVelocity) {
            Id = new ReferenceFrameId(id);
            WorldPose = worldPose;
            LinearVelocity = linearVelocity;
            AngularVelocity = angularVelocity;
        }

        public ReferenceFrameId Id { get; }

        public Pose WorldPose { get; set; }

        public Vector3 LinearVelocity { get; }

        public Vector3 AngularVelocity { get; }

        public Vector3 VelocityAt(Vector3 worldPoint) =>
                LinearVelocity + Vector3.Cross(AngularVelocity, worldPoint - WorldPose.position);
    }
}
