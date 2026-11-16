// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.GeoForge;
using UnityEngine;

namespace Spellbound.Character.GeoForge.Tests {
    public sealed class TestGeoVolume : IGeoVolume {
        public float SurfaceHeight { get; set; }

        public bool HasUnloadedRegion { get; set; }

        public Bounds UnloadedRegion { get; set; }

        public bool Ready { get; set; } = true;

        public int SolidQueryCount { get; private set; }

        public int ReadinessQueryCount { get; private set; }

        public Bounds LastReadinessBounds { get; private set; }

        public ReadinessScope LastReadinessScope { get; private set; }

        public GeoVolumeEngine GeoVolumeEngine => null;

        public Vector2[] ViewDistanceLodRanges => Array.Empty<Vector2>();

        public bool IsMoving { get; set; }

        public bool IsPrimaryTerrain { get; set; }

        public bool TryQuerySolid(Vector3 worldPosition, out bool solid) {
            SolidQueryCount++;

            if (HasUnloadedRegion && UnloadedRegion.Contains(worldPosition)) {
                solid = false;

                return false;
            }

            solid = worldPosition.y < SurfaceHeight;

            return true;
        }

        public bool IsBoxReady(in Bounds worldBounds, ReadinessScope scope, out VolumeVersion version) {
            ReadinessQueryCount++;
            LastReadinessBounds = worldBounds;
            LastReadinessScope = scope;
            version = default;

            return Ready;
        }

        public void ResetEditedChunksToProcedural() { }
    }
}
