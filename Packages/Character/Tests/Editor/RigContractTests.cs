// Copyright (c) Spellbound Studios. All rights reserved.

using System.Reflection;
using NUnit.Framework;
using Spellbound.Character.Rig;
using Spellbound.Core.Authoring;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class RigContractTests {
        [Test]
        public void NewContract_ValidatesWithNoFinding() {
            var contract = ScriptableObject.CreateInstance<RigContract>();

            try {
                var report = new ValidationReport(contract.name);
                contract.Validate(report);

                Assert.IsTrue(report.IsEmpty, report.ToText());
            }
            finally {
                Object.DestroyImmediate(contract);
            }
        }

        [Test]
        public void ContentHash_IsEqualAcrossReads() {
            var contract = ScriptableObject.CreateInstance<RigContract>();
            var other = ScriptableObject.CreateInstance<RigContract>();

            try {
                var first = contract.ContentHash;
                var second = contract.ContentHash;

                Assert.AreEqual(first, second);
                Assert.AreEqual(first, other.ContentHash);
            }
            finally {
                Object.DestroyImmediate(contract);
                Object.DestroyImmediate(other);
            }
        }

        [Test]
        public void RigContract_IsInTheCreateMenu() {
            var attribute = typeof(RigContract).GetCustomAttribute<CreateAssetMenuAttribute>();

            Assert.IsNotNull(attribute);
            Assert.AreEqual("Spellbound/Character/Rig Contract", attribute.menuName);
        }
    }
}
