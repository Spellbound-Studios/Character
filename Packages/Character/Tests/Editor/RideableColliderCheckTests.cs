// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core.Authoring;
using Spellbound.Core.Authoring.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class RideableColliderCheckTests {
        private const int RideableLayer = 8;
        private const int OtherLayer = 9;

        private RidingSettings _settings;

        [SetUp]
        public void CreateSceneAndSettings() {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            _settings = ScriptableObject.CreateInstance<RidingSettings>();
            _settings.rideableLayers = 1 << RideableLayer;
        }

        [TearDown]
        public void DestroySceneAndSettings() {
            Object.DestroyImmediate(_settings);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [Test]
        public void Check_ReportsNothingForAColliderUnderASource() {
            var platform = new GameObject("Platform");
            platform.AddComponent<TransformReferenceFrameSource>();
            CreateCollider(platform.transform, "Deck", RideableLayer);

            var report = RunCheck();

            Assert.IsTrue(report.IsEmpty);
        }

        [Test]
        public void Check_WarnsOnceNamingAColliderWithNoSourceAbove() {
            var platform = new GameObject("Platform");
            CreateCollider(platform.transform, "Deck", RideableLayer);

            var report = RunCheck();

            Assert.AreEqual(1, report.Findings.Count);
            var finding = report.Findings[0];
            Assert.AreEqual(RideableColliderCheck.Name, finding.Code);
            Assert.AreEqual(FindingSeverity.Warning, finding.Severity);
            Assert.AreEqual(nameof(RidingSettings.rideableLayers), finding.Location);
            StringAssert.Contains("Platform/Deck", finding.Message);
        }

        [Test]
        public void Check_IgnoresAColliderOffTheRideableLayers() {
            var platform = new GameObject("Platform");
            CreateCollider(platform.transform, "Deck", OtherLayer);

            var report = RunCheck();

            Assert.IsTrue(report.IsEmpty);
        }

        [Test]
        public void Check_IgnoresATrigger() {
            var platform = new GameObject("Platform");
            var deck = CreateCollider(platform.transform, "Deck", RideableLayer);
            deck.isTrigger = true;

            var report = RunCheck();

            Assert.IsTrue(report.IsEmpty);
        }

        [Test]
        public void PreviewLines_ListTheCollidersTheCheckWarnsFor() {
            var platform = new GameObject("Platform");
            CreateCollider(platform.transform, "Deck", RideableLayer);
            var carried = new GameObject("Carried");
            carried.AddComponent<TransformReferenceFrameSource>();
            CreateCollider(carried.transform, "Deck", RideableLayer);

            var lines = new RideableColliderCheck().PreviewLines(_settings);

            Assert.AreEqual(1, lines.Count);
            StringAssert.EndsWith("Platform/Deck", lines[0]);
        }

        [Test]
        public void Registry_FindsTheCheckByName() {
            var description = DefinitionCheckRegistry.Default.Find(RideableColliderCheck.Name);

            Assert.IsNotNull(description);
            Assert.AreEqual(typeof(RideableColliderCheck), description.CheckType);
        }

        [Test]
        public void Validate_WarnsWhenNoLayerIsRideable() {
            _settings.rideableLayers = 0;
            var report = new ValidationReport("riding settings");

            _settings.Validate(report);

            Assert.AreEqual(1, report.Findings.Count);
            Assert.AreEqual("character.rideable-layers-empty", report.Findings[0].Code);
            Assert.AreEqual(FindingSeverity.Warning, report.Findings[0].Severity);
            Assert.AreEqual(nameof(RidingSettings.rideableLayers), report.Findings[0].Location);
        }

        private ValidationReport RunCheck() {
            var report = new ValidationReport("riding settings");
            new RideableColliderCheck().Check(_settings, report);
            return report;
        }

        private static BoxCollider CreateCollider(Transform parent, string name, int layer) {
            var child = new GameObject(name) { layer = layer };
            child.transform.SetParent(parent, false);
            return child.AddComponent<BoxCollider>();
        }
    }
}
