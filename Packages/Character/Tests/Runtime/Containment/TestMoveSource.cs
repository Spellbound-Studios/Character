// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Tests {
    public sealed class TestMoveSource : IMoveSource {
        public Vector3 DesiredMove { get; set; }

        public Vector3 DesiredUp { get; set; }
    }
}
