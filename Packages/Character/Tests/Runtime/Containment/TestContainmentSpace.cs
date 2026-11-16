// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using Spellbound.Character.Body;
using Spellbound.Core.Ground;
using UnityEngine;

namespace Spellbound.Character.Tests {
    public sealed class TestContainmentSpace : IContainmentSpace {
        public bool Ready { get; set; } = true;

        public List<Bounds> Solids { get; } = new();

        public bool IsReady(in Bounds worldBounds) => Ready;

        public Occupancy Classify(in Bounds worldBounds) {
            var touchesSolid = false;
            foreach (var solid in Solids) {
                if (solid.Contains(worldBounds.min) && solid.Contains(worldBounds.max))
                    return Occupancy.Solid;

                if (solid.Intersects(worldBounds))
                    touchesSolid = true;
            }

            return touchesSolid ? Occupancy.Mixed : Occupancy.Empty;
        }
    }
}
