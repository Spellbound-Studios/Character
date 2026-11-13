// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Rig {
    [Serializable]
    public class RagdollBoneDefinition {
        public string Name;
        public string BonePath;
        public bool UseHumanBone;
        public HumanBodyBones HumanBone;
        public int ParentIndex = -1;
        public RagdollColliderShape Shape;
        public Vector3 ColliderCenter;
        public Vector3 ColliderSize;
        public int CapsuleDirection = 1;
        public float MassShare = 0.1f;
        public RagdollJointLimits Joint;
    }
}
