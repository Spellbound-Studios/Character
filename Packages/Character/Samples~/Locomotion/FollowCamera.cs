// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    public sealed class FollowCamera : MonoBehaviour {
        [SerializeField, Tooltip("The transform the camera follows.")]
        private Transform target;

        [SerializeField, Tooltip("Where the camera sits relative to the target, in world space.")]
        private Vector3 offset = new(0f, 2.5f, -5.5f);

        [SerializeField, Tooltip("Height above the target's origin the camera looks at.")]
        private float lookHeight = 1.2f;

        [SerializeField, Min(0f), Tooltip("Seconds the camera takes to catch up. 0 = locked to the target.")]
        private float smoothTime = 0.15f;

        private Vector3 _velocity;

        public Transform Target {
            get => target;
            set => target = value;
        }

        private void LateUpdate() {
            if (target == null)
                return;

            var goal = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, smoothTime);
            transform.LookAt(target.position + Vector3.up * lookHeight);
        }
    }
}
