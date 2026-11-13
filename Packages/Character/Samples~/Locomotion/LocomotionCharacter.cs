// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using Spellbound.Character.Rig;
using Spellbound.Character.StateMachine;
using Spellbound.StateMachine;
using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class LocomotionCharacter : MonoBehaviour, IPlayableCharacter {
        [SerializeField, Tooltip("The state assets this character can be handed. They fill the character's own " +
                                 "state registry.")]
        private StateCatalogue states;

        [SerializeField, Tooltip("The state asset in each locomotion slot, and the slot the character starts in.")]
        private StateMachineConfiguration<LocomotionStateKey> configuration = new();

        [SerializeField] private ResizableCapsuleCollider capsule = new();

        [SerializeField] private ReferenceFrameBinding referenceFrame = new();

        [SerializeField, Tooltip("The model's Animator. Empty = no animation.")]
        private Animator animator;

        [SerializeField, Tooltip("The transform move input is relative to, usually the camera. Empty = world axes.")]
        private Transform moveReference;

        [SerializeField, Tooltip("The layers the support probe stands on.")]
        private LayerMask supportLayers = Physics.DefaultRaycastLayers;

        private global::Spellbound.StateMachine.StateMachine<BodyContext, LocomotionStateKey> _machine;
        private LocomotionContext _context;

        public ILocomotionInput LocomotionInput { get; set; }

        public IStateMachine Machine => _machine;

        public BodyContext Body => _context;

        public StateRegistry Registry { get; private set; }

        public StateCatalogue States {
            get => states;
            set => states = value;
        }

        public StateMachineConfiguration<LocomotionStateKey> Configuration {
            get => configuration;
            set => configuration = value;
        }

        public Transform MoveReference {
            get => moveReference;
            set => moveReference = value;
        }

        private void Awake() {
            var body = GetComponent<Rigidbody>();
            body.freezeRotation = true;

            capsule.Initialize(gameObject, null);
            capsule.CalculateCapsuleColliderDimensions();

            if (LocomotionInput == null && TryGetComponent(out ILocomotionInput input))
                LocomotionInput = input;

            var animation = animator != null ? new AnimatorAnimationController(animator) : null;
            _context = new LocomotionContext(transform, body, capsule, referenceFrame, animation, ChangeState);

            Registry = states != null ? new StateRegistry(states) : new StateRegistry();
            _machine = CharacterStateMachineKit.Create(_context, configuration);
        }

        private void Update() {
            if (LocomotionInput != null) {
                _context.DesiredMove = ToWorld(LocomotionInput.Move);
                _context.RunRequested = LocomotionInput.Run;

                if (LocomotionInput.JumpPressed)
                    _context.JumpRequested = true;
            }
            else {
                _context.DesiredMove = Vector3.zero;
                _context.RunRequested = false;
            }

            _machine.UpdateStateMachine();
        }

        private void FixedUpdate() {
            _context.ElapsedFixedTime = Time.fixedTimeAsDouble;
            referenceFrame.Update(_context.ElapsedFixedTime);
            _context.Support = ProbeSupport();
            _machine.FixedUpdateStateMachine();
            _context.JumpRequested = false;
        }

        private void OnDestroy() => _machine?.Dispose();

        private void ChangeState(LocomotionStateKey slot) => _machine.ChangeState(slot);

        private GroundProbeResult ProbeSupport() {
            var floatData = capsule.CapsuleFloatData;

            return capsule.ProbeGround(-transform.up, floatData.RideHeight + floatData.GroundedTolerance,
                supportLayers);
        }

        private Vector3 ToWorld(Vector2 move) {
            if (moveReference != null)
                return VelocityMath.GetInputDirectionRelativeToCamera(move, moveReference, transform.up);

            return Vector3.ClampMagnitude(new Vector3(move.x, 0f, move.y), 1f);
        }
    }
}
