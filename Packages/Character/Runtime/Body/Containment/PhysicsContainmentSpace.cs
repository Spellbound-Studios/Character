// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.Ground;
using UnityEngine;

namespace Spellbound.Character.Body {
    public sealed class PhysicsContainmentSpace : IContainmentSpace {
        public bool IsReady(in Bounds worldBounds) => true;

        public Occupancy Classify(in Bounds worldBounds) => Occupancy.Empty;
    }
}
