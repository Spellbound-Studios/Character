// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Rig {
    public static class RagdollBuilder {
        internal static Transform ResolveBone(Transform rigRoot, Animator animator, RagdollBoneDefinition definition) {
            if (definition.UseHumanBone && animator != null && animator.isHuman) {
                var human = animator.GetBoneTransform(definition.HumanBone);

                if (human != null)
                    return human;
            }

            if (rigRoot == null || string.IsNullOrEmpty(definition.BonePath))
                return null;

            return rigRoot.Find(definition.BonePath);
        }

        public static bool Build(
            Transform rigRoot,
            Animator animator,
            RagdollProfile profile,
            List<RagdollBone> output,
            out string error) {
            if (TryBuild(rigRoot, animator, profile, output, out error))
                return true;

            Log.Error(error, rigRoot);

            return false;
        }

        private static bool TryBuild(
            Transform rigRoot,
            Animator animator,
            RagdollProfile profile,
            List<RagdollBone> output,
            out string error) {
            if (output == null) {
                error = "No list to hold the ragdoll's bones.";

                return false;
            }

            output.Clear();

            if (profile == null) {
                error = "No ragdoll profile assigned.";

                return false;
            }

            if (!profile.Validate(out error))
                return false;

            var definitions = profile.Bones;

            if (!TryResolveBones(rigRoot, animator, definitions, out var boneTransforms, out error))
                return false;

            if (!TryCheckColliders(boneTransforms, definitions, out error))
                return false;

            var added = new List<Component>();

            if (!TryAddComponents(boneTransforms, definitions, added, output, out error)) {
                RemoveAdded(added);
                output.Clear();

                return false;
            }

            ApplyProfile(profile, output);

            if (profile.Physics.IgnoreSelfCollision && Application.isPlaying)
                IgnoreSelfCollision(CollidersOf(output));

            error = null;

            return true;
        }

        private static bool TryResolveBones(
            Transform rigRoot,
            Animator animator,
            IReadOnlyList<RagdollBoneDefinition> definitions,
            out Transform[] boneTransforms,
            out string error) {
            boneTransforms = new Transform[definitions.Count];

            for (var i = 0; i < definitions.Count; i++) {
                var definition = definitions[i];
                var boneTransform = ResolveBone(rigRoot, animator, definition);

                if (boneTransform == null) {
                    error = $"Bone {i} ({definition.Name}) could not be resolved. " +
                            $"Path '{definition.BonePath}', human bone {definition.HumanBone}.";

                    return false;
                }

                boneTransforms[i] = boneTransform;
            }

            error = null;

            return true;
        }

        private static bool TryCheckColliders(
            Transform[] boneTransforms,
            IReadOnlyList<RagdollBoneDefinition> definitions,
            out string error) {
            for (var i = 0; i < definitions.Count; i++) {
                var definition = definitions[i];
                var colliderType = ColliderTypeOf(definition.Shape);

                if (colliderType == null) {
                    error = $"Bone {i} ({definition.Name}) has collider shape {(int)definition.Shape}, " +
                            "which the builder does not know.";

                    return false;
                }

                var boneTransform = boneTransforms[i];

                if (boneTransform.GetComponent(colliderType) != null)
                    continue;

                var authored = boneTransform.GetComponent<Collider>();

                if (authored != null) {
                    error = $"Bone {i} ({definition.Name}) already holds a {authored.GetType().Name} and the " +
                            $"profile names a {colliderType.Name}. Remove the authored collider or change the " +
                            "profile.";

                    return false;
                }
            }

            error = null;

            return true;
        }

        private static bool TryAddComponents(
            Transform[] boneTransforms,
            IReadOnlyList<RagdollBoneDefinition> definitions,
            List<Component> added,
            List<RagdollBone> output,
            out string error) {
            for (var i = 0; i < definitions.Count; i++) {
                var definition = definitions[i];
                var boneTransform = boneTransforms[i];
                var colliderType = ColliderTypeOf(definition.Shape);
                var hasParent = definition.ParentIndex >= 0;
                var body = GetOrAdd<Rigidbody>(boneTransform, added);
                var collider = (Collider)GetOrAdd(boneTransform, colliderType, added);
                var joint = hasParent ? GetOrAdd<CharacterJoint>(boneTransform, added) : null;

                if (body == null || collider == null || (hasParent && joint == null)) {
                    error = $"Bone {i} ({definition.Name}) could not take a Rigidbody, a {colliderType.Name}" +
                            (hasParent ? " and a CharacterJoint." : ".");

                    return false;
                }

                output.Add(new RagdollBone {
                    Bone = boneTransform,
                    Body = body,
                    Collider = collider,
                    Joint = joint,
                    ParentIndex = definition.ParentIndex
                });
            }

            error = null;

            return true;
        }

        private static void ApplyProfile(RagdollProfile profile, List<RagdollBone> bones) {
            var definitions = profile.Bones;
            var shareSum = 0f;

            foreach (var definition in definitions)
                shareSum += Mathf.Max(0f, definition.MassShare);

            if (shareSum <= 0f)
                shareSum = 1f;

            for (var i = 0; i < bones.Count; i++) {
                var bone = bones[i];
                var definition = definitions[i];

                bone.Body.mass = profile.TotalMass * Mathf.Max(0f, definition.MassShare) / shareSum;
                ConfigureCollider(bone.Collider, definition);

                if (bone.Joint != null)
                    ConfigureJoint(bone.Joint, bones[definition.ParentIndex].Body, definition.Joint);

                ApplyPhysicsSettings(bone, profile.Physics);
            }
        }

        public static void Clear(List<RagdollBone> bones) {
            for (var i = bones.Count - 1; i >= 0; i--) {
                var bone = bones[i];

                DestroyComponent(bone.Joint);
                DestroyComponent(bone.Collider);
                DestroyComponent(bone.Body);
            }

            bones.Clear();
        }

        public static void IgnoreSelfCollision(IReadOnlyList<Collider> colliders) {
            if (colliders == null)
                return;

            for (var first = 0; first < colliders.Count; first++) {
                if (colliders[first] == null)
                    continue;

                for (var second = first + 1; second < colliders.Count; second++) {
                    if (colliders[second] != null)
                        Physics.IgnoreCollision(colliders[first], colliders[second], true);
                }
            }
        }

        public static void ApplyPhysicsSettings(RagdollBone bone, RagdollPhysicsSettings settings) {
            var body = bone.Body;

            if (body != null) {
                body.interpolation = settings.Interpolation;
                body.collisionDetectionMode = settings.CollisionDetection;
                body.solverIterations = settings.SolverIterations;
                body.solverVelocityIterations = settings.SolverVelocityIterations;
                body.linearDamping = settings.LinearDamping;
                body.angularDamping = settings.AngularDamping;
                body.maxAngularVelocity = settings.MaxAngularVelocity;
                body.maxLinearVelocity = settings.MaxLinearVelocity;
                body.maxDepenetrationVelocity = settings.MaxDepenetrationVelocity;
                body.useGravity = true;
            }

            if (bone.Collider != null)
                bone.Collider.excludeLayers = settings.ExcludeLayers;

            if (bone.Joint != null) {
                bone.Joint.enableCollision = false;
                bone.Joint.enableProjection = settings.EnableProjection;
                bone.Joint.projectionDistance = settings.ProjectionDistance;
                bone.Joint.projectionAngle = settings.ProjectionAngle;
                bone.Joint.enablePreprocessing = settings.EnablePreprocessing;
            }
        }

        public static void SetDormant(IReadOnlyList<RagdollBone> bones) {
            for (var i = 0; i < bones.Count; i++) {
                var bone = bones[i];

                if (bone.Body != null) {
                    bone.Body.isKinematic = true;
                    bone.Body.detectCollisions = false;
                }

                if (bone.Collider != null)
                    bone.Collider.enabled = false;
            }
        }

        private static Type ColliderTypeOf(RagdollColliderShape shape) =>
                shape switch {
                    RagdollColliderShape.Capsule => typeof(CapsuleCollider),
                    RagdollColliderShape.Box => typeof(BoxCollider),
                    RagdollColliderShape.Sphere => typeof(SphereCollider),
                    _ => null
                };

        private static void ConfigureCollider(Collider collider, RagdollBoneDefinition definition) {
            switch (collider) {
                case CapsuleCollider capsule:
                    capsule.center = definition.ColliderCenter;
                    capsule.radius = definition.ColliderSize.x;
                    capsule.height = definition.ColliderSize.y;
                    capsule.direction = Mathf.Clamp(definition.CapsuleDirection, 0, 2);

                    break;
                case BoxCollider box:
                    box.center = definition.ColliderCenter;
                    box.size = definition.ColliderSize;

                    break;
                case SphereCollider sphere:
                    sphere.center = definition.ColliderCenter;
                    sphere.radius = definition.ColliderSize.x;

                    break;
            }
        }

        private static T GetOrAdd<T>(Transform target, List<Component> added) where T : Component =>
                (T)GetOrAdd(target, typeof(T), added);

        private static Component GetOrAdd(Transform target, Type type, List<Component> added) {
            var existing = target.GetComponent(type);

            if (existing != null)
                return existing;

            var component = target.gameObject.AddComponent(type);

            if (component != null)
                added.Add(component);

            return component;
        }

        private static void RemoveAdded(List<Component> added) {
            for (var i = added.Count - 1; i >= 0; i--)
                DestroyComponent(added[i]);

            added.Clear();
        }

        private static List<Collider> CollidersOf(List<RagdollBone> bones) {
            var colliders = new List<Collider>(bones.Count);

            foreach (var bone in bones)
                colliders.Add(bone.Collider);

            return colliders;
        }

        private static void ConfigureJoint(CharacterJoint joint, Rigidbody connectedBody, RagdollJointLimits limits) {
            joint.connectedBody = connectedBody;

            joint.axis = limits.Axis.sqrMagnitude > 0f
                    ? limits.Axis.normalized
                    : Vector3.right;

            joint.swingAxis = limits.SwingAxis.sqrMagnitude > 0f
                    ? limits.SwingAxis.normalized
                    : Vector3.up;

            var low = joint.lowTwistLimit;
            low.limit = limits.LowTwist;
            joint.lowTwistLimit = low;

            var high = joint.highTwistLimit;
            high.limit = limits.HighTwist;
            joint.highTwistLimit = high;

            var swing1 = joint.swing1Limit;
            swing1.limit = limits.Swing1;
            joint.swing1Limit = swing1;

            var swing2 = joint.swing2Limit;
            swing2.limit = limits.Swing2;
            joint.swing2Limit = swing2;
        }

        private static void DestroyComponent(Component component) {
            if (component == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(component);
            else
                Object.DestroyImmediate(component);
        }
    }
}
