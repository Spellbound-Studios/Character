// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Samples.SwimAndFly.Tests {
    public sealed class ScriptedMoveSource : IMoveSource {
        public Vector3 DesiredMove { get; set; }

        public Vector3 DesiredUp { get; set; }
    }
}
