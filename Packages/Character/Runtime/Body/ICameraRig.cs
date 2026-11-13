// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Character.Body {
    public interface ICameraRig {
        public string Current { get; }

        public IReadOnlyList<string> CameraNames { get; }

        public Transform CurrentCameraTransform { get; }

        public event Action<string, string> CurrentChanged;

        public void SetFollowTarget(Transform target);

        public bool Switch(string cameraName);
    }
}
