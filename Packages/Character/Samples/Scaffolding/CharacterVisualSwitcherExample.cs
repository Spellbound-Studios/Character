// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Demo glue that swaps which visual model is shown at runtime and retargets the animation graph onto its
    /// Animator — proving one humanoid <see cref="AnimationCollection"/> drives every humanoid character, with no
    /// per-character clips or collections. The capsule entry has no Animator (the floating-capsule view): selecting
    /// it retargets to nothing and shows the bare physics shape while locomotion still drives the body.
    /// </summary>
    public sealed class CharacterVisualSwitcherExample : MonoBehaviour {
        [Serializable]
        public struct Visual {
            public string label;
            public GameObject root;
            public Animator animator;
        }

        [SerializeField] private PlayerControllerExample controller;
        [SerializeField] private Visual[] visuals;

        [SerializeField, Tooltip("Label shown on Start; leave empty to show nothing until a button is pressed.")]
        private string defaultLabel;

        /// <summary>
        /// The label of the visual currently shown, or null if none — used by the demo panel to highlight.
        /// </summary>
        public string Current { get; private set; }

        /// <summary>
        /// The configured visuals, exposed so a demo panel can build one button per entry.
        /// </summary>
        public IReadOnlyList<Visual> Visuals => visuals;

        private void Awake() {
            if (controller == null)
                controller = GetComponent<PlayerControllerExample>();
        }

        private void Start() {
            if (!string.IsNullOrEmpty(defaultLabel))
                Show(defaultLabel);
        }

        /// <summary>
        /// Shows the visual whose label matches, hides the rest, and retargets the animation graph onto its Animator
        /// (or nothing, for the capsule). Wire this to a UI button or call it from the demo panel.
        /// </summary>
        public void Show(string label) {
            Current = label;

            for (var i = 0; i < visuals.Length; i++) {
                var visual = visuals[i];
                var match = visual.label == label;

                if (visual.root != null)
                    visual.root.SetActive(match);

                if (match)
                    controller.SetAnimator(visual.animator);
            }
        }
    }
}
