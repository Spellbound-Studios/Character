// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Character.Body;
using Spellbound.Core.Ground;
using Spellbound.GeoForge;
using UnityEngine;

namespace Spellbound.Character.GeoForge {
    public sealed class GeoForgeContainmentSpace : IContainmentSpace {
        private const int CornerCount = 8;

        private readonly IGeoVolume _volume;

        public GeoForgeContainmentSpace(IGeoVolume volume) {
            _volume = volume ?? throw new ArgumentNullException(nameof(volume));
        }

        public bool IsReady(in Bounds worldBounds) => _volume.IsBoxReady(worldBounds, ReadinessScope.AnyLevel, out _);

        public Occupancy Classify(in Bounds worldBounds) {
            var minimum = worldBounds.min;
            var maximum = worldBounds.max;
            var solidCorners = 0;

            for (var corner = 0; corner < CornerCount; corner++) {
                var point = new Vector3(
                    (corner & 1) == 0 ? minimum.x : maximum.x,
                    (corner & 2) == 0 ? minimum.y : maximum.y,
                    (corner & 4) == 0 ? minimum.z : maximum.z);

                if (!_volume.TryQuerySolid(point, out var solid))
                    return Occupancy.Mixed;

                if (solid)
                    solidCorners++;
            }

            if (solidCorners == CornerCount)
                return Occupancy.Solid;

            return solidCorners == 0 ? Occupancy.Empty : Occupancy.Mixed;
        }
    }
}
