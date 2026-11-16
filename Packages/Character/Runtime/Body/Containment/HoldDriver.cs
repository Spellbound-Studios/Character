// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.ReferenceFrames;
using UnityEngine;

namespace Spellbound.Character.Body {
    internal sealed class HoldDriver : IMotionDriver {
        private readonly Rigidbody _body;
        private Vector3 _up;

        public HoldDriver(Rigidbody body) {
            _body = body;
        }

        public int Priority { get; set; }

        public bool NeedsSupport => false;

        public void Begin(in Handoff entering) {
            _up = entering.Up;
        }

        public Handoff End() =>
                new(new Pose(_body.position, _body.rotation), Vector3.zero, ReferenceFrameId.World, _up);
    }
}
