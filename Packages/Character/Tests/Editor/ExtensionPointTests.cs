// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Character.Rig;
using Spellbound.Character.Tests;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class ExtensionPointTests {
        private const string Folder = "Assets/CharacterExtensionPointTests";

        [TearDown]
        public void DeleteFolder() {
            AssetDatabase.DeleteAsset(Folder);
        }

        [Test]
        public void ICameraRig_CustomImplementation() {
            var close = new GameObject("close-camera");
            var far = new GameObject("far-camera");
            var hero = new GameObject("hero");

            try {
                var named = new NamedCameraRig();
                named.Add("Close", close.transform);
                named.Add("Far", far.transform);
                ICameraRig rig = named;
                string changedFrom = "unset";
                string changedTo = null;
                rig.CurrentChanged += (previous, current) => {
                    changedFrom = previous;
                    changedTo = current;
                };

                rig.SetFollowTarget(hero.transform);

                Assert.IsTrue(rig.Switch("Far"));
                Assert.IsFalse(rig.Switch("Overhead"));
                Assert.AreEqual("Far", rig.Current);
                Assert.AreSame(far.transform, rig.CurrentCameraTransform);
                Assert.IsNull(changedFrom);
                Assert.AreEqual("Far", changedTo);
                CollectionAssert.AreEqual(new[] { "Close", "Far" }, rig.CameraNames);
                Assert.AreSame(hero.transform, named.FollowTarget);
            }
            finally {
                Object.DestroyImmediate(close);
                Object.DestroyImmediate(far);
                Object.DestroyImmediate(hero);
            }
        }

        [Test]
        public void IAnimationController_CustomImplementation() {
            var recording = new RecordingAnimationController();
            IAnimationController controller = recording;
            var runState = 1802;
            var blendParameter = 77;

            controller.CrossFade(runState, 1, 0.25f);
            controller.SetFloat(blendParameter, AnimationMath.NormalizeBlend(3f, new[] { 0f, 2f, 4f }));
            controller.SetLayerWeight(1, 0.5f);

            Assert.AreEqual(runState, recording.StateOn(1));
            Assert.AreEqual(0, recording.StateOn(0));
            Assert.AreEqual(0.25f, recording.LastFade);
            Assert.AreEqual(0.75f, recording.Parameter(blendParameter), 1e-5f);
            Assert.AreEqual(0.5f, recording.LayerWeight(1));
        }

        [Test]
        public void IPlayableCharacter_CustomImplementation() {
            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets", "CharacterExtensionPointTests");

            var instance = new GameObject("Companion");
            instance.AddComponent<PlayableTestCompanion>();

            try {
                PrefabUtility.SaveAsPrefabAsset(instance, Folder + "/Companion.prefab");
            }
            finally {
                Object.DestroyImmediate(instance);
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Companion.prefab");

            Assert.IsNotNull(prefab);
            Assert.IsTrue(CharacterKit.Body.IsPlayable(prefab));
        }
    }
}
