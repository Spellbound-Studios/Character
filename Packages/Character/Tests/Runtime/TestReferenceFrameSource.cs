// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Tests {
    public sealed class TestReferenceFrameSource : ReferenceFrameSource {
        public Vector3 LinearVelocity { get; set; }

        public Vector3 Spin { get; set; }

        public override Pose WorldPose => new(transform.position, transform.rotation);

        public override Vector3 AngularVelocity => Spin;

        public override Vector3 VelocityAt(Vector3 worldPoint) =>
                LinearVelocity + Vector3.Cross(Spin, worldPoint - transform.position);
    }
}
