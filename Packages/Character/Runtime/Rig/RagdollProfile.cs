// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Character.Rig {
    [CreateAssetMenu(fileName = "RagdollProfile", menuName = "Spellbound/Character/Ragdoll Profile")]
    public class RagdollProfile : ScriptableObject {
        [SerializeField] private List<RagdollBoneDefinition> bones = new();
        [SerializeField] private float totalMass = 70f;
        [SerializeField] private RagdollPhysicsSettings physics = new();
        [SerializeField] private Vector3 hipsForwardLocal = Vector3.forward;
        [SerializeField] private Vector3 hipsUpLocal = Vector3.up;
        [SerializeField] private float hipsHeight = 1f;

        public IReadOnlyList<RagdollBoneDefinition> Bones => bones;

        public List<RagdollBoneDefinition> EditableBones => bones;

        public float TotalMass => totalMass;

        public RagdollPhysicsSettings Physics => physics;

        public Vector3 HipsForwardLocal {
            get => hipsForwardLocal;
            set => hipsForwardLocal = value;
        }

        public Vector3 HipsUpLocal {
            get => hipsUpLocal;
            set => hipsUpLocal = value;
        }

        public float HipsHeight {
            get => hipsHeight;
            set => hipsHeight = value;
        }

        public void NormalizeMassShares() {
            var sum = 0f;

            foreach (var bone in bones)
                sum += Mathf.Max(0f, bone.MassShare);

            if (sum <= 0f)
                return;

            foreach (var bone in bones)
                bone.MassShare = Mathf.Max(0f, bone.MassShare) / sum;
        }

        public bool Validate(out string error) {
            error = null;

            if (bones.Count == 0) {
                error = "Profile has no bones.";

                return false;
            }

            if (bones[0].ParentIndex != -1) {
                error = "The first bone must be the root (hips) with ParentIndex -1.";

                return false;
            }

            for (var i = 1; i < bones.Count; i++) {
                var parent = bones[i].ParentIndex;

                if (parent < 0 || parent >= i) {
                    error = $"Bone {i} ({bones[i].Name}) must reference a parent earlier in the list.";

                    return false;
                }
            }

            return true;
        }
    }
}