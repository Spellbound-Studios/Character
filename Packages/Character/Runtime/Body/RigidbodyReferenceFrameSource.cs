// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RigidbodyReferenceFrameSource : ReferenceFrameSource {
        private Rigidbody _body;

        private Rigidbody Body {
            get {
                if (_body == null)
                    _body = GetComponent<Rigidbody>();

                return _body;
            }
        }

        public override Pose WorldPose => new(Body.position, Body.rotation);

        public override Vector3 AngularVelocity => Body.angularVelocity;

        public override Vector3 VelocityAt(Vector3 worldPoint) => Body.GetPointVelocity(worldPoint);
    }
}
