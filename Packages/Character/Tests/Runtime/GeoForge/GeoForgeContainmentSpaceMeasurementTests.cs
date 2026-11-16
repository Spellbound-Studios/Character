// Copyright (c) Spellbound Studios. All rights reserved.

using System.Diagnostics;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Character.GeoForge.Tests {
    [Category("Measurement")]
    public class GeoForgeContainmentSpaceMeasurementTests {
        private const int BodyCount = 5000;
        private const int CornersPerBody = 8;
        private const int CornerQueryCount = BodyCount * CornersPerBody;
        private const int WarmPasses = 3;
        private const int MeasuredPasses = 5;

        [Test]
        public void FiveThousandBodies_CostOfTheirCornerQueries() {
            var settings = ScriptableObject.CreateInstance<ContainmentSettings>();
            var checkStride = settings.checkStride;
            var volume = new TestGeoVolume { SurfaceHeight = 0f };
            var space = new GeoForgeContainmentSpace(volume);
            var bodies = new Bounds[BodyCount];

            for (var body = 0; body < BodyCount; body++) {
                var centre = new Vector3(body % 100, (body % 7) - 3f, body / 100);
                bodies[body] = new Bounds(centre, new Vector3(0.6f, 1.8f, 0.6f));
            }

            for (var pass = 0; pass < WarmPasses; pass++)
                ClassifyEveryBody(space, bodies);

            var queriesBefore = volume.SolidQueryCount;
            var fastestTicks = long.MaxValue;

            for (var pass = 0; pass < MeasuredPasses; pass++) {
                var stopwatch = Stopwatch.StartNew();
                ClassifyEveryBody(space, bodies);
                stopwatch.Stop();

                if (stopwatch.ElapsedTicks < fastestTicks)
                    fastestTicks = stopwatch.ElapsedTicks;
            }

            Assert.AreEqual(CornerQueryCount * MeasuredPasses, volume.SolidQueryCount - queriesBefore);

            var totalMilliseconds = fastestTicks * 1000.0 / Stopwatch.Frequency;
            var perQueryNanoseconds = totalMilliseconds * 1_000_000.0 / CornerQueryCount;
            var spreadStepMilliseconds = totalMilliseconds / checkStride;
            var figure = "GeoForgeContainmentSpaceMeasurement: " + CornerQueryCount + " corner queries over "
                         + nameof(TestGeoVolume) + "; TOTAL_MS=" + totalMilliseconds.ToString("F3")
                         + "; PER_QUERY_NS=" + perQueryNanoseconds.ToString("F1")
                         + "; SPREAD_OVER_STRIDE_" + checkStride + "_MS=" + spreadStepMilliseconds.ToString("F3");

            Object.DestroyImmediate(settings);
            Log.Emit(LogLevel.Info, figure);
            Assert.Pass(figure);
        }

        private static void ClassifyEveryBody(GeoForgeContainmentSpace space, Bounds[] bodies) {
            for (var body = 0; body < bodies.Length; body++)
                space.Classify(bodies[body]);
        }
    }
}
