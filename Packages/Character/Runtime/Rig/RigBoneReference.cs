// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Rig {
    [Serializable]
    public struct RigBoneReference {
        public bool UseHumanBone;
        public HumanBodyBones HumanBone;
        public string BonePath;

        public bool IsEmpty => !UseHumanBone && string.IsNullOrEmpty(BonePath);

        public static RigBoneReference Human(HumanBodyBones bone) =>
                new() { UseHumanBone = true, HumanBone = bone, BonePath = string.Empty };

        public static RigBoneReference Path(string bonePath) => new() { BonePath = bonePath };
    }
}
