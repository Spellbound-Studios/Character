// Copyright 2026 Spellbound Studio Inc.

using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller {
    public class AimCore : MonoBehaviour, IInputAxisOwner {
        [Tooltip("Horizontal look rotation in degrees. 0 = world forward.")]
        public InputAxis HorizontalLook = new() {
            Range = new Vector2(-180f, 180f), Wrap = true, Recentering = InputAxis.RecenteringSettings.Default
        };

        [Tooltip("Vertical look rotation in degrees. Positive looks down; negative looks up.")]
        public InputAxis VerticalLook = new() {
            Range = new Vector2(-89f, 89f), Recentering = InputAxis.RecenteringSettings.Default
        };

        void IInputAxisOwner.GetInputAxes(List<IInputAxisOwner.AxisDescriptor> axes) {
            axes.Add(new IInputAxisOwner.AxisDescriptor {
                DrivenAxis = () => ref HorizontalLook, Name = "Horizontal Look",
                Hint = IInputAxisOwner.AxisDescriptor.Hints.X
            });
            axes.Add(new IInputAxisOwner.AxisDescriptor {
                DrivenAxis = () => ref VerticalLook, Name = "Vertical Look",
                Hint = IInputAxisOwner.AxisDescriptor.Hints.Y
            });
        }

        private void OnValidate() {
            HorizontalLook.Validate();
            VerticalLook.Range.x = Mathf.Clamp(VerticalLook.Range.x, -89f, 89f);
            VerticalLook.Range.y = Mathf.Clamp(VerticalLook.Range.y, -89f, 89f);
            VerticalLook.Validate();
        }

        private void OnEnable() =>
                SetLookDirection(transform.rotation * Vector3.forward);

        private void Update() {
            transform.rotation = Quaternion.Euler(VerticalLook.Value, HorizontalLook.Value, 0f);
            HorizontalLook.UpdateRecentering(Time.deltaTime, HorizontalLook.TrackValueChange());
            VerticalLook.UpdateRecentering(Time.deltaTime, VerticalLook.TrackValueChange());
        }

        public void SetLookDirection(Vector3 worldDirection) {
            if (worldDirection.sqrMagnitude < 1e-6f)
                return;

            var angles = Quaternion.LookRotation(worldDirection, Vector3.up).eulerAngles;
            HorizontalLook.Value = HorizontalLook.ClampValue(NormalizeAngle(angles.y));
            VerticalLook.Value = VerticalLook.ClampValue(NormalizeAngle(angles.x));
        }

        private static float NormalizeAngle(float angle) {
            while (angle > 180f)
                angle -= 360f;

            while (angle < -180f)
                angle += 360f;

            return angle;
        }
    }
}
