// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Rig {
    public sealed class AnimatorAnimationController : IAnimationController {
        public AnimatorAnimationController(Animator animator) {
            if (animator == null)
                throw new ArgumentNullException(nameof(animator));

            Animator = animator;
        }

        public Animator Animator { get; }

        public void CrossFade(int stateHash, int layer = 0, float fadeSeconds = 0f) =>
                Animator.CrossFadeInFixedTime(stateHash, fadeSeconds, layer);

        public void SetFloat(int parameterHash, float value) => Animator.SetFloat(parameterHash, value);

        public void SetLayerWeight(int layer, float weight) => Animator.SetLayerWeight(layer, weight);
    }
}
