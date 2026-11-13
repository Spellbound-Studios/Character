// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion.Tests {
    public sealed class ScriptedLocomotionInput : ILocomotionInput {
        public Vector2 Move { get; set; }

        public bool Run { get; set; }

        public bool JumpPressed { get; set; }
    }
}
