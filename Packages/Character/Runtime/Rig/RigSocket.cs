// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Rig {
    [Serializable]
    public sealed class RigSocket {
        private const string IdPlaceholder = "{id}";

        public string Id;
        public RigBoneReference ParentBone;
        public string NodeNamePattern = "socket_{id}";
        public Vector3 DefaultLocalPosition;
        public Vector3 DefaultLocalEulerAngles;

        public string NodeName => (NodeNamePattern ?? string.Empty).Replace(IdPlaceholder, Id ?? string.Empty);

        public Pose DefaultLocalPose => new(DefaultLocalPosition, Quaternion.Euler(DefaultLocalEulerAngles));
    }
}
