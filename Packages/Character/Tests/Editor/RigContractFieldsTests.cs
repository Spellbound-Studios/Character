// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class RigContractFieldsTests {
        private RigContract _contract;

        [SetUp]
        public void CreateContract() {
            _contract = ScriptableObject.CreateInstance<RigContract>();
        }

        [TearDown]
        public void DestroyContract() {
            Object.DestroyImmediate(_contract);
        }

        [Test]
        public void NewContract_IsHumanoidWithUnitysFifteenRequiredBones() {
            var expected = new[] {
                HumanBodyBones.Hips, HumanBodyBones.Spine, HumanBodyBones.Head, HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot, HumanBodyBones.RightUpperLeg,
                HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot, HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand, HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand
            };

            Assert.AreEqual(RigKind.Humanoid, _contract.Kind);
            CollectionAssert.AreEqual(expected, _contract.RequiredHumanBones);
        }

        [Test]
        public void NewContract_HasEmptyListsAndNoReferences() {
            Assert.IsEmpty(_contract.RequiredBonePaths);
            Assert.IsEmpty(_contract.Sockets);
            Assert.IsEmpty(_contract.Grips);
            Assert.IsEmpty(_contract.Pieces);
            Assert.IsEmpty(_contract.Poses);
            Assert.IsNull(_contract.RagdollProfile);
            Assert.IsNull(_contract.ReferenceFigure);
        }

        [Test]
        public void NewContract_HasTheDefaultHipsAndHeightRange() {
            var hips = _contract.Hips;

            Assert.IsNotNull(hips);
            Assert.IsTrue(hips.Bone.UseHumanBone);
            Assert.AreEqual(HumanBodyBones.Hips, hips.Bone.HumanBone);
            Assert.AreEqual(1f, hips.HeightMetres);
            Assert.AreEqual(Vector3.forward, hips.FacingAxis);
            Assert.AreEqual(0.5f, _contract.MinimumHeightMetres);
            Assert.AreEqual(3f, _contract.MaximumHeightMetres);
        }

        [Test]
        public void Setters_StoreTheirValues() {
            var profile = ScriptableObject.CreateInstance<RagdollProfile>();
            var figure = new GameObject("Figure");
            var hips = new RigHips { Bone = RigBoneReference.Path("Root/Pelvis"), HeightMetres = 0.9f };

            try {
                _contract.Kind = RigKind.Generic;
                _contract.Hips = hips;
                _contract.RagdollProfile = profile;
                _contract.MinimumHeightMetres = 0.25f;
                _contract.MaximumHeightMetres = 6f;
                _contract.ReferenceFigure = figure;

                Assert.AreEqual(RigKind.Generic, _contract.Kind);
                Assert.AreSame(hips, _contract.Hips);
                Assert.AreSame(profile, _contract.RagdollProfile);
                Assert.AreEqual(0.25f, _contract.MinimumHeightMetres);
                Assert.AreEqual(6f, _contract.MaximumHeightMetres);
                Assert.AreSame(figure, _contract.ReferenceFigure);
            }
            finally {
                Object.DestroyImmediate(profile);
                Object.DestroyImmediate(figure);
            }
        }

        [Test]
        public void Hips_IsNeverNull() {
            _contract.Hips = null;

            Assert.IsNotNull(_contract.Hips);
        }

        [Test]
        public void EditableLists_AreTheListsTheReadersShow() {
            var clip = new AnimationClip();

            try {
                _contract.EditableRequiredHumanBones.Clear();
                _contract.EditableRequiredHumanBones.Add(HumanBodyBones.Jaw);
                _contract.EditableRequiredBonePaths.Add("Root/Pelvis");
                _contract.EditableSockets.Add(new RigSocket { Id = "hand" });
                _contract.EditableGrips.Add(new RigGrip { Id = "handle", SocketId = "hand" });
                _contract.EditablePieces.Add(new RigPieceSlot { Id = "torso" });
                _contract.EditablePoses.Add(clip);

                CollectionAssert.AreEqual(new[] { HumanBodyBones.Jaw }, _contract.RequiredHumanBones);
                CollectionAssert.AreEqual(new[] { "Root/Pelvis" }, _contract.RequiredBonePaths);
                Assert.AreEqual("hand", _contract.Sockets[0].Id);
                Assert.AreEqual("handle", _contract.Grips[0].Id);
                Assert.AreEqual("torso", _contract.Pieces[0].Id);
                Assert.AreSame(clip, _contract.Poses[0]);
            }
            finally {
                Object.DestroyImmediate(clip);
            }
        }

        [Test]
        public void FindSocket_MatchesTheIdOrdinally() {
            var hand = new RigSocket { Id = "hand" };
            _contract.EditableSockets.Add(hand);
            _contract.EditableSockets.Add(new RigSocket { Id = "hand" });

            Assert.AreSame(hand, _contract.FindSocket("hand"));
            Assert.IsNull(_contract.FindSocket("Hand"));
            Assert.IsNull(_contract.FindSocket("back"));
            Assert.IsNull(_contract.FindSocket(null));
        }

        [Test]
        public void SerializedFields_AreInTheContractsOrder() {
            var expected = new[] {
                "m_Script", "rigKind", "requiredHumanBones", "requiredBonePaths", "sockets", "grips", "pieces", "hips",
                "ragdollProfile", "minimumHeightMetres", "maximumHeightMetres", "referenceFigure", "poses"
            };
            var names = new List<string>();
            var serialized = new SerializedObject(_contract);
            var property = serialized.GetIterator();
            var enterChildren = true;

            while (property.NextVisible(enterChildren)) {
                enterChildren = false;
                names.Add(property.name);
            }

            CollectionAssert.AreEqual(expected, names);
        }
    }
}
