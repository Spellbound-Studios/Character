// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Character.Editor {
    public static class ContainmentGizmos {
        private const float TargetRadiusMetres = 0.2f;

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawRescues(Containment containment, GizmoType gizmoType) {
            var rescues = containment.LastRescues;

            for (var index = 0; index < rescues.Count; index++) {
                var (from, step) = rescues[index];
                var target = step.Target.position;
                var colour = RungColour(step.Rung);

                Gizmos.color = colour;
                Gizmos.DrawLine(from, target);
                Gizmos.DrawWireSphere(target, TargetRadiusMetres);
                Handles.color = colour;
                Handles.Label(target, step.Reason.ToString());
            }
        }

        private static Color RungColour(RescueRung rung) {
            return rung switch {
                RescueRung.LastSupported => Color.green,
                RescueRung.OpenAbove => Color.yellow,
                _ => Color.red
            };
        }
    }
}
