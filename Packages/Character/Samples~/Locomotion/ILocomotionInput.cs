// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    public interface ILocomotionInput {
        Vector2 Move { get; }

        bool Run { get; }

        bool JumpPressed { get; }
    }
}
