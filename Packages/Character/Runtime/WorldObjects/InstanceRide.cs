// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using global::Spellbound.WorldObjects;
using Spellbound.Character.Body;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Core.Simulation;
using UnityEngine;

namespace Spellbound.Character.WorldObjects {
    public sealed class InstanceRide {
        private readonly WorldInstance _world;
        private readonly InstanceId _instance;

        internal InstanceRide(WorldInstance world, InstanceId instance) {
            _world = world;
            _instance = instance;
        }

        public ReferenceFrameBinding Binding { get; } = new();

        public bool IsBound => Binding.IsBound;

        public void Bind(IReferenceFrame frame, double time) {
            var worldPose = CurrentWorldPose();
            Binding.Bind(frame, worldPose, time);
            _world.Writer().Move(_instance, Binding.LocalPose);
        }

        public void Release() {
            var worldPose = CurrentWorldPose();
            Binding.Release();
            _world.Writer().Move(_instance, ReferenceFramePose.InWorld(worldPose));
        }

        private Pose CurrentWorldPose() {
            if (!_world.TryGetView(_instance, out var view)) {
                throw new ArgumentException("The world '" + _world.Name + "' holds no instance " + _instance + ".");
            }

            return _world.Reader.ToWorld(view.Pose);
        }
    }
}
