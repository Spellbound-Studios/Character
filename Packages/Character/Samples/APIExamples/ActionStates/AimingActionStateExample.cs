// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Action state entered while aiming (bow/gun): on enter it switches the rig to the aiming camera (by name) —
    /// Cinemachine blends the current view into the tighter over-the-shoulder one — and restores the prior camera
    /// on exit. Both cameras attach to the player's <see cref="AimCore"/>, so the blend can never change where
    /// the player is looking.
    /// </summary>
    [CreateAssetMenu(fileName = "AimingActionStateExample",
        menuName = "Spellbound/StateMachine/AimingActionStateExample")]
    public class AimingActionStateExample : BaseActionStateExample {
        [SerializeField, Tooltip("Name of the camera switched to while aiming (the rig camera's GameObject name).")]
        private string aimCameraName = "AimingCamera";

        [Header("Animation")]
        [SerializeField, Tooltip("Animator state on the masked action layer played while aiming.")]
        private string aimStateName = "Aiming";
        private int _aimHash;

        private string _restore;

        protected override void OnStateInitialize() => _aimHash = Animator.StringToHash(aimStateName);

        protected override void EnterStateLogic() {
            Ctx.Animation?.CrossFade(_aimHash, PlayerControllerExample.ActionLayer);
            Ctx.FadeActionLayer(1f);

            _restore = Ctx.SwitchCamera(aimCameraName);
        }

        protected override void UpdateStateLogic() {
            if (!Ctx.ExampleInput.IsAiming)
                Ctx.ActionStateMachine.ChangeState(ActionStateTypes.Ready);
        }

        protected override void FixedUpdateStateLogic() { }

        protected override void ExitStateLogic() {
            Ctx.FadeActionLayer(0f);

            if (!string.IsNullOrEmpty(_restore))
                Ctx.SwitchCamera(_restore);
        }
    }
}
