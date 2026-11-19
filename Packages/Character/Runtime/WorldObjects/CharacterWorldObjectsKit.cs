// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using global::Spellbound.WorldObjects;
using Spellbound.Core.Simulation;

namespace Spellbound.Character.WorldObjects {
    public static class CharacterWorldObjectsKit {
        public static InstanceRide Ride(WorldInstance world, InstanceId instance) {
            if (world == null) {
                throw new ArgumentNullException(nameof(world));
            }

            return new InstanceRide(world, instance);
        }
    }
}
