// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Spellbound.Character.Rig;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class RigContractChangeMarkerTests {
        private readonly List<Object> _created = new();
        private RigContract _contract;

        [SetUp]
        public void CreateContract() {
            _contract = Create<RigContract>();
        }

        [TearDown]
        public void DestroyCreated() {
            foreach (var created in _created) {
                if (created != null)
                    Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        [Test]
        public void TwoFreshContracts_HashEqual() {
            var other = Create<RigContract>();

            Assert.AreEqual(_contract.ContentHash, other.ContentHash);
        }

        [Test]
        public void Kind_MovesTheHash() {
            AssertHashMoves(() => _contract.Kind = RigKind.Generic);
        }

        [Test]
        public void RequiredHumanBones_MovesTheHash() {
            AssertHashMoves(() => _contract.EditableRequiredHumanBones.Add(HumanBodyBones.Neck));
        }

        [Test]
        public void RequiredHumanBonesOrder_MovesTheHash() {
            AssertHashMoves(() => _contract.EditableRequiredHumanBones.Reverse());
        }

        [Test]
        public void RequiredBonePaths_MovesTheHash() {
            AssertHashMoves(() => _contract.EditableRequiredBonePaths.Add("Root/Hips"));
        }

        [Test]
        public void Sockets_MovesTheHash() {
            AssertHashMoves(() => _contract.EditableSockets.Add(new RigSocket { Id = "hand" }));
        }

        [Test]
        public void SocketDefaultPose_MovesTheHash() {
            var socket = new RigSocket { Id = "hand", ParentBone = RigBoneReference.Path("Hips") };
            _contract.EditableSockets.Add(socket);

            AssertHashMoves(() => socket.DefaultLocalPosition = new Vector3(0f, 0.1f, 0f));
        }

        [Test]
        public void Grips_MovesTheHash() {
            AssertHashMoves(() => _contract.EditableGrips.Add(new RigGrip { Id = "handle", SocketId = "hand" }));
        }

        [Test]
        public void Pieces_MovesTheHash() {
            AssertHashMoves(() => _contract.EditablePieces.Add(new RigPieceSlot { Id = "head" }));
        }

        [Test]
        public void HipsBone_MovesTheHash() {
            AssertHashMoves(() => _contract.Hips.Bone = RigBoneReference.Path("Root/Hips"));
        }

        [Test]
        public void HipsHeight_MovesTheHash() {
            AssertHashMoves(() => _contract.Hips.HeightMetres = 1.25f);
        }

        [Test]
        public void FacingAxis_MovesTheHash() {
            AssertHashMoves(() => _contract.Hips.FacingAxis = Vector3.right);
        }

        [Test]
        public void RagdollProfile_MovesTheHash() {
            var profile = Create<RagdollProfile>();
            profile.name = "Profile";

            AssertHashMoves(() => _contract.RagdollProfile = profile);
        }

        [Test]
        public void MinimumHeight_MovesTheHash() {
            AssertHashMoves(() => _contract.MinimumHeightMetres = 0.75f);
        }

        [Test]
        public void MaximumHeight_MovesTheHash() {
            AssertHashMoves(() => _contract.MaximumHeightMetres = 2.5f);
        }

        [Test]
        public void ReferenceFigure_MovesTheHash() {
            var figure = new GameObject("Figure");
            _created.Add(figure);

            AssertHashMoves(() => _contract.ReferenceFigure = figure);
        }

        [Test]
        public void Poses_MovesTheHash() {
            var pose = new AnimationClip { name = "Pose" };
            _created.Add(pose);

            AssertHashMoves(() => _contract.EditablePoses.Add(pose));
        }

        [Test]
        public void StringsAcrossFields_DoNotCollide() {
            var other = Create<RigContract>();
            _contract.EditableRequiredBonePaths.Add("a");
            _contract.EditableRequiredBonePaths.Add("b");
            other.EditableRequiredBonePaths.Add("a\nb");

            Assert.AreNotEqual(_contract.ContentHash, other.ContentHash);
        }

        private void AssertHashMoves(Action change) {
            var before = _contract.ContentHash;

            change();

            Assert.AreNotEqual(before, _contract.ContentHash);
        }

        private T Create<T>() where T : ScriptableObject {
            var created = ScriptableObject.CreateInstance<T>();
            _created.Add(created);

            return created;
        }
    }
}
