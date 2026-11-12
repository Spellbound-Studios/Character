// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// The central store for camera operations: which camera is live, switching/blending between them, and the
    /// target the cameras follow. <see cref="CameraRigManager"/> is the Cinemachine-backed implementation.
    /// </summary>
    public interface ICameraRig {
        /// <summary>
        /// The name of the camera currently shown (or being blended to); null if none.
        /// </summary>
        public string Current { get; }

        public IReadOnlyList<string> CameraNames { get; }

        /// <summary>
        /// The transform of the live camera.
        /// </summary>
        public Transform CurrentCameraTransform { get; }

        public event Action<string, string> CurrentChanged;

        /// <summary>
        /// Points every camera at <paramref name="target"/>.
        /// </summary>
        public void SetFollowTarget(Transform target);

        /// <summary>
        /// Makes the camera named <paramref name="cameraName"/> the live one; the backend blends to it.
        /// Returns false when no camera has that name.
        /// </summary>
        public bool Switch(string cameraName);
    }
}
