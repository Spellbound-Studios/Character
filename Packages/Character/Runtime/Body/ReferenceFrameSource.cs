// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Logging;
using Spellbound.Core.ReferenceFrames;
using UnityEngine;

namespace Spellbound.Character.Body {
    [DefaultExecutionOrder(-100)]
    public abstract class ReferenceFrameSource : MonoBehaviour, IReferenceFrame {
        [SerializeField] private ulong referenceFrameId;

        [SerializeField] private MonoBehaviour world;

        private IReferenceFrameWorld _world;
        private IReferenceFrameWorld _registeredWorld;

        public IReferenceFrameWorld World {
            get => _world;
            set {
                RefuseWhileRegistered();
                _world = value;
            }
        }

        public ReferenceFrameId Id {
            get => new(referenceFrameId);
            set {
                RefuseWhileRegistered();
                referenceFrameId = value.Value;
            }
        }

        public bool IsRegistered { get; private set; }

        public abstract Pose WorldPose { get; }

        public abstract Vector3 AngularVelocity { get; }

        public abstract Vector3 VelocityAt(Vector3 worldPoint);

        protected virtual void OnEnable() {
            var resolved = ResolveWorld();

            if (resolved == null) {
                Log.Error($"[ReferenceFrameSource] '{name}' found no reference frame world and stays unregistered.",
                    this);

                return;
            }

            if (referenceFrameId == 0ul)
                referenceFrameId = ReferenceFrameIds.FromHierarchyPath(transform).Value;

            if (!resolved.ReferenceFrames.TryRegister(this)) {
                Log.Error($"[ReferenceFrameSource] '{name}' was refused id {Id} and stays unregistered.",
                    this);

                return;
            }

            _registeredWorld = resolved;
            IsRegistered = true;
        }

        protected virtual void OnDisable() {
            if (!IsRegistered)
                return;

            _registeredWorld.ReferenceFrames.Unregister(Id);
            _registeredWorld = null;
            IsRegistered = false;
        }

        private IReferenceFrameWorld ResolveWorld() {
            if (_world != null)
                return _world;

            if (world != null && world is IReferenceFrameWorld serialized)
                return serialized;

            return GetComponentInParent<IReferenceFrameWorld>();
        }

        private void RefuseWhileRegistered() {
            if (IsRegistered)
                throw new InvalidOperationException(
                    $"'{name}' is registered as reference frame {Id}; disable it before changing its world or id.");
        }
    }
}
