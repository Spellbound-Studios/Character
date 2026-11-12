// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Default action state: idle until the player aims. Polls the aim input and hands off to the aiming state —
    /// the same poll-and-ChangeState pattern the loco states use, just on the action state machine.
    /// </summary>
    [CreateAssetMenu(fileName = "ReadyStateExample", menuName = "Spellbound/StateMachine/ReadyStateExample")]
    public class ReadyStateExample : BaseActionStateExample {
        protected override void EnterStateLogic() =>
                Ctx.ExampleInput.OnCinematicTogglePressed += HandleCinematicTogglePressed;

        protected override void UpdateStateLogic() {
            if (Ctx.ExampleInput.IsAiming)
                Ctx.ActionStateMachine.ChangeState(ActionStateTypes.Aiming);
        }

        protected override void FixedUpdateStateLogic() { }

        protected override void ExitStateLogic() =>
                Ctx.ExampleInput.OnCinematicTogglePressed -= HandleCinematicTogglePressed;

        private void HandleCinematicTogglePressed() => Ctx.ActionStateMachine.ChangeState(ActionStateTypes.Cinematic);
    }
}
