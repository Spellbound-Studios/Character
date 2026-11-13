// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using Spellbound.Character.Rig;
using Spellbound.Core.Logging;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Spellbound.Character.Editor.Tests {
    public class RagdollBuilderTests {
        private const float Tolerance = 1e-5f;

        private Scene _previewScene;
        private GameObject _root;
        private RagdollProfile _profile;

        [SetUp]
        public void CreateHierarchy() {
            _previewScene = EditorSceneManager.NewPreviewScene();
            _root = new GameObject("Character");
            SceneManager.MoveGameObjectToScene(_root, _previewScene);

            var hips = AddChild(_root.transform, "Hips");
            AddChild(hips, "Spine");
            AddChild(hips, "LeftUpperLeg");

            _profile = ScriptableObject.CreateInstance<RagdollProfile>();
            _profile.EditableBones.Add(new RagdollBoneDefinition {
                Name = "Hips",
                BonePath = "Hips",
                ParentIndex = -1,
                Shape = RagdollColliderShape.Box,
                ColliderCenter = new Vector3(0f, 0.05f, -0.01f),
                ColliderSize = new Vector3(0.3f, 0.2f, 0.25f),
                MassShare = 0.5f
            });
            _profile.EditableBones.Add(new RagdollBoneDefinition {
                Name = "Spine",
                BonePath = "Hips/Spine",
                ParentIndex = 0,
                Shape = RagdollColliderShape.Capsule,
                ColliderCenter = new Vector3(0f, 0.15f, 0f),
                ColliderSize = new Vector3(0.08f, 0.3f, 0f),
                CapsuleDirection = 5,
                MassShare = 0.3f,
                Joint = new RagdollJointLimits {
                    LowTwist = -20f,
                    HighTwist = 30f,
                    Swing1 = 25f,
                    Swing2 = 15f,
                    Axis = new Vector3(0f, 0f, 2f),
                    SwingAxis = new Vector3(3f, 0f, 0f)
                }
            });
            _profile.EditableBones.Add(new RagdollBoneDefinition {
                Name = "LeftUpperLeg",
                BonePath = "Hips/LeftUpperLeg",
                ParentIndex = 0,
                Shape = RagdollColliderShape.Sphere,
                ColliderCenter = new Vector3(0f, -0.2f, 0f),
                ColliderSize = new Vector3(0.1f, 0f, 0f),
                MassShare = 0.2f
            });
            _profile.Physics.SolverIterations = 12;
            _profile.Physics.ProjectionDistance = 0.2f;
        }

        [TearDown]
        public void DestroyHierarchy() {
            Object.DestroyImmediate(_profile);
            EditorSceneManager.ClosePreviewScene(_previewScene);
        }

        [Test]
        public void Build_MakesTheJointsAndCollidersTheProfileNames() {
            var bones = new List<RagdollBone>();

            var built = RagdollBuilder.Build(_root.transform, null, _profile, bones, out var error);

            Assert.IsTrue(built, error);
            Assert.IsNull(error);
            Assert.AreEqual(3, bones.Count);

            var hips = _root.transform.Find("Hips");
            var spine = _root.transform.Find("Hips/Spine");
            var leg = _root.transform.Find("Hips/LeftUpperLeg");
            var hipsBody = hips.GetComponent<Rigidbody>();

            Assert.IsNotNull(hipsBody);
            Assert.IsNull(hips.GetComponent<CharacterJoint>());

            var spineJoint = spine.GetComponent<CharacterJoint>();
            var legJoint = leg.GetComponent<CharacterJoint>();

            Assert.IsNotNull(spineJoint);
            Assert.IsNotNull(legJoint);
            Assert.AreSame(hipsBody, spineJoint.connectedBody);
            Assert.AreSame(hipsBody, legJoint.connectedBody);
            Assert.AreEqual(-20f, spineJoint.lowTwistLimit.limit);
            Assert.AreEqual(30f, spineJoint.highTwistLimit.limit);
            Assert.AreEqual(25f, spineJoint.swing1Limit.limit);
            Assert.AreEqual(15f, spineJoint.swing2Limit.limit);
            AssertVector(Vector3.forward, spineJoint.axis);
            AssertVector(Vector3.right, spineJoint.swingAxis);
            AssertVector(Vector3.right, legJoint.axis);
            AssertVector(Vector3.up, legJoint.swingAxis);
            Assert.AreEqual(_profile.TotalMass * 0.5f, hipsBody.mass, 1e-4f);
            Assert.AreSame(spineJoint, bones[1].Joint);
            Assert.AreEqual(0, bones[2].ParentIndex);
        }

        [Test]
        public void Build_SizesEachColliderFromTheProfile() {
            var bones = new List<RagdollBone>();

            Assert.IsTrue(RagdollBuilder.Build(_root.transform, null, _profile, bones, out var error), error);

            var box = _root.transform.Find("Hips").GetComponent<BoxCollider>();
            var capsule = _root.transform.Find("Hips/Spine").GetComponent<CapsuleCollider>();
            var sphere = _root.transform.Find("Hips/LeftUpperLeg").GetComponent<SphereCollider>();

            Assert.IsNotNull(box);
            AssertVector(new Vector3(0f, 0.05f, -0.01f), box.center);
            AssertVector(new Vector3(0.3f, 0.2f, 0.25f), box.size);
            Assert.IsNotNull(capsule);
            AssertVector(new Vector3(0f, 0.15f, 0f), capsule.center);
            Assert.AreEqual(0.08f, capsule.radius, Tolerance);
            Assert.AreEqual(0.3f, capsule.height, Tolerance);
            Assert.AreEqual(2, capsule.direction);
            Assert.IsNotNull(sphere);
            AssertVector(new Vector3(0f, -0.2f, 0f), sphere.center);
            Assert.AreEqual(0.1f, sphere.radius, Tolerance);
            Assert.AreSame(box, bones[0].Collider);
        }

        [Test]
        public void Build_AppliesThePhysicsSettings() {
            var bones = new List<RagdollBone>();

            Assert.IsTrue(RagdollBuilder.Build(_root.transform, null, _profile, bones, out var error), error);

            foreach (var bone in bones)
                Assert.AreEqual(12, bone.Body.solverIterations, bone.Bone.name);

            Assert.AreEqual(0.2f, bones[1].Joint.projectionDistance, Tolerance);
            Assert.IsFalse(bones[1].Joint.enableCollision);
        }

        [Test]
        public void Build_ReportsAMissingBoneAsAnError() {
            AddMissingHead();
            var bones = new List<RagdollBone>();
            var recording = new RecordingLogSink(LogLevel.Error);
            var built = true;
            string error = null;

            WithFailingMessagesIgnored(() => {
                using (Log.AddScopedSink(recording, null, LogLevel.Error)) {
                    Assert.DoesNotThrow(() => built = RagdollBuilder.Build(_root.transform, null, _profile, bones,
                        out error));
                }
            });

            Assert.IsFalse(built);
            StringAssert.Contains("Head", error);
            Assert.AreEqual(0, bones.Count);
            Assert.IsNull(_root.transform.Find("Hips").GetComponent<Rigidbody>());
            Assert.AreEqual(1, recording.CountOf(LogLevel.Error));
        }

        [Test]
        public void Build_KeepsAnAuthoredRigidbodyWhenABoneIsMissing() {
            var hips = _root.transform.Find("Hips");
            var authoredBody = hips.gameObject.AddComponent<Rigidbody>();
            AddMissingHead();
            var bones = new List<RagdollBone>();
            var built = true;

            WithFailingMessagesIgnored(() => built = RagdollBuilder.Build(_root.transform, null, _profile, bones,
                out _));

            Assert.IsFalse(built);
            Assert.AreSame(authoredBody, hips.GetComponent<Rigidbody>());
            Assert.IsNull(hips.GetComponent<Collider>());
            Assert.IsNull(_root.transform.Find("Hips/Spine").GetComponent<Rigidbody>());
        }

        [Test]
        public void Build_ReportsAnAuthoredColliderOfAnotherShapeAndKeepsIt() {
            var hips = _root.transform.Find("Hips");
            var spine = _root.transform.Find("Hips/Spine");
            var authoredBody = hips.gameObject.AddComponent<Rigidbody>();
            var authoredBox = spine.gameObject.AddComponent<BoxCollider>();
            var bones = new List<RagdollBone>();
            var built = true;
            string error = null;

            WithFailingMessagesIgnored(() => built = RagdollBuilder.Build(_root.transform, null, _profile, bones,
                out error));

            Assert.IsFalse(built);
            StringAssert.Contains("Spine", error);
            Assert.AreEqual(0, bones.Count);
            Assert.AreSame(authoredBox, spine.GetComponent<Collider>());
            Assert.IsNull(spine.GetComponent<CapsuleCollider>());
            Assert.IsNull(spine.GetComponent<Rigidbody>());
            Assert.IsNull(spine.GetComponent<CharacterJoint>());
            Assert.AreSame(authoredBody, hips.GetComponent<Rigidbody>());
            Assert.IsNull(hips.GetComponent<BoxCollider>());
        }

        [Test]
        public void Build_LeavesTheHierarchyUnchangedWhenALaterBoneFails() {
            var hips = _root.transform.Find("Hips");
            var spine = _root.transform.Find("Hips/Spine");
            var leg = _root.transform.Find("Hips/LeftUpperLeg");
            var authoredBody = hips.gameObject.AddComponent<Rigidbody>();
            authoredBody.mass = 10f;
            authoredBody.linearDamping = 3f;
            authoredBody.solverIterations = 1;
            var authoredCapsule = spine.gameObject.AddComponent<CapsuleCollider>();
            authoredCapsule.radius = 0.5f;
            authoredCapsule.height = 2f;
            leg.gameObject.AddComponent<BoxCollider>();
            var componentCounts = ComponentCountsOf(hips, spine, leg);
            var bones = new List<RagdollBone>();
            var built = true;
            string error = null;

            WithFailingMessagesIgnored(() => built = RagdollBuilder.Build(_root.transform, null, _profile, bones,
                out error));

            Assert.IsFalse(built);
            StringAssert.Contains("LeftUpperLeg", error);
            Assert.AreEqual(0, bones.Count);
            CollectionAssert.AreEqual(componentCounts, ComponentCountsOf(hips, spine, leg));
            Assert.AreEqual(10f, authoredBody.mass, Tolerance);
            Assert.AreEqual(3f, authoredBody.linearDamping, Tolerance);
            Assert.AreEqual(1, authoredBody.solverIterations);
            Assert.AreEqual(0.5f, authoredCapsule.radius, Tolerance);
            Assert.AreEqual(2f, authoredCapsule.height, Tolerance);
            AssertVector(Vector3.zero, authoredCapsule.center);
        }

        [Test]
        public void Build_AppliesTheMassAndPhysicsSettingsToEveryBone() {
            var hips = _root.transform.Find("Hips");
            var authoredBody = hips.gameObject.AddComponent<Rigidbody>();
            authoredBody.solverIterations = 1;
            authoredBody.linearDamping = 3f;
            _profile.Physics.LinearDamping = 0.25f;
            _profile.Physics.MaxAngularVelocity = 11f;
            _profile.Physics.Interpolation = RigidbodyInterpolation.Extrapolate;
            _profile.Physics.ExcludeLayers = 1 << 3;
            var bones = new List<RagdollBone>();

            Assert.IsTrue(RagdollBuilder.Build(_root.transform, null, _profile, bones, out var error), error);
            Assert.AreSame(authoredBody, bones[0].Body);

            for (var i = 0; i < bones.Count; i++) {
                var bone = bones[i];
                var name = bone.Bone.name;

                Assert.AreEqual(_profile.TotalMass * _profile.Bones[i].MassShare, bone.Body.mass, 1e-4f, name);
                Assert.AreEqual(12, bone.Body.solverIterations, name);
                Assert.AreEqual(0.25f, bone.Body.linearDamping, Tolerance, name);
                Assert.AreEqual(11f, bone.Body.maxAngularVelocity, Tolerance, name);
                Assert.AreEqual(RigidbodyInterpolation.Extrapolate, bone.Body.interpolation, name);
                Assert.AreEqual(1 << 3, bone.Collider.excludeLayers.value, name);

                if (bone.Joint == null)
                    continue;

                Assert.AreSame(bones[bone.ParentIndex].Body, bone.Joint.connectedBody, name);
                Assert.AreEqual(0.2f, bone.Joint.projectionDistance, Tolerance, name);
            }
        }

        [Test]
        public void Build_ReportsAnUnknownColliderShape() {
            _profile.EditableBones[2].Shape = (RagdollColliderShape)99;
            var bones = new List<RagdollBone>();
            var built = true;
            string error = null;

            WithFailingMessagesIgnored(() => built = RagdollBuilder.Build(_root.transform, null, _profile, bones,
                out error));

            Assert.IsFalse(built);
            StringAssert.Contains("LeftUpperLeg", error);
            StringAssert.Contains("99", error);
            Assert.AreEqual(0, bones.Count);
            Assert.IsNull(_root.transform.Find("Hips/LeftUpperLeg").GetComponent<Collider>());
            Assert.IsNull(_root.transform.Find("Hips").GetComponent<Rigidbody>());
            Assert.IsNull(_root.transform.Find("Hips/Spine").GetComponent<CharacterJoint>());
        }

        [Test]
        public void Build_InEditModeLeavesTheCollisionPairsUntouched() {
            _profile.Physics.IgnoreSelfCollision = true;
            var bones = new List<RagdollBone>();

            Assert.IsTrue(RagdollBuilder.Build(_root.transform, null, _profile, bones, out var error), error);

            var colliders = CollidersOf(bones);

            for (var first = 0; first < colliders.Count; first++) {
                for (var second = first + 1; second < colliders.Count; second++)
                    Assert.IsFalse(Physics.GetIgnoreCollision(colliders[first], colliders[second]));
            }
        }

        [Test]
        public void IgnoreSelfCollision_IgnoresEveryPairOfColliders() {
            var bones = new List<RagdollBone>();

            Assert.IsTrue(RagdollBuilder.Build(_root.transform, null, _profile, bones, out var error), error);

            var colliders = CollidersOf(bones);

            RagdollBuilder.IgnoreSelfCollision(colliders);

            for (var first = 0; first < colliders.Count; first++) {
                for (var second = first + 1; second < colliders.Count; second++)
                    Assert.IsTrue(Physics.GetIgnoreCollision(colliders[first], colliders[second]));
            }
        }

        [Test]
        public void CharacterKit_BuildRagdoll_BuildsAndReportsAMissingBone() {
            var bones = new List<RagdollBone>();

            Assert.IsTrue(CharacterKit.Rig.BuildRagdoll(_root.transform, null, _profile, bones, out var error), error);
            Assert.AreEqual(3, bones.Count);
            Assert.IsNotNull(_root.transform.Find("Hips/Spine").GetComponent<CharacterJoint>());

            RagdollBuilder.Clear(bones);
            AddMissingHead();
            var built = true;

            WithFailingMessagesIgnored(() => Assert.DoesNotThrow(() =>
                    built = CharacterKit.Rig.BuildRagdoll(_root.transform, null, _profile, bones, out error)));

            Assert.IsFalse(built);
            StringAssert.Contains("Head", error);
            Assert.AreEqual(0, bones.Count);
        }

        private void AddMissingHead() {
            _profile.EditableBones.Add(new RagdollBoneDefinition {
                Name = "Head",
                BonePath = "Hips/Spine/Neck/Head",
                ParentIndex = 1,
                Shape = RagdollColliderShape.Sphere,
                ColliderSize = new Vector3(0.1f, 0f, 0f)
            });
        }

        private static int[] ComponentCountsOf(params Transform[] transforms) {
            var counts = new int[transforms.Length];

            for (var i = 0; i < transforms.Length; i++)
                counts[i] = transforms[i].GetComponents<Component>().Length;

            return counts;
        }

        private static List<Collider> CollidersOf(List<RagdollBone> bones) {
            var colliders = new List<Collider>();

            foreach (var bone in bones)
                colliders.Add(bone.Collider);

            return colliders;
        }

        private static void WithFailingMessagesIgnored(System.Action action) {
            var ignoredFailingMessages = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            try {
                action();
            }
            finally {
                LogAssert.ignoreFailingMessages = ignoredFailingMessages;
            }
        }

        private static void AssertVector(Vector3 expected, Vector3 actual) =>
                Assert.That(Vector3.Distance(expected, actual), Is.LessThan(Tolerance),
                    $"expected {expected:F6}, was {actual:F6}");

        private static Transform AddChild(Transform parent, string childName) {
            var child = new GameObject(childName).transform;
            child.SetParent(parent, false);

            return child;
        }
    }
}
