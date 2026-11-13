// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spellbound.Character.Editor.Tests {
    public class RagdollProfileFixtureTests {
        private const string Fixtures = "Packages/com.spellboundstudios.character/Tests/Editor/Fixtures/";
        private const float AxisTolerance = 1e-5f;
        private const float MassTolerance = 1e-4f;

        private static readonly (string bone, int parent, float mass, Vector3 axis, Vector3 swingAxis, float lowTwist,
                float highTwist, float swing1, float swing2)[] ExpectedJoints = {
                    ("Hips", -1, 9.813085f, Vector3.zero, Vector3.zero, 0f, 0f, 0f, 0f),
                    ("Spine", 0, 6.542056f, new Vector3(1f, 0f, 0f), new Vector3(0f, 4.917383e-07f, 1f), -20f, 40f,
                        20f, 20f),
                    ("Chest", 1, 13.08411f, new Vector3(1f, 0f, 0f), new Vector3(0f, 5.066395e-07f, 1f), -20f, 30f,
                        15f, 15f),
                    ("Head", 2, 5.233645f, new Vector3(1f, -2.842171e-13f, -4.689582e-13f),
                        new Vector3(5.684342e-13f, 2.384186e-07f, 1f), -40f, 40f, 30f, 30f),
                    ("Left Upper Arm", 2, 2.616822f, new Vector3(0.9999573f, -1.713633e-06f, -0.009238123f),
                        new Vector3(0.009238082f, 1.414912e-06f, 0.9999573f), -60f, 100f, 70f, 40f),
                    ("Left Forearm", 4, 1.962617f, new Vector3(0.9999665f, 7.603501e-07f, -0.00818254f),
                        new Vector3(0.008182327f, 1.131021e-05f, 0.9999665f), -5f, 140f, 5f, 30f),
                    ("Right Upper Arm", 2, 2.616822f, new Vector3(0.9999573f, 1.713633e-06f, 0.009238123f),
                        new Vector3(-0.009238082f, 1.414912e-06f, 0.9999573f), -60f, 100f, 70f, 40f),
                    ("Right Forearm", 6, 1.962617f, new Vector3(0.9999665f, -7.603501e-07f, 0.00818254f),
                        new Vector3(-0.008182327f, 1.131021e-05f, 0.9999665f), -5f, 140f, 5f, 30f),
                    ("Left Thigh", 0, 7.850467f, new Vector3(-0.9996227f, -1.788139e-07f, -0.02746617f),
                        new Vector3(0.02746617f, -1.013279e-06f, -0.9996227f), -30f, 100f, 40f, 30f),
                    ("Left Shin", 8, 5.233645f, new Vector3(0.9997758f, 9.918585e-07f, 0.0211745f),
                        new Vector3(-0.02117449f, -1.683831e-06f, 0.9997758f), -5f, 130f, 5f, 20f),
                    ("Right Thigh", 0, 7.850467f, new Vector3(-0.9996226f, -7.897615e-07f, 0.02747107f),
                        new Vector3(-0.02747106f, -2.726912e-06f, -0.9996226f), -30f, 100f, 40f, 30f),
                    ("Right Shin", 10, 5.233645f, new Vector3(0.9997757f, -3.241003e-07f, -0.02117962f),
                        new Vector3(0.02117962f, -3.278255e-06f, 0.9997757f), -5f, 130f, 5f, 20f)
                };

        private Scene _previewScene;

        [SetUp]
        public void OpenScene() {
            _previewScene = EditorSceneManager.NewPreviewScene();
        }

        [TearDown]
        public void CloseScene() {
            EditorSceneManager.ClosePreviewScene(_previewScene);
        }

        [TestCase("DwarfHumanoidRagdollProfile")]
        [TestCase("PlayerDwarfMaleRagdollProfile")]
        public void Build_GivesTheJointsTheProfileFixtureRecords(string fixture) {
            var profile = AssetDatabase.LoadAssetAtPath<RagdollProfile>(Fixtures + fixture + ".asset");

            Assert.IsNotNull(profile, fixture + " does not load as a RagdollProfile.");
            Assert.AreEqual(ExpectedJoints.Length, profile.Bones.Count);

            var root = new GameObject(fixture);
            SceneManager.MoveGameObjectToScene(root, _previewScene);

            foreach (var definition in profile.Bones)
                AddPath(root.transform, definition.BonePath);

            var bones = new List<RagdollBone>();

            Assert.IsTrue(RagdollBuilder.Build(root.transform, null, profile, bones, out var error), error);
            Assert.AreEqual(ExpectedJoints.Length, bones.Count);

            for (var i = 0; i < ExpectedJoints.Length; i++) {
                var expected = ExpectedJoints[i];
                var bone = bones[i];

                Assert.AreEqual(expected.bone, profile.Bones[i].Name);
                Assert.AreEqual(expected.parent, bone.ParentIndex, expected.bone);
                Assert.AreEqual(expected.mass, bone.Body.mass, MassTolerance, expected.bone);

                if (expected.parent < 0) {
                    Assert.IsNull(bone.Joint, expected.bone);

                    continue;
                }

                var joint = bone.Joint;

                Assert.IsNotNull(joint, expected.bone);
                Assert.AreSame(bones[expected.parent].Body, joint.connectedBody, expected.bone);
                Assert.That(Vector3.Distance(expected.axis, joint.axis), Is.LessThan(AxisTolerance), expected.bone);
                Assert.That(Vector3.Distance(expected.swingAxis, joint.swingAxis), Is.LessThan(AxisTolerance),
                    expected.bone);
                Assert.AreEqual(expected.lowTwist, joint.lowTwistLimit.limit, expected.bone);
                Assert.AreEqual(expected.highTwist, joint.highTwistLimit.limit, expected.bone);
                Assert.AreEqual(expected.swing1, joint.swing1Limit.limit, expected.bone);
                Assert.AreEqual(expected.swing2, joint.swing2Limit.limit, expected.bone);
            }
        }

        private static void AddPath(Transform root, string path) {
            var parent = root;

            foreach (var segment in path.Split('/')) {
                var child = parent.Find(segment);

                if (child == null) {
                    child = new GameObject(segment).transform;
                    child.SetParent(parent, false);
                }

                parent = child;
            }
        }
    }
}
