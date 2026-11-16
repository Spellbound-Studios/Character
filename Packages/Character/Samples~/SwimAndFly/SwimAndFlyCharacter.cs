// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using Spellbound.Core.ReferenceFrames;
using UnityEngine;

namespace Spellbound.Character.Samples.SwimAndFly {
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(-40)]
    public sealed class SwimAndFlyCharacter : MonoBehaviour, IMoveSource {
        [SerializeField] private SwimDriver swim = new();

        [SerializeField] private FlyDriver fly = new();

        [SerializeField, Tooltip("The transform move input is relative to, usually the camera. Empty = world axes.")]
        private Transform moveReference;

        private readonly MotionClaims _motion = new();
        private Rigidbody _body;
        private IUpProvider _upProvider;
        private MotionClaim _swimClaim;
        private MotionClaim _flyClaim;
        private bool _swimClaimed;
        private bool _flyClaimed;

        public ISwimAndFlyInput SwimAndFlyInput { get; set; }

        public IUpProvider UpProvider {
            get => _upProvider ??= new ConstantUpProvider();
            set => _upProvider = value;
        }

        public Vector3 DesiredMove { get; private set; }

        public Vector3 DesiredUp => UpProvider.UpAt(transform.position);

        public MotionClaims Motion => _motion;

        public SwimDriver Swim => swim;

        public FlyDriver Fly => fly;

        public Transform MoveReference {
            get => moveReference;
            set => moveReference = value;
        }

        private void Awake() {
            _body = GetComponent<Rigidbody>();
            _body.useGravity = false;
            _body.freezeRotation = true;

            if (SwimAndFlyInput == null && TryGetComponent(out ISwimAndFlyInput input))
                SwimAndFlyInput = input;

            if (TryGetComponent(out Containment containment)) {
                containment.Motion = _motion;
                containment.MoveSource = this;
            }
        }

        private void OnEnable() {
            _swimClaim = _motion.Claim(swim);
            _swimClaimed = true;
        }

        private void OnDisable() {
            ReleaseFly();

            if (!_swimClaimed)
                return;

            _swimClaim.Release();
            _swimClaimed = false;
        }

        private void FixedUpdate() {
            var flyHeld = SwimAndFlyInput != null && SwimAndFlyInput.FlyHeld;

            if (flyHeld && !_flyClaimed) {
                _flyClaim = _motion.Claim(fly);
                _flyClaimed = true;
            }
            else if (!flyHeld) {
                ReleaseFly();
            }

            var up = DesiredUp;
            var planar = ToWorld(SwimAndFlyInput?.Move ?? Vector2.zero, up);
            DesiredMove = flyHeld ? Vector3.ClampMagnitude(planar + up, 1f) : planar;

            if (_motion.Active is SampleMotionDriver driver)
                driver.Move(_body, this, Time.fixedDeltaTime);
        }

        private void ReleaseFly() {
            if (!_flyClaimed)
                return;

            _flyClaim.Release();
            _flyClaimed = false;
        }

        private Vector3 ToWorld(Vector2 move, Vector3 up) {
            if (moveReference == null)
                return Vector3.ClampMagnitude(new Vector3(move.x, 0f, move.y), 1f);

            var forward = Vector3.ProjectOnPlane(moveReference.forward, up).normalized;
            var right = Vector3.Cross(up, forward);

            return Vector3.ClampMagnitude(right * move.x + forward * move.y, 1f);
        }
    }
}
