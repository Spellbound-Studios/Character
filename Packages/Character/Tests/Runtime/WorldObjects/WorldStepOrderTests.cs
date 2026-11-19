// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using global::Spellbound.WorldObjects;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Character.Tests;
using Spellbound.Core;
using Spellbound.Core.Observations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Spellbound.Character.WorldObjects.Tests {
    public class WorldStepOrderTests {
        private const int OrderSteps = 30;
        private const int BoundByStep = 10;
        private const int StepsBeforeTheHitch = 5;
        private const int HitchFixedSteps = 6;
        private const int StepsAfterTheHitch = 5;
        private const int FallingSteps = 25;
        private const int DefaultSceneSteps = 25;
        private const float BodyHeight = 0.5f;
        private const float RiderGap = 0.05f;
        private const float FallingHeight = 10f;
        private const float FallenMetres = 0.5f;
        private const float StillTolerance = 1e-5f;

        private const string StartEntry = "start";
        private const string ScriptsEntry = "scripts";
        private const string SimulateEntry = "simulate";
        private const string SteppedEntry = "stepped";

        private static readonly Vector3 PlatformVelocity = new(1f, 0f, 0f);

        private readonly List<WorldInstance> _worlds = new();
        private readonly List<ObservationContext> _contexts = new();
        private readonly List<UnityEngine.Object> _objects = new();

        [TearDown]
        public void DisposeWorlds() {
            foreach (var created in _objects) {
                if (created != null) {
                    UnityEngine.Object.Destroy(created);
                }
            }

            foreach (var world in _worlds) {
                world.Dispose();
            }

            foreach (var context in _contexts) {
                context.Dispose();
            }

            _objects.Clear();
            _worlds.Clear();
            _contexts.Clear();
        }

        [UnityTest]
        public IEnumerator EveryFixedStep_ReadsClockThenScriptsThenOneSimulate_AndOneStepped() {
            var policy = new CountingPhysicsStepPolicy();
            var world = CreateWorld(WorldSceneMode.OwnScene, policy);
            var rider = CreatePlatformAndRider(world);
            var sequence = Record(world, policy, rider);

            yield return new WaitForFixedUpdate();

            sequence.Clear();
            var boundAtStep = 0;

            for (var step = 1; step <= OrderSteps; step++) {
                yield return new WaitForFixedUpdate();

                if (boundAtStep == 0 && rider.Binding.IsBound) {
                    boundAtStep = step;
                }
            }

            var steps = SplitByStep(sequence);
            Assert.AreEqual(OrderSteps, steps.Count, string.Join("\n", sequence));
            Assert.AreEqual(Count(sequence, StartEntry), Count(sequence, SimulateEntry));

            var steppedBeforeSimulate = 0;

            for (var index = 0; index < steps.Count; index++) {
                var entries = steps[index];
                var context = "step " + index + ": " + string.Join(" | ", entries);
                Assert.AreEqual(1, Count(entries, ScriptsEntry), context);
                Assert.AreEqual(1, Count(entries, SimulateEntry), context);
                Assert.AreEqual(1, Count(entries, SteppedEntry), context);

                var scripts = IndexOf(entries, ScriptsEntry);
                var simulate = IndexOf(entries, SimulateEntry);
                var stepped = IndexOf(entries, SteppedEntry);
                Assert.Less(0, scripts, context);
                Assert.Less(scripts, simulate, context);
                Assert.Less(0, stepped, context);
                Assert.AreEqual(Time.fixedDeltaTime, ReadStep(entries[simulate]), context);
                Assert.AreEqual(ReadTick(entries[0]), ReadTick(entries[scripts]), context);
                Assert.AreEqual(ReadTick(entries[0]), ReadTick(entries[stepped]), context);

                if (stepped < simulate) {
                    steppedBeforeSimulate++;
                }
            }

            Assert.That(boundAtStep, Is.InRange(1, BoundByStep), "the rider was not bound by step " + BoundByStep);
            Assert.Pass("stepped before simulate in " + steppedBeforeSimulate + " of " + steps.Count +
                    " fixed steps; rider bound at step " + boundAtStep);
        }

        [UnityTest]
        public IEnumerator Hitch_RunsOneSimulatePerFixedStepUnityRan_AndNoExtraStep() {
            var policy = new CountingPhysicsStepPolicy();
            var world = CreateWorld(WorldSceneMode.OwnScene, policy);
            var rider = CreatePlatformAndRider(world);
            var sequence = Record(world, policy, rider);

            yield return new WaitForFixedUpdate();

            sequence.Clear();

            for (var step = 0; step < StepsBeforeTheHitch; step++) {
                yield return new WaitForFixedUpdate();
            }

            yield return null;

            var startsBeforeTheHitch = Count(sequence, StartEntry);
            System.Threading.Thread.Sleep((int)(Time.fixedDeltaTime * 1000 * HitchFixedSteps));

            yield return null;
            yield return null;

            var stepsInTheHitch = Count(sequence, StartEntry) - startsBeforeTheHitch;

            for (var step = 0; step < StepsAfterTheHitch; step++) {
                yield return new WaitForFixedUpdate();
            }

            var starts = 0;
            var simulates = 0;

            foreach (var entry in sequence) {
                if (IsEntry(entry, StartEntry)) {
                    starts++;
                }
                else if (IsEntry(entry, SimulateEntry)) {
                    simulates++;
                }

                Assert.LessOrEqual(simulates, starts, string.Join("\n", sequence));
            }

            Assert.GreaterOrEqual(stepsInTheHitch, HitchFixedSteps - 2, string.Join("\n", sequence));
            Assert.AreEqual(starts, simulates);
            Assert.Pass("fixed steps across the hitch frames " + stepsInTheHitch + "; starts " + starts +
                    ", simulates " + simulates);
        }

        [UnityTest]
        public IEnumerator RefusingPolicy_LeavesItsSceneStill_WhileTheSteppedSceneFalls() {
            var stepped = new CountingPhysicsStepPolicy();
            var refused = new CountingPhysicsStepPolicy { Rule = request => false };
            var steppedWorld = CreateWorld(WorldSceneMode.OwnScene, stepped);
            var refusedWorld = CreateWorld(WorldSceneMode.OwnScene, refused);
            var steppedBody = CreateFallingBody(steppedWorld);
            var refusedBody = CreateFallingBody(refusedWorld);

            yield return new WaitForFixedUpdate();

            var steppedHeight = steppedBody.position.y;
            var refusedHeight = refusedBody.position.y;
            var refusedBefore = refused.Count;

            for (var step = 0; step < FallingSteps; step++) {
                yield return new WaitForFixedUpdate();
            }

            Assert.Greater(steppedHeight - steppedBody.position.y, FallenMetres);
            Assert.AreEqual(refusedHeight, refusedBody.position.y, StillTolerance);
            Assert.AreEqual(FallingSteps, refused.Count - refusedBefore);

            foreach (var request in refused.Requests) {
                Assert.IsFalse(request.IsDefaultScene);
            }
        }

        [UnityTest]
        public IEnumerator DefaultSceneWorld_UnderTheAutomaticRule_StepsNothing_WhileTheClockAdvances() {
            var automatic = new AutomaticPhysicsStepPolicy();
            var policy = new CountingPhysicsStepPolicy { Rule = request => automatic.ShouldStep(request) };
            var answers = new List<bool>();
            policy.Asked = request => answers.Add(policy.Rule(request));
            var world = CreateWorld(WorldSceneMode.DefaultScene, policy, out var settings);
            var steppedCount = 0;
            world.Clock.Stepped += _ => steppedCount++;

            yield return new WaitForFixedUpdate();

            var tickBefore = world.Clock.Current.Value;
            var steppedBefore = steppedCount;
            var requestsBefore = policy.Count;

            for (var step = 0; step < DefaultSceneSteps; step++) {
                yield return new WaitForFixedUpdate();
            }

            var expectedTicks = Mathf.RoundToInt(DefaultSceneSteps * Time.fixedDeltaTime * settings.ticksPerSecond);
            Assert.GreaterOrEqual(policy.Count - requestsBefore, DefaultSceneSteps);
            Assert.AreEqual(policy.Count, answers.Count);

            foreach (var request in policy.Requests) {
                Assert.IsTrue(request.IsDefaultScene);
            }

            CollectionAssert.DoesNotContain(answers, true);
            Assert.GreaterOrEqual((long)(world.Clock.Current.Value - tickBefore), expectedTicks);
            Assert.GreaterOrEqual(steppedCount - steppedBefore, DefaultSceneSteps);
        }

        private WorldInstance CreateWorld(WorldSceneMode sceneMode, CountingPhysicsStepPolicy policy) =>
                CreateWorld(sceneMode, policy, out _);

        private WorldInstance CreateWorld(WorldSceneMode sceneMode, CountingPhysicsStepPolicy policy,
                out WorldSettings settings) {
            settings = ScriptableObject.CreateInstance<WorldSettings>();
            settings.sceneMode = sceneMode;
            settings.worldName = "world step order " + _worlds.Count;
            _objects.Add(settings);
            var context = CoreKit.Observations.CreateContext("world_step_order_" + Guid.NewGuid().ToString("N"));
            _contexts.Add(context);
            var world = WorldObjectsKit.CreateWorld(settings, new WorldServices { PhysicsStep = policy }, context);
            _worlds.Add(world);
            return world;
        }

        private ReferenceFrameRider CreatePlatformAndRider(WorldInstance world) {
            var root = new GameObject("World step order frames");
            root.SetActive(false);
            _objects.Add(root);
            root.AddComponent<WorldObjectsReferenceFrameWorld>().World = world;
            SceneManager.MoveGameObjectToScene(root, world.Scene);

            var platform = MovingPlatformFixture.Create(root.transform, Vector3.zero, PlatformVelocity);
            var rider = new GameObject("Rider");
            rider.transform.SetParent(root.transform, false);
            rider.transform.position = new Vector3(0f, platform.TopHeight + BodyHeight * 0.5f + RiderGap, 0f);
            rider.AddComponent<BoxCollider>().size = Vector3.one * BodyHeight;
            rider.AddComponent<Rigidbody>();
            var riding = rider.AddComponent<ReferenceFrameRider>();

            root.SetActive(true);
            return riding;
        }

        private List<string> Record(WorldInstance world, CountingPhysicsStepPolicy policy, ReferenceFrameRider rider) {
            var sequence = new List<string>();
            var host = new GameObject("World step order recorder");
            host.SetActive(false);
            _objects.Add(host);
            SceneManager.MoveGameObjectToScene(host, world.Scene);

            var start = host.AddComponent<StepStartRecorder>();
            start.Sequence = sequence;
            start.Clock = world.Clock;
            var scripts = host.AddComponent<StepSequenceRecorder>();
            scripts.Sequence = sequence;
            scripts.Clock = world.Clock;
            scripts.Rider = rider;

            policy.Asked = request =>
                    sequence.Add(SimulateEntry + " step=" +
                        Time.fixedDeltaTime.ToString("R", CultureInfo.InvariantCulture));
            world.Clock.Stepped += tick => sequence.Add(SteppedEntry + " tick=" + tick.Value);

            host.SetActive(true);
            return sequence;
        }

        private Rigidbody CreateFallingBody(WorldInstance world) {
            var falling = new GameObject("Falling body");
            _objects.Add(falling);
            SceneManager.MoveGameObjectToScene(falling, world.Scene);
            falling.transform.position = new Vector3(0f, FallingHeight, 0f);
            falling.AddComponent<SphereCollider>();
            var body = falling.AddComponent<Rigidbody>();
            body.linearDamping = 0f;
            body.interpolation = RigidbodyInterpolation.None;
            return body;
        }

        private static List<List<string>> SplitByStep(List<string> sequence) {
            var steps = new List<List<string>>();

            foreach (var entry in sequence) {
                if (IsEntry(entry, StartEntry)) {
                    steps.Add(new List<string>());
                }

                Assert.IsNotEmpty(steps, "an entry came before the first start: " + entry);
                steps[^1].Add(entry);
            }

            return steps;
        }

        private static bool IsEntry(string entry, string kind) => entry.StartsWith(kind + " ");

        private static int Count(List<string> entries, string kind) {
            var count = 0;

            foreach (var entry in entries) {
                if (IsEntry(entry, kind)) {
                    count++;
                }
            }

            return count;
        }

        private static int IndexOf(List<string> entries, string kind) {
            for (var index = 0; index < entries.Count; index++) {
                if (IsEntry(entries[index], kind)) {
                    return index;
                }
            }

            return -1;
        }

        private static string ReadField(string entry, string field) {
            foreach (var part in entry.Split(' ')) {
                if (part.StartsWith(field + "=")) {
                    return part.Substring(field.Length + 1);
                }
            }

            Assert.Fail("no " + field + " in " + entry);
            return null;
        }

        private static ulong ReadTick(string entry) =>
                ulong.Parse(ReadField(entry, "tick"), CultureInfo.InvariantCulture);

        private static float ReadStep(string entry) =>
                float.Parse(ReadField(entry, "step"), CultureInfo.InvariantCulture);
    }
}
