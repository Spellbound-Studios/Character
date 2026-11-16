// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.StateMachine.Editor.Tests {
    internal sealed class ContextMoveSource : IMoveSource {
        public Vector3 DesiredMove { get; set; }

        public Vector3 DesiredUp { get; set; }
    }
}
