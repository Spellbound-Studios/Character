// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.ReferenceFrames;
using UnityEngine;

namespace Spellbound.Character.Body {
    internal sealed class RescueClaimDriver : IMotionDriver {
        private Pose _target;
        private Vector3 _up;

        public int Priority { get; private set; }

        public bool NeedsSupport => false;

        public void Aim(int priority, Pose target, Vector3 up) {
            Priority = priority;
            _target = target;
            _up = up;
        }

        public void Begin(in Handoff entering) { }

        public Handoff End() => new(_target, Vector3.zero, ReferenceFrameId.World, _up);
    }
}
