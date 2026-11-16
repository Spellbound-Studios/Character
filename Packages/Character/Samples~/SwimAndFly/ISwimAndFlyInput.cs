// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.SwimAndFly {
    public interface ISwimAndFlyInput {
        Vector2 Move { get; }

        bool FlyHeld { get; }
    }
}
