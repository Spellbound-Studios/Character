// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Rig {
    [Serializable]
    public sealed class RigHips {
        public RigBoneReference Bone = RigBoneReference.Human(HumanBodyBones.Hips);
        public float HeightMetres = 1f;
        public Vector3 FacingAxis = Vector3.forward;
    }
}
