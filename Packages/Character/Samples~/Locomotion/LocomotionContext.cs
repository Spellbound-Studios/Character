// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Character.Body;
using Spellbound.Character.Rig;
using Spellbound.Character.StateMachine;
using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    public sealed class LocomotionContext : BodyContext {
        private readonly Action<LocomotionStateKey> _changeState;

        public LocomotionContext(
            Transform transform, Rigidbody rigidbody, ResizableCapsuleCollider capsule,
            ReferenceFrameBinding referenceFrame, IAnimationController animation,
            Action<LocomotionStateKey> changeState) : base(transform, rigidbody, capsule, referenceFrame) {
            Animation = animation;
            _changeState = changeState ?? throw new ArgumentNullException(nameof(changeState));
        }

        public IAnimationController Animation { get; }

        public bool RunRequested { get; set; }

        public bool JumpRequested { get; set; }

        public void ChangeState(LocomotionStateKey slot) => _changeState(slot);
    }
}
