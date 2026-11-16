// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Core.Ground;
using UnityEngine;

namespace Spellbound.Character.Body {
    public interface IContainmentSpace {
        bool IsReady(in Bounds worldBounds);

        Occupancy Classify(in Bounds worldBounds);
    }
}
