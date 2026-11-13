// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Character.Body;
using UnityEngine;
using Object = UnityEngine.Object;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Spellbound.Character.Editor.Tests {
    public class CharacterCinemachineKitTests {
        private const string Bridge = "Spellbound.Character.Cinemachine";
        private const string CinemachinePackage = "com.unity.cinemachine";

        private static readonly Version LowestBridgedVersion = new(3, 1, 6);
        private static readonly Version FirstUnbridgedVersion = new(4, 0, 0);

        private static Type BridgeType(string name) {
            var type = Type.GetType(Bridge + "." + name + ", " + Bridge);

            if (type != null)
                return type;

            var installed = PackageInfo.FindForPackageName(CinemachinePackage);

            if (installed != null && IsBridged(installed.version)) {
                Assert.Fail(CinemachinePackage + " " + installed.version + " is installed inside the bridge's range " +
                            "and " + Bridge + "." + name + " is missing.");
            }

            Assert.Ignore("The Cinemachine bridge is not compiled because " + CinemachinePackage + " is " +
                          (installed == null ? "absent" : installed.version + ", outside the bridge's range") + ".");

            return null;
        }

        private static bool IsBridged(string packageVersion) {
            var numbers = packageVersion.Split('-')[0];

            if (!Version.TryParse(numbers, out var version))
                return false;

            return version >= LowestBridgedVersion && version < FirstUnbridgedVersion;
        }

        private static ICameraRig Rig(GameObject root) =>
                (ICameraRig)BridgeType("CharacterCinemachineKit").GetMethod("Rig").Invoke(null, new object[] { root });

        [Test]
        public void Rig_FindsTheCameraRigOnTheRootOrItsChildren() {
            var managerType = BridgeType("CameraRigManager");
            var root = new GameObject("camera-root");

            try {
                var child = new GameObject("camera-rig");
                child.transform.SetParent(root.transform);
                var manager = child.AddComponent(managerType);

                Assert.AreSame(manager, Rig(root));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Rig_FindsTheCameraRigOnAnInactiveChild() {
            var managerType = BridgeType("CameraRigManager");
            var root = new GameObject("camera-root");

            try {
                var child = new GameObject("camera-rig");
                child.SetActive(false);
                child.transform.SetParent(root.transform);
                var manager = child.AddComponent(managerType);

                Assert.AreSame(manager, Rig(root));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Rig_IsNullWhenNoCameraRigIsFound() {
            var root = new GameObject("camera-root");

            try {
                Assert.IsNull(Rig(root));
                Assert.IsNull(Rig(null));
            }
            finally {
                Object.DestroyImmediate(root);
            }
        }
    }
}
