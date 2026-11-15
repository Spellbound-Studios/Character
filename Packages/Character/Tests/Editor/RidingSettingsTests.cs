// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core.Authoring;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class RidingSettingsTests {
        private RidingSettings _settings;

        [SetUp]
        public void CreateSettings() {
            _settings = ScriptableObject.CreateInstance<RidingSettings>();
        }

        [TearDown]
        public void DestroySettings() {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void RideableLayers_DefaultsToEverything() {
            Assert.AreEqual(~0, _settings.rideableLayers.value);
        }

        [Test]
        public void Validate_ReportsNothingOnANewAsset() {
            var report = new ValidationReport("riding settings");

            _settings.Validate(report);

            Assert.IsTrue(report.IsEmpty);
        }

        [Test]
        public void Validate_RefusesAMissingReport() {
            Assert.Throws<ArgumentNullException>(() => _settings.Validate(null));
        }

        [Test]
        public void Menu_IsUnderCharacterRidingSettings() {
            var menu = typeof(RidingSettings).GetCustomAttribute<CreateAssetMenuAttribute>();

            Assert.AreEqual("Spellbound/Character/Riding Settings", menu.menuName);
            Assert.AreEqual("RidingSettings", menu.fileName);
        }
    }
}
