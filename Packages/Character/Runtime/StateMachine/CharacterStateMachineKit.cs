// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.StateMachine;

namespace Spellbound.Character.StateMachine {
    public static class CharacterStateMachineKit {
        public static global::Spellbound.StateMachine.StateMachine<BodyContext, TStateKey> Create<TStateKey>(
            BodyContext context, IStateMachineConfiguration<TStateKey> configuration) where TStateKey : Enum =>
                StateMachineKit.Create(context, configuration);
    }
}
