// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Rig {
    [Serializable]
    public class RagdollBone {
        public Transform Bone;
        public Rigidbody Body;
        public Collider Collider;
        public CharacterJoint Joint;
        public int ParentIndex = -1;
    }
}
