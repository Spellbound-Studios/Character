// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.SwimAndFly.Tests {
    public sealed class ScriptedSwimAndFlyInput : ISwimAndFlyInput {
        public Vector2 Move { get; set; }

        public bool FlyHeld { get; set; }
    }
}
