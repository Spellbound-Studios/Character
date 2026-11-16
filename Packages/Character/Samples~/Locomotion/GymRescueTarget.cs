// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    public sealed class GymRescueTarget : IRescueTarget {
        private readonly Transform _spawnMarker;

        public GymRescueTarget(Transform spawnMarker) {
            _spawnMarker = spawnMarker != null ? spawnMarker : throw new ArgumentNullException(nameof(spawnMarker));
        }

        public Pose Fallback(Vector3 from) => new(_spawnMarker.position, _spawnMarker.rotation);
    }
}
