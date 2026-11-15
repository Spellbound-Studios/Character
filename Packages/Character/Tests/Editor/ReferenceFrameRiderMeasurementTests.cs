// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Character.Tests;
using Spellbound.Core.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    [Category("Measurement")]
    public class ReferenceFrameRiderMeasurementTests {
        private const int Iterations = 100_000;
        private const double ProbeBudgetNanoseconds = 5000.0;
        private const double ObservationBudgetNanoseconds = 1000.0;
        private const double StepSeconds = 0.02;

        private GameObject _root;
        private ReferenceFrameRider _rider;

        [SetUp]
        public void CreateScene() {
            _root = new GameObject("reference frame rider measurement");
            var world = _root.AddComponent<TestReferenceFrameWorld>();

            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.transform.SetParent(_root.transform, false);
            platform.transform.localScale = new Vector3(4f, 0.2f, 4f);
            platform.AddComponent<Rigidbody>().isKinematic = true;
            var source = platform.AddComponent<TestReferenceFrameSource>();
            source.World = world;
            Invoke(typeof(ReferenceFrameSource), source, "OnEnable");

            var body = new GameObject("rider");
            body.transform.SetParent(_root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            body.AddComponent<Rigidbody>().isKinematic = true;
            _rider = body.AddComponent<ReferenceFrameRider>();
            _rider.World = world;
            Invoke(typeof(ReferenceFrameRider), _rider, "OnEnable");

            Physics.SyncTransforms();
        }

        [TearDown]
        public void DestroyScene() {
            Object.DestroyImmediate(_root);
        }

        [Test]
        public void ProbeAndObservation_AreUnderTheirBudgets() {
            _rider.ProbeOnce();
            Assert.IsTrue(_rider.Binding.IsBound, "The rider did not bind to the platform below it.");

            var probeNanoseconds = MeasureProbe();
            var observationNanoseconds = MeasureObservation(_rider.Binding);

            Log.Emit(LogLevel.Info, "ReferenceFrameRiderMeasurement: " + Iterations + " iterations"
                                    + "; PROBE_NS=" + probeNanoseconds.ToString("F1")
                                    + "; BINDING_OBSERVATION_NS=" + observationNanoseconds.ToString("F1"));

            Assert.Less(probeNanoseconds, ProbeBudgetNanoseconds, "The rider's probe is over its budget.");
            Assert.Less(observationNanoseconds, ObservationBudgetNanoseconds,
                "The binding's observation is over its budget.");
        }

        private double MeasureProbe() {
            for (var warm = 0; warm < 1000; warm++)
                _rider.ProbeOnce();

            var stopwatch = Stopwatch.StartNew();

            for (var iteration = 0; iteration < Iterations; iteration++)
                _rider.ProbeOnce();

            stopwatch.Stop();
            Assert.IsTrue(_rider.Binding.IsBound, "The rider lost the platform while it was measured.");

            return Nanoseconds(stopwatch);
        }

        private static double MeasureObservation(ReferenceFrameBinding binding) {
            var time = Time.fixedTimeAsDouble;

            for (var warm = 0; warm < 1000; warm++) {
                time += StepSeconds;
                binding.Sight(time);
                binding.Update(time);
            }

            var stopwatch = Stopwatch.StartNew();

            for (var iteration = 0; iteration < Iterations; iteration++) {
                time += StepSeconds;
                binding.Sight(time);
                binding.Update(time);
            }

            stopwatch.Stop();
            Assert.IsTrue(binding.IsBound, "The binding released while it was sighted every step.");

            return Nanoseconds(stopwatch);
        }

        private static double Nanoseconds(Stopwatch stopwatch) =>
                stopwatch.ElapsedTicks * 1_000_000_000.0 / Stopwatch.Frequency / Iterations;

        private static void Invoke(Type type, Object target, string message) {
            type.GetMethod(message, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
        }
    }
}
