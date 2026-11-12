// Copyright 2025 Spellbound Studio Inc.

using System;
using Spellbound.Core.Logging;
using Spellbound.Core.Tooling;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// This example is meant to be a semi-complete game ready drop in or rubric for any aspiring game dev to use as
    /// a reference or copy and paste. Our intention is to give you an idea of how to structure your MonoBehaviour to
    /// have it interact and direct a state machine based on your player's actions in-game. Please note that the getters
    /// and setters you see below are just preferences and inspired by Indie Wafflus. We make no claim to this being the
    /// best or a one-size-fit-all controller. However, we do think it will give you a good foundation to stand on and
    /// grow from whether you're looking to make a highly custom controller or just a controller that feels good.
    ///
    /// This example is also meant to show you how to create n-number of semi-orthogonal state machines that can
    /// compliment and react to one another. This really provides the user with a lot of control and allows the user to
    /// encapsulate states or expose them however they want to. For instance, I could make it so that the swimming state
    /// makes it so that the player can't do any combat or do a specific type of combat - the choice is yours!
    ///
    /// Please reference the documentation for additional details, or please feel free to use the discord and use this
    /// as a reference to see how others leverage these tools and solve game-specific challenges.
    /// </summary>
    public sealed class PlayerControllerExample : MonoBehaviour, IDebuggingInfo, IAnimationController {
        [Header("Input Reference:")]
        [field: SerializeField]
        public ExampleInputManager ExampleInput { get; private set; }

        [Header("Camera:")]
        [SerializeField,
         Tooltip("Lock and hide the cursor on start for mouse-look. On = locked; off = free cursor.")]
        private bool lockCursorOnStart = true;

        private ICameraRig _cameraRig;

        private Transform _tr;

        [Header("Rigidbody Reference:")]
        [field: SerializeField]
        public Rigidbody Rb { get; private set; }

        [Header("Collider Settings:")]
        [field: SerializeField]
        public ResizableCapsuleCollider ResizableCapsuleCollider { get; private set; }

        [Header("Layer Settings:")]
        [field: SerializeField]
        public LayerData LayerData { get; private set; }

        [Header("Rigidbody Settings:")]
        [field: SerializeField]
        public RigidbodyData RigidbodyData { get; private set; }

        [Header("Character Rotation Settings:")]
        [field: SerializeField]
        public RotationData RotationData { get; private set; }

        [Header("Stat Settings:")]
        [field: SerializeField]
        public StatData StatData { get; private set; }

        public StateMachine<PlayerControllerExample, LocoStateTypes> LocoStateMachine { get; private set; }
        public StateMachine<PlayerControllerExample, ActionStateTypes> ActionStateMachine { get; private set; }

        [Header("State Configs")]
        [SerializeField] private LocoStateConfigExample locoConfig;

        [SerializeField] private ActionStateConfigExample actionConfig;

        [Header("Animator"), SerializeField] private Animator animator;

        [SerializeField, Tooltip("Float param the locomotion blend trees read as their normalized 0..1 axis.")]
        private string blendParam = "Blend";

        [SerializeField, Tooltip("Float param wired to the locomotion state's speed multiplier (stride/foot-slide warp).")]
        private string speedWarpParam = "SpeedMul";

        [SerializeField, Min(0f), Tooltip("Default crossfade duration (seconds) used when a caller passes 0.")]
        private float defaultFade = 0.2f;

        /// <summary>Base locomotion layer index — what this sample assigns layer 0 to mean.</summary>
        public const int BaseLayer = 0;

        /// <summary>Masked upper-body action layer index — what this sample assigns layer 1 to mean.</summary>
        public const int ActionLayer = 1;

        private int _blendHash;
        private int _speedWarpHash;
        private float _actionWeight;
        private float _actionTarget;
        private float _actionRate;

        /// <summary>
        /// The animation seam states drive. This controller is its own backend — a plain (non-networked) Animator;
        /// a game would supply a networked implementation of the same interface instead.
        /// </summary>
        public IAnimationController Animation => this;

        /// <summary>Hash of the locomotion blend axis param, for states feeding <see cref="AnimationMath.NormalizeBlend"/>.</summary>
        public int LocomotionBlend => _blendHash;

        /// <summary>Hash of the speed-warp multiplier param, for states feeding <see cref="AnimationMath.SpeedWarp"/>.</summary>
        public int SpeedWarp => _speedWarpHash;

        // What direction is up from the player?
        public Vector3 PlanarUp { get; private set; }

        public Transform ReferenceTransform => _aimCore != null
                ? _aimCore.transform
                : null;

        public ICameraRig CameraRig => _cameraRig;

        public AimCore AimCore => _aimCore;

        private AimCore _aimCore;

        private void Awake() {
            _tr = transform;
            PlanarUp = _tr.up;

            if (ExampleInput == null) {
                if (!SingletonManager.TryGetSingletonInstance<ExampleInputManager>(out var im)) {
                    Log.Error("ExampleInput is missing in the scene most likely.");
                    return;
                }

                ExampleInput = im;
            }

            Rb = GetComponent<Rigidbody>();
            Rb.freezeRotation = true;
            Rb.useGravity = true;
            Rb.interpolation = RigidbodyInterpolation.Interpolate;

            ResizableCapsuleCollider.Initialize(gameObject);
            ResizableCapsuleCollider.CalculateCapsuleColliderDimensions();
        }

        private void Start() {
            InitAnimation();
            ConfigureStateMachines();
            InitCamera();
        }

        public void Update() {
            LocoStateMachine.UpdateStateMachine();
            ActionStateMachine.UpdateStateMachine();
            TickActionLayer(Time.deltaTime);
        }

        public void FixedUpdate() {
            LocoStateMachine.FixedUpdateStateMachine();
            ActionStateMachine.FixedUpdateStateMachine();
        }
        
        private void OnDestroy() {
            LocoStateMachine?.Dispose();
            ActionStateMachine?.Dispose();
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (ResizableCapsuleCollider == null || !TryGetComponent(out CapsuleCollider _))
                return;

            ResizableCapsuleCollider.Initialize(gameObject);
            ResizableCapsuleCollider.CalculateCapsuleColliderDimensions();
        }
#endif

        /// <summary>
        /// Hands this character's aim core to the rig as the follow target. The core owns the look rotation;
        /// the rig's cameras attach to it, so every camera agrees on where the player is looking.
        /// </summary>
        private void InitCamera() {
            if (!SingletonManager.TryGetSingletonInstance<ICameraRig>(out _cameraRig)) {
                Log.Error("CameraRigManager is missing from the scene.");
                return;
            }

            _aimCore = GetComponentInChildren<AimCore>();

            if (_aimCore == null) {
                Log.Error("Player has no AimCore child; the rig has nothing to follow.");
                return;
            }

            Cursor.lockState = lockCursorOnStart
                    ? CursorLockMode.Locked
                    : CursorLockMode.None;

            _cameraRig.SetFollowTarget(_aimCore.transform);
        }

        public void SetCameraFollowMouse(bool follow) {
            if (ExampleInput != null)
                ExampleInput.CameraInputSuppressed = !follow;
        }

        public string SwitchCamera(string cameraName) {
            if (_cameraRig == null)
                return null;

            var previous = _cameraRig.Current;

            return _cameraRig.Switch(cameraName)
                    ? previous
                    : null;
        }

        /// <summary>
        /// Caches the parameter hashes states drive. The Animator itself owns the states, blend trees, layers, and
        /// mask — this controller only feeds it crossfades, params, and layer weights through <see cref="Animation"/>.
        /// </summary>
        private void InitAnimation() {
            _blendHash = Animator.StringToHash(blendParam);
            _speedWarpHash = Animator.StringToHash(speedWarpParam);

            if (animator == null)
                Log.Error("Animator is missing; animation calls will be ignored until one is assigned.");
        }

        /// <inheritdoc />
        public void CrossFade(int stateHash, int layer = 0, float fade = 0f) {
            if (animator != null)
                animator.CrossFadeInFixedTime(stateHash, fade > 0f ? fade : defaultFade, layer);
        }

        /// <inheritdoc />
        public void SetFloat(int paramHash, float value) {
            if (animator != null)
                animator.SetFloat(paramHash, value);
        }

        /// <inheritdoc />
        public void SetLayerWeight(int layer, float weight) {
            if (animator != null)
                animator.SetLayerWeight(layer, weight);
        }

        /// <summary>
        /// Eases the masked <see cref="ActionLayer"/> toward <paramref name="target"/> (0..1) over
        /// <paramref name="fade"/> seconds — the sample's masking convention, ramped in Update over the generic
        /// <see cref="SetLayerWeight"/>. Action states call this to fade the upper body in and out.
        /// </summary>
        public void FadeActionLayer(float target, float fade = 0f) {
            _actionTarget = Mathf.Clamp01(target);
            var f = fade > 0f
                    ? fade
                    : defaultFade;
            _actionRate = f > 0f
                    ? 1f / f
                    : float.PositiveInfinity;
        }

        /// <summary>
        /// Points the controller at a different Animator at runtime (a visual swap), or null to drive nothing. The
        /// parameter hashes are name-based and carry over; the new Animator must share the same controller layout.
        /// </summary>
        public void SetAnimator(Animator target) => animator = target;

        private void TickActionLayer(float deltaTime) {
            if (animator == null || Mathf.Approximately(_actionWeight, _actionTarget))
                return;

            _actionWeight = Mathf.MoveTowards(_actionWeight, _actionTarget, _actionRate * deltaTime);
            animator.SetLayerWeight(ActionLayer, _actionWeight);
        }

        private void ConfigureStateMachines() {
            LocoStateMachine = new StateMachine<PlayerControllerExample, LocoStateTypes>(this);
            LocoStateMachine.Configure(locoConfig);

            ActionStateMachine = new StateMachine<PlayerControllerExample, ActionStateTypes>(this);
            ActionStateMachine.Configure(actionConfig);
        }

        /// <summary>
        /// Optional.
        /// This method comes from the IDebuggingInfo interface that we implemented. It allows the user to add or remove
        /// the debugging component at runtime should they choose to. It will simply allow you to print things to canvas
        /// in an easy and convenient way.
        /// </summary>
        public void RegisterDebugInfo(ControllerDebugging debugHud) {
            // Show which ScriptableObject state is currently running
            debugHud.Field("Current Loco State", () => {
                var currentStateVariant = LocoStateMachine.GetCurrentRunningState();

                return currentStateVariant != null
                        ? currentStateVariant.name
                        : "None";
            });

            // Show each driver (enum value) and what variant it's pointing to
            foreach (LocoStateTypes stateType in Enum.GetValues(typeof(LocoStateTypes))) {
                debugHud.Field($"{stateType}", () => {
                    var currentVariant = LocoStateMachine.GetCurrentVariant(stateType);

                    return currentVariant != null
                            ? currentVariant.name
                            : "Not Assigned";
                });
            }

            // Repeat for action state machine.
            debugHud.Field("Current Action State", () => {
                var currentStateVariant = ActionStateMachine.GetCurrentRunningState();

                return currentStateVariant != null
                        ? currentStateVariant.name
                        : "None";
            });

            foreach (ActionStateTypes stateType in Enum.GetValues(typeof(ActionStateTypes))) {
                debugHud.Field($"{stateType}", () => {
                    var currentVariant = ActionStateMachine.GetCurrentVariant(stateType);

                    return currentVariant != null
                            ? currentVariant.name
                            : "Not Assigned";
                });
            }

            debugHud.Gizmo(() => {
                var capsule = ResizableCapsuleCollider;

                if (capsule.collider == null)
                    return;

                var floatData = capsule.CapsuleFloatData;
                var up = PlanarUp.sqrMagnitude > 0.5f
                        ? PlanarUp
                        : transform.up;
                var origin = capsule.collider.bounds.center;
                var radius = floatData.ProbeRadius;

                var maxReach = floatData.RideHeight *
                               ControllerHelper.GetSlopeReachFactor(StatData.maxSlopeAngle, StatData.maxSlopeAngle) +
                               floatData.GroundedTolerance + radius;
                var probe = capsule.ProbeGround(-up, maxReach, LayerData.GroundLayer);

                var angle = probe.HasHit
                        ? ControllerHelper.GetSlopeAngle(probe.Normal, up)
                        : 0f;
                var reach = floatData.RideHeight * ControllerHelper.GetSlopeReachFactor(angle, StatData.maxSlopeAngle) +
                            floatData.GroundedTolerance;

                Gizmos.color = probe.HasHit && probe.Distance <= reach
                        ? Color.green
                        : Color.red;
                Gizmos.DrawWireSphere(origin, radius);
                Gizmos.DrawLine(origin, origin - up * reach);
                Gizmos.DrawWireSphere(origin - up * (reach - radius), radius);

                if (!probe.HasHit)
                    return;

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(origin - up * (probe.Distance - radius), radius);
                Gizmos.DrawLine(probe.Point, probe.Point + probe.Normal * 0.5f);
            });
        }
    }
}
