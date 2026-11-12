// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Locomotion state entered when the capsule stands on ground steeper than the walkable
    /// <see cref="StatData.maxSlopeAngle"/>. Gravity drags the capsule down the fall line, input carves it
    /// laterally, and planar drag plus a terminal cap bound the speed. Recovers to the ground state once the
    /// surface flattens back inside the walkable angle (minus a hysteresis buffer) and falls when airborne.
    /// </summary>
    [CreateAssetMenu(fileName = "SlidingStateExample", menuName = "Spellbound/StateMachine/SlidingStateExample")]
    public class SlidingStateExample : BaseLocomotionStateExample {
        [field: SerializeField, Range(0f, 15f),
         Tooltip("Hysteresis: how many degrees below Max Slope Angle the ground must flatten before sliding ends " +
                 "and you stand up. Higher = keeps sliding longer (resists flip-flopping near the cutoff); lower = stands up sooner.")]
        public float SlideExitAngleBuffer { get; set; } = 2f;

        [Header("Animation")]
        [SerializeField, Tooltip("Animator state this state plays while sliding.")]
        private string slideStateName = "Sliding";
        private int _slideHash;

        protected override void OnStateInitialize() => _slideHash = Animator.StringToHash(slideStateName);

        protected override void EnterStateLogic() =>
                Ctx.Animation?.CrossFade(_slideHash, PlayerControllerExample.BaseLayer);

        protected override void UpdateStateLogic() { }

        protected override void FixedUpdateStateLogic() {
            if (!PerformGroundCheck()) {
                Ctx.LocoStateMachine.ChangeState(LocoStateTypes.Falling);

                return;
            }

            if (HasRecoveredToWalkable()) {
                Ctx.LocoStateMachine.ChangeState(LocoStateTypes.Grounded);

                return;
            }

            KeepCapsuleFloating();
            HandleSlide();
            HandleCharacterRotation();
        }

        protected override void ExitStateLogic() { }

        /// <summary>
        /// Accelerates the capsule down the fall line, applies lateral steering from input, then bounds the
        /// planar speed with drag and a hard terminal cap.
        /// </summary>
        protected virtual void HandleSlide() {
            var up = Ctx.PlanarUp;
            var normal = Ground.Normal;
            var slopeDir = ControllerHelper.GetSlopeDirection(normal, up);

            var slideAccel = ControllerHelper.GetSlopeAccelerationMagnitude(
                    Mathf.Abs(Ctx.StatData.gravity), CurrentSlopeAngle()) * Ctx.StatData.slideAccelMultiplier;

            Ctx.Rb.AddForce(slopeDir * slideAccel, ForceMode.Acceleration);

            var lateralAxis = Vector3.Cross(normal, slopeDir);
            var steer = Vector3.Dot(GetInputDirectionRelativeToCamera(), lateralAxis);

            Ctx.Rb.AddForce(lateralAxis * (steer * Ctx.StatData.lateralSteerAccel), ForceMode.Acceleration);

            var planarVelocity = ControllerHelper.GetPlanarVelocity(Ctx.Rb, up);

            Ctx.Rb.AddForce(-planarVelocity * Ctx.StatData.planarDrag, ForceMode.Acceleration);

            var terminal = Ctx.StatData.TerminalSlidingSpeed;

            if (planarVelocity.sqrMagnitude > terminal * terminal)
                Ctx.Rb.linearVelocity -= planarVelocity - planarVelocity.normalized * terminal;
        }

        /// <summary>
        /// Reach uses the true slope angle rather than the walkable cap so the capsule stays glued to steep
        /// descents instead of stepping off them.
        /// </summary>
        protected override float SlopeReachFactor() {
            var angle = CurrentSlopeAngle();

            return ControllerHelper.GetSlopeReachFactor(angle, angle);
        }

        private bool HasRecoveredToWalkable() =>
                CurrentSlopeAngle() <= Ctx.StatData.maxSlopeAngle - SlideExitAngleBuffer;
    }
}
