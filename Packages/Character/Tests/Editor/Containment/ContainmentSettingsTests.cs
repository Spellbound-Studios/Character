// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core.Authoring;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class ContainmentSettingsTests {
        private ContainmentSettings _settings;

        [SetUp]
        public void CreateSettings() {
            _settings = ScriptableObject.CreateInstance<ContainmentSettings>();
        }

        [TearDown]
        public void DestroySettings() {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void Defaults_AreTheFloaterValues() {
            Assert.IsTrue(_settings.hasFloor);
            Assert.AreEqual(-100f, _settings.floorHeight);
            Assert.AreEqual(5, _settings.embeddedChecksBeforeRescue);
            Assert.AreEqual(0.5f, _settings.openSearchStepMetres);
            Assert.AreEqual(20f, _settings.maxOpenSearchMetres);
            Assert.AreEqual(25f, _settings.sustainedFallSpeed);
            Assert.AreEqual(8f, _settings.sustainedFallSeconds);
            Assert.AreEqual(Vector3.zero, _settings.fallbackAnchor);
            Assert.AreEqual(5, _settings.checkStride);
            Assert.AreEqual(1000, _settings.rescuePriority);
            Assert.AreEqual(10f, _settings.supportHoldTimeoutSeconds);
            Assert.AreEqual(~0, _settings.supportLayers.value);
        }

        [Test]
        public void Menu_IsUnderCharacterContainmentSettings() {
            var menu = typeof(ContainmentSettings).GetCustomAttribute<CreateAssetMenuAttribute>();

            Assert.AreEqual("Spellbound/Character/Containment Settings", menu.menuName);
            Assert.AreEqual("ContainmentSettings", menu.fileName);
        }

        [Test]
        public void Settings_IsAnAuthoredDefinition() {
            Assert.IsInstanceOf<IAuthoredDefinition>(_settings);
        }

        [Test]
        public void Validate_ReportsNothingOnANewAsset() {
            var report = new ValidationReport("containment settings");

            _settings.Validate(report);

            Assert.IsTrue(report.IsEmpty);
        }

        [Test]
        public void Validate_RefusesAMissingReport() {
            Assert.Throws<ArgumentNullException>(() => _settings.Validate(null));
        }

        [Test]
        public void Validate_ReportsAStrideUnderOne() {
            _settings.checkStride = 0;

            AssertOnlyError("character.containment-stride-under-one");
        }

        [Test]
        public void Validate_ReportsASearchStepThatIsNotPositive() {
            _settings.openSearchStepMetres = 0f;

            AssertOnlyError("character.containment-search-step-not-positive");
        }

        [Test]
        public void Validate_ReportsASearchRangeThatIsNotPositive() {
            _settings.maxOpenSearchMetres = -1f;

            AssertOnlyError("character.containment-search-range-not-positive");
        }

        [Test]
        public void Validate_ReportsEmbeddedChecksUnderOne() {
            _settings.embeddedChecksBeforeRescue = 0;

            AssertOnlyError("character.containment-embedded-checks-under-one");
        }

        [Test]
        public void Validate_ReportsANegativeFallSpeed() {
            _settings.sustainedFallSpeed = -1f;

            AssertOnlyError("character.containment-fall-limits-negative");
        }

        [Test]
        public void Validate_ReportsNegativeFallSeconds() {
            _settings.sustainedFallSeconds = -1f;

            AssertOnlyError("character.containment-fall-limits-negative");
        }

        [Test]
        public void Validate_ReportsANegativeHoldTimeout() {
            _settings.supportHoldTimeoutSeconds = -1f;

            AssertOnlyError("character.containment-hold-timeout-negative");
        }

        [Test]
        public void Validate_ReportsARescuePriorityUnderOne() {
            _settings.rescuePriority = 0;

            AssertOnlyError("character.containment-rescue-priority-under-one");
        }

        [Test]
        public void Validate_ReportsANotANumberSearchStep() {
            _settings.openSearchStepMetres = float.NaN;

            AssertOnlyError("character.containment-search-step-not-positive");
        }

        private void AssertOnlyError(string code) {
            var report = new ValidationReport("containment settings");

            _settings.Validate(report);

            Assert.AreEqual(1, report.Findings.Count, report.ToText());
            Assert.AreEqual(code, report.Findings.Single().Code);
            Assert.IsTrue(report.HasErrors);
        }
    }
}
