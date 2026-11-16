// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Samples.SwimAndFly {
    [Serializable]
    public abstract class SampleMotionDriver : IMotionDriver {
        private Rigidbody _body;
        private Vector3 _up;

        public abstract int Priority { get; }

        public abstract bool NeedsSupport { get; }

        public Handoff Entered { get; private set; }

        public void Begin(in Handoff entering) {
            Entered = entering;
            _up = entering.Up;
        }

        public Handoff End() {
            if (_body == null)
                return Entered;

            return new Handoff(new Pose(_body.position, _body.rotation), _body.linearVelocity, Entered.BoundFrame,
                _up);
        }

        public void Move(Rigidbody body, IMoveSource source, float stepSeconds) {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            _body = body;
            var desiredUp = source.DesiredUp;

            if (desiredUp.sqrMagnitude > 0f)
                _up = desiredUp.normalized;

            body.linearVelocity = Velocity(body, source.DesiredMove, _up, stepSeconds);
        }

        protected abstract Vector3 Velocity(Rigidbody body, Vector3 desiredMove, Vector3 up, float stepSeconds);
    }
}
