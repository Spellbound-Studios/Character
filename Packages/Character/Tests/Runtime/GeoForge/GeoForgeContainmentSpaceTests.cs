// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using NUnit.Framework;
using Spellbound.Core.Ground;
using Spellbound.GeoForge;
using UnityEngine;

namespace Spellbound.Character.GeoForge.Tests {
    public class GeoForgeContainmentSpaceTests {
        private static readonly Bounds BodyBounds = new(Vector3.zero, new Vector3(1f, 2f, 1f));

        private TestGeoVolume _volume;
        private GeoForgeContainmentSpace _space;

        [SetUp]
        public void CreateSpace() {
            _volume = new TestGeoVolume();
            _space = new GeoForgeContainmentSpace(_volume);
        }

        [Test]
        public void Classify_EveryCornerSolid_IsSolid() {
            _volume.SurfaceHeight = 10f;

            Assert.AreEqual(Occupancy.Solid, _space.Classify(BodyBounds));
        }

        [Test]
        public void Classify_NoCornerSolid_IsEmpty() {
            _volume.SurfaceHeight = -10f;

            Assert.AreEqual(Occupancy.Empty, _space.Classify(BodyBounds));
        }

        [Test]
        public void Classify_CornersOnBothSidesOfTheSurface_IsMixed() {
            _volume.SurfaceHeight = 0f;

            Assert.AreEqual(Occupancy.Mixed, _space.Classify(BodyBounds));
        }

        [Test]
        public void Classify_AnUnansweredCornerInTheAir_IsMixed() {
            _volume.SurfaceHeight = -10f;
            _volume.HasUnloadedRegion = true;
            _volume.UnloadedRegion = new Bounds(BodyBounds.max, Vector3.one * 0.1f);

            Assert.AreEqual(Occupancy.Mixed, _space.Classify(BodyBounds));
        }

        [Test]
        public void Classify_AnUnansweredCornerUnderground_IsMixed() {
            _volume.SurfaceHeight = 10f;
            _volume.HasUnloadedRegion = true;
            _volume.UnloadedRegion = new Bounds(BodyBounds.min, Vector3.one * 0.1f);

            Assert.AreEqual(Occupancy.Mixed, _space.Classify(BodyBounds));
        }

        [Test]
        public void Classify_QueriesTheEightCorners() {
            _volume.SurfaceHeight = -10f;

            _space.Classify(BodyBounds);

            Assert.AreEqual(8, _volume.SolidQueryCount);
        }

        [Test]
        public void IsReady_PassesTheVolumesReadinessThrough() {
            _volume.Ready = false;
            Assert.IsFalse(_space.IsReady(BodyBounds));

            _volume.Ready = true;
            Assert.IsTrue(_space.IsReady(BodyBounds));
        }

        [Test]
        public void IsReady_AsksForTheBodyBoundsAtAnyLevel() {
            _space.IsReady(BodyBounds);

            Assert.AreEqual(1, _volume.ReadinessQueryCount);
            Assert.AreEqual(BodyBounds, _volume.LastReadinessBounds);
            Assert.AreEqual(ReadinessScope.AnyLevel, _volume.LastReadinessScope);
        }

        [Test]
        public void Constructor_WithoutAVolume_Throws() {
            Assert.Throws<ArgumentNullException>(() => new GeoForgeContainmentSpace(null));
        }
    }
}
