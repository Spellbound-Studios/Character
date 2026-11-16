// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Tests {
    public sealed class TestRescueTarget : IRescueTarget {
        public Pose Pose { get; set; } = Pose.identity;

        public Pose Fallback(Vector3 from) => Pose;
    }
}
