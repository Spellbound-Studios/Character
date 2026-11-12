// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Tooling;
using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [RequireComponent(typeof(CinemachineOrbitalFollow))]
    public sealed class CinematicOrbitExample : MonoBehaviour {
        [SerializeField, Range(0f, 90f)] private float degreesPerSecond = 12f;
        [SerializeField, Range(0f, 2f)] private float zoomAmplitude = 0.6f;
        [SerializeField, Range(1f, 60f)] private float zoomPeriod = 14f;

        private CinemachineOrbitalFollow _orbit;
        private ICameraRig _rig;
        private string _cameraName;
        private bool _live;
        private float _elapsed;

        private void Awake() {
            _orbit = GetComponent<CinemachineOrbitalFollow>();
            _cameraName = gameObject.name;
        }

        private void OnDestroy() {
            if (_rig != null)
                _rig.CurrentChanged -= OnCurrentChanged;
        }

        private void Update() {
            if (_rig == null) {
                if (!SingletonManager.TryGetSingletonInstance(out _rig))
                    return;

                _rig.CurrentChanged += OnCurrentChanged;
                _live = _rig.Current == _cameraName;
            }

            if (!_live)
                return;

            _elapsed += Time.deltaTime;
            _orbit.HorizontalAxis.Value += degreesPerSecond * Time.deltaTime;

            if (_orbit.HorizontalAxis.Value > 180f)
                _orbit.HorizontalAxis.Value -= 360f;

            _orbit.RadialAxis.Value =
                    1f + zoomAmplitude * (0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * _elapsed / zoomPeriod));
        }

        private void OnCurrentChanged(string previous, string current) {
            _live = current == _cameraName;

            if (_live)
                _elapsed = 0f;
        }
    }
}
