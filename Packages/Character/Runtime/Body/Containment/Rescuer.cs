// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Body {
    public sealed class Rescuer {
        private readonly ContainmentSettings _settings;
        private readonly RescueClaimDriver _driver = new();

        public Rescuer(ContainmentSettings settings) {
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
        }

        public void Rescue(Rigidbody body, MotionClaims motion, in RescueStep step, Vector3 worldUp) {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            if (motion == null)
                throw new ArgumentNullException(nameof(motion));

            var target = new Pose(step.Target.position, body.rotation);
            _driver.Aim(_settings.rescuePriority, target, worldUp);
            var claim = motion.Claim(_driver);

            body.position = target.position;
            body.transform.position = target.position;

            if (!body.isKinematic) {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            claim.Release();
        }
    }
}
