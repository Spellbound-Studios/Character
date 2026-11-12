// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller {
    [RequireComponent(typeof(CinemachineThirdPersonFollow))]
    public class ThirdPersonFollowZoom : MonoBehaviour, IInputAxisOwner {
        [Tooltip("Camera distance in meters. The axis Range is the closest and furthest the camera may zoom.")]
        public InputAxis Distance = new() { Value = 4f, Center = 4f, Range = new Vector2(1f, 8f) };

        private CinemachineThirdPersonFollow _follow;

        void IInputAxisOwner.GetInputAxes(List<IInputAxisOwner.AxisDescriptor> axes) =>
                axes.Add(new IInputAxisOwner.AxisDescriptor { DrivenAxis = () => ref Distance, Name = "Zoom" });

        private void OnValidate() => Distance.Validate();

        private void Awake() => _follow = GetComponent<CinemachineThirdPersonFollow>();

        private void Update() => _follow.CameraDistance = Distance.Value;
    }
}
