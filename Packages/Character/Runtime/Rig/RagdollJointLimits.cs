// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Rig {
    [Serializable]
    public struct RagdollJointLimits {
        public float LowTwist;
        public float HighTwist;
        public float Swing1;
        public float Swing2;
        public Vector3 Axis;
        public Vector3 SwingAxis;
    }
}
