// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Cinemachine {
    public static class CharacterCinemachineKit {
        public static ICameraRig Rig(GameObject root) =>
                root != null
                        ? root.GetComponentInChildren<CameraRigManager>(true)
                        : null;
    }
}
