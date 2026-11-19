// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using global::Spellbound.WorldObjects;
using Spellbound.Character.Body;
using Spellbound.Core.Observations;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Core.Simulation;
using UnityEngine;

namespace Spellbound.Character.WorldObjects {
    [DefaultExecutionOrder(-1000)]
    public sealed class WorldObjectsReferenceFrameWorld : MonoBehaviour, IReferenceFrameWorld {
        private WorldInstance _world;
        private bool _read;

        public WorldInstance World {
            get => _world;
            set {
                if (_read) {
                    throw new InvalidOperationException("The reference frame world '" + name +
                        "' was already read by a source, so its World cannot change.");
                }

                _world = value;
            }
        }

        public ReferenceFrameDirectory ReferenceFrames => Read(nameof(ReferenceFrames)).ReferenceFrames;

        public ObservationContext Observations => Read(nameof(Observations)).Observations;

        public Tick CurrentTick => Read(nameof(CurrentTick)).Clock.Current;

        private WorldInstance Read(string member) {
            if (_world == null) {
                throw new InvalidOperationException("The reference frame world '" + name + "' has no World, so " +
                    member + " cannot be read.");
            }

            _read = true;
            return _world;
        }
    }
}
