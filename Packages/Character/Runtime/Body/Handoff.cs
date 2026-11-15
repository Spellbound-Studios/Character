// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.ReferenceFrames;
using UnityEngine;

namespace Spellbound.Character.Body {
    public readonly struct Handoff {
        public Handoff(Pose worldPose, Vector3 worldVelocity, ReferenceFrameId boundFrame, Vector3 up) {
            WorldPose = worldPose;
            WorldVelocity = worldVelocity;
            BoundFrame = boundFrame;
            Up = up;
        }

        public Pose WorldPose { get; }

        public Vector3 WorldVelocity { get; }

        public ReferenceFrameId BoundFrame { get; }

        public Vector3 Up { get; }

        public static Handoff AtRest(Pose worldPose, Vector3 up) =>
                new(worldPose, Vector3.zero, ReferenceFrameId.World, up);
    }
}
