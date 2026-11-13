// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion.Tests {
    public sealed class HeldForwardInput : MonoBehaviour, ILocomotionInput {
        public Vector2 Move => Vector2.up;

        public bool Run => false;

        public bool JumpPressed => false;
    }
}
