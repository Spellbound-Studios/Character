// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Rig;
using Spellbound.Core;
using Spellbound.Core.Observations;
using Spellbound.Core.Operations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class CharacterOperationsTests {
        private const string ParentFolder = "Assets/_Project";
        private const string ScratchFolder = ParentFolder + "/TestScratch";
        private const string FigureName = "CharacterOperationsFigure";
        private const string CompleteProfilePath = ScratchFolder + "/CompleteRagdollProfile.asset";
        private const string IncompleteProfilePath = ScratchFolder + "/IncompleteRagdollProfile.asset";
        private const string ContractPath = ScratchFolder + "/SocketsRigContract.asset";

        private GameObject _figure;

        [SetUp]
        public void CreateFigureAndAssets() {
            if (!AssetDatabase.IsValidFolder(ScratchFolder))
                AssetDatabase.CreateFolder(ParentFolder, "TestScratch");

            _figure = new GameObject(FigureName);
            var hips = AddChild(_figure.transform, "Hips");
            AddChild(hips, "Spine");
            AddChild(hips, "socket_hand");

            var complete = ScriptableObject.CreateInstance<RagdollProfile>();
            AddBones(complete);
            AssetDatabase.CreateAsset(complete, CompleteProfilePath);

            var incomplete = ScriptableObject.CreateInstance<RagdollProfile>();
            AddBones(incomplete);
            incomplete.EditableBones.Add(new RagdollBoneDefinition {
                Name = "Head",
                BonePath = "Hips/Spine/Head",
                ParentIndex = 1,
                Shape = RagdollColliderShape.Sphere,
                MassShare = 0.1f
            });
            AssetDatabase.CreateAsset(incomplete, IncompleteProfilePath);

            var contract = ScriptableObject.CreateInstance<RigContract>();
            contract.EditableSockets.Add(new RigSocket { Id = "hand", ParentBone = RigBoneReference.Path("Hips") });
            contract.EditableSockets.Add(new RigSocket {
                Id = "back",
                ParentBone = RigBoneReference.Path("Hips/Spine")
            });
            contract.EditableSockets.Add(new RigSocket {
                Id = "tail",
                ParentBone = RigBoneReference.Path("Hips/Tail")
            });
            AssetDatabase.CreateAsset(contract, ContractPath);
            AssetDatabase.SaveAssets();
        }

        [TearDown]
        public void DestroyFigureAndAssets() {
            Object.DestroyImmediate(_figure);
            AssetDatabase.DeleteAsset(ScratchFolder);
        }

        [Test]
        public void Help_ListsBothOperations() {
            var text = Text("core help --group character");

            StringAssert.Contains("character sockets", text);
            StringAssert.Contains("character ragdoll", text);
            StringAssert.Contains("--contract", text);
            StringAssert.Contains("--profile", text);
        }

        [Test]
        public void Ragdoll_OnACompleteModel_WouldBuild() {
            var text = Text("character ragdoll --root scene:/" + FigureName + " --profile asset:" + CompleteProfilePath);

            StringAssert.StartsWith("succeeded: would build 2 bones", text);
        }

        [Test]
        public void Ragdoll_OnAnIncompleteModel_FailsNamingTheBone() {
            var text = Text("character ragdoll --root scene:/" + FigureName + " --profile asset:"
                            + IncompleteProfilePath);

            StringAssert.StartsWith("failed:", text);
            StringAssert.Contains("Head", text);
        }

        [Test]
        public void Sockets_ListsEachSocketWithItsState() {
            var lines = Text("character sockets --root scene:/" + FigureName + " --contract asset:" + ContractPath)
                    .Split('\n');

            Assert.AreEqual("succeeded: 3 sockets, 1 defaulted, 1 missing", lines[0]);
            StringAssert.StartsWith("id", lines[1]);
            Assert.AreEqual(5, lines.Length);
            Assert.That(lines[2], Does.StartWith("hand").And.Contains("node").And.Contains("Hips/socket_hand"));
            Assert.That(lines[3], Does.StartWith("back").And.Contains("default").And.Contains("Hips/Spine"));
            Assert.That(lines[4], Does.StartWith("tail").And.Contains("missing"));
        }

        [Test]
        public void Ragdoll_OfARootNotInTheScene_IsInvalid() {
            var text = Text("character ragdoll --root scene:/NoSuchCharacterFigure --profile asset:"
                            + CompleteProfilePath);

            StringAssert.StartsWith("invalid:", text);
        }

        private static void AddBones(RagdollProfile profile) {
            profile.EditableBones.Add(new RagdollBoneDefinition {
                Name = "Hips",
                BonePath = "Hips",
                ParentIndex = -1,
                Shape = RagdollColliderShape.Box,
                MassShare = 0.6f
            });
            profile.EditableBones.Add(new RagdollBoneDefinition {
                Name = "Spine",
                BonePath = "Hips/Spine",
                ParentIndex = 0,
                Shape = RagdollColliderShape.Capsule,
                MassShare = 0.4f
            });
        }

        private static Transform AddChild(Transform parent, string name) {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);

            return child;
        }

        private static string Text(string line) =>
            CoreKit.Operations.Run(line, OperationCaller.Development(), ObservationContext.Discard).Result.Text;
    }
}
