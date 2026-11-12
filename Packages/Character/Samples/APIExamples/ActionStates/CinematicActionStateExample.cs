// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    [CreateAssetMenu(fileName = "CinematicActionStateExample",
        menuName = "Spellbound/StateMachine/CinematicActionStateExample")]
    public class CinematicActionStateExample : BaseActionStateExample {
        [SerializeField] private string cinematicCameraName = "CinematicCamera";

        private string _restore;

        protected override void EnterStateLogic() {
            Ctx.ExampleInput.OnCinematicTogglePressed += HandleCinematicTogglePressed;
            Ctx.ExampleInput.DisablePlayerInput();
            _restore = Ctx.SwitchCamera(cinematicCameraName);
        }

        protected override void UpdateStateLogic() { }

        protected override void FixedUpdateStateLogic() { }

        protected override void ExitStateLogic() {
            Ctx.ExampleInput.OnCinematicTogglePressed -= HandleCinematicTogglePressed;

            if (!string.IsNullOrEmpty(_restore))
                Ctx.SwitchCamera(_restore);

            Ctx.ExampleInput.EnablePlayerInput();
        }

        private void HandleCinematicTogglePressed() => Ctx.ActionStateMachine.ChangeState(ActionStateTypes.Ready);
    }
}
