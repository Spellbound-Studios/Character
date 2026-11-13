// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.StateMachine;

namespace Spellbound.Character.StateMachine {
    public abstract class BodyState : State {
        public BodyContext Body => (BodyContext)Context;
    }
}
