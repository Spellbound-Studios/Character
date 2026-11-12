// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.Tooling;
using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller {
    public class CameraRigManager : CinemachineCameraManagerBase, ICameraRig {
        [SerializeField,
         Tooltip("Camera made live on start (by name). Falls back to the first child camera when empty/unmatched.")]
        private string defaultCamera;

        private readonly List<string> _names = new();

        private CinemachineVirtualCameraBase _currentCamera;
        private string _currentName;

        public string Current => _currentName;

        public CinemachineVirtualCameraBase CurrentCamera => _currentCamera;

        public Transform CurrentCameraTransform => _currentCamera != null
                ? _currentCamera.transform
                : null;

        public IReadOnlyList<string> CameraNames {
            get {
                _names.Clear();

                var children = ChildCameras;

                for (var i = 0; i < children.Count; i++) {
                    if (children[i] != null)
                        _names.Add(children[i].Name);
                }

                return _names;
            }
        }

        public event Action<string, string> CurrentChanged;

        private void Awake() {
            if (SingletonManager.TryGetSingletonInstance<ICameraRig>(out var existing) && !ReferenceEquals(existing, this)) {
                Destroy(gameObject);

                return;
            }

            SingletonManager.RegisterSingleton<ICameraRig>(this);
        }

        protected override void OnDestroy() {
            if (SingletonManager.TryGetSingletonInstance<ICameraRig>(out var registered) && ReferenceEquals(registered, this))
                SingletonManager.UnregisterSingleton<ICameraRig>();

            base.OnDestroy();
        }

        protected override void Start() {
            base.Start();

            var initial = ResolveDefaultName();

            if (initial != null)
                Switch(initial);
            else
                Log.Error("[CameraRigManager] No child cameras found.");
        }

        public void SetFollowTarget(Transform target) {
            DefaultTarget.Enabled = true;
            DefaultTarget.Target.TrackingTarget = target;
        }

        public bool Switch(string cameraName) {
            var camera = FindByName(cameraName);

            if (camera == null) {
                Log.Warn($"[CameraRigManager] No camera named '{cameraName}'.");

                return false;
            }

            if (camera == _currentCamera)
                return true;

            var previous = _currentName;
            _currentCamera = camera;
            _currentName = cameraName;
            CurrentChanged?.Invoke(previous, cameraName);

            return true;
        }

        public bool Register(CinemachineVirtualCameraBase camera) {
            if (camera == null) {
                Log.Error("[CameraRigManager] Cannot register a null camera.");

                return false;
            }

            if (FindByName(camera.Name) != null) {
                Log.Error($"[CameraRigManager] Duplicate camera name '{camera.Name}'.");

                return false;
            }

            if (camera.transform.parent != transform)
                camera.transform.SetParent(transform, true);

            InvalidateCameraCache();

            return true;
        }

        public bool Unregister(string cameraName) {
            var camera = FindByName(cameraName);

            if (camera == null)
                return false;

            camera.transform.SetParent(null, true);
            InvalidateCameraCache();

            if (_currentCamera != camera)
                return true;

            _currentCamera = null;
            _currentName = null;

            var fallback = ResolveDefaultName();

            if (fallback != null)
                Switch(fallback);
            else
                Log.Warn("[CameraRigManager] Unregistered the last camera; nothing is live.");

            return true;
        }

        protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime) {
            if (_currentCamera == null) {
                var fallback = ResolveDefaultName();

                if (fallback != null)
                    Switch(fallback);
            }

            return _currentCamera;
        }

        private CinemachineVirtualCameraBase FindByName(string cameraName) {
            if (string.IsNullOrEmpty(cameraName))
                return null;

            var children = ChildCameras;

            for (var i = 0; i < children.Count; i++) {
                if (children[i] != null && children[i].Name == cameraName)
                    return children[i];
            }

            return null;
        }

        private string ResolveDefaultName() {
            if (FindByName(defaultCamera) != null)
                return defaultCamera;

            var children = ChildCameras;

            for (var i = 0; i < children.Count; i++) {
                if (children[i] != null)
                    return children[i].Name;
            }

            return null;
        }
    }
}
