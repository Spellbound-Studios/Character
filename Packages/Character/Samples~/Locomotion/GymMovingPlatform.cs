// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    [RequireComponent(typeof(Rigidbody))]
    public sealed class GymMovingPlatform : MonoBehaviour {
        [SerializeField, Tooltip("The first end of the platform's run, relative to where it starts.")]
        private Vector3 firstPoint = Vector3.zero;

        [SerializeField, Tooltip("The second end of the platform's run, relative to where it starts.")]
        private Vector3 secondPoint = new(6f, 0f, 0f);

        [SerializeField, Min(0f), Tooltip("Metres per second the platform travels between its two ends.")]
        private float speed = 1.5f;

        private Rigidbody _body;
        private Vector3 _origin;
        private bool _towardsSecond = true;

        public Vector3 FirstPoint {
            get => firstPoint;
            set => firstPoint = value;
        }

        public Vector3 SecondPoint {
            get => secondPoint;
            set => secondPoint = value;
        }

        public float Speed {
            get => speed;
            set => speed = Mathf.Max(0f, value);
        }

        public Rigidbody Body {
            get {
                if (_body == null)
                    _body = GetComponent<Rigidbody>();

                return _body;
            }
        }

        private void Awake() {
            Body.isKinematic = true;
            Body.useGravity = false;
            Body.interpolation = RigidbodyInterpolation.Interpolate;
            _origin = Body.position;
        }

        private void FixedUpdate() {
            var target = _origin + (_towardsSecond ? secondPoint : firstPoint);
            var next = Vector3.MoveTowards(Body.position, target, speed * Time.fixedDeltaTime);

            if ((next - target).sqrMagnitude <= Vector3.kEpsilon * Vector3.kEpsilon)
                _towardsSecond = !_towardsSecond;

            Body.MovePosition(next);
        }
    }
}
