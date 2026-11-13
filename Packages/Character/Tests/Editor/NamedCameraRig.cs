// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public sealed class NamedCameraRig : ICameraRig {
        private readonly Dictionary<string, Transform> _cameras = new();
        private readonly List<string> _names = new();

        public void Add(string cameraName, Transform camera) {
            _cameras.Add(cameraName, camera);
            _names.Add(cameraName);
        }

        public Transform FollowTarget { get; private set; }

        public string Current { get; private set; }

        public IReadOnlyList<string> CameraNames => _names;

        public Transform CurrentCameraTransform => Current != null ? _cameras[Current] : null;

        public event Action<string, string> CurrentChanged;

        public void SetFollowTarget(Transform target) => FollowTarget = target;

        public bool Switch(string cameraName) {
            if (cameraName == null || !_cameras.ContainsKey(cameraName))
                return false;

            if (cameraName == Current)
                return true;

            var previous = Current;
            Current = cameraName;
            CurrentChanged?.Invoke(previous, cameraName);

            return true;
        }
    }
}
