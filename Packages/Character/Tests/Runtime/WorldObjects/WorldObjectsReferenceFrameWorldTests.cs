// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using global::Spellbound.WorldObjects;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core.Observations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Spellbound.Character.WorldObjects.Tests {
    public class WorldObjectsReferenceFrameWorldTests {
        private const int FixedSteps = 10;
        private const int TickSteps = 5;

        private readonly List<WorldInstance> _worlds = new();
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

            _objects.Clear();
            _worlds.Clear();
        }

        [UnityTest]
        public IEnumerator Source_UnderTheAdapter_RegistersInTheWorldsDirectory() {
            var world = CreateWorld(new CountingPhysicsStepPolicy());
            var adapter = CreateAdapter(world);
            var platform = new GameObject("Platform");
            platform.transform.SetParent(adapter.transform, false);
            var source = platform.AddComponent<TransformReferenceFrameSource>();

            yield return null;

            Assert.IsTrue(source.IsRegistered);
            Assert.IsTrue(world.ReferenceFrames.TryGet(source.Id, out var frame));
            Assert.AreSame(source, frame);
            Assert.AreSame(world.ReferenceFrames, adapter.ReferenceFrames);
            Assert.AreSame(world.Observations, adapter.Observations);
            Assert.AreSame(world, adapter.World);
        }

        [UnityTest]
        public IEnumerator CurrentTick_AfterFiveFixedSteps_EqualsTheWorldClock() {
            var world = CreateWorld(new CountingPhysicsStepPolicy());
            var adapter = CreateAdapter(world);

            for (var step = 0; step < TickSteps; step++) {
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(world.Clock.Current, adapter.CurrentTick);
        }

        [UnityTest]
        public IEnumerator Adapter_WithoutAWorld_ThrowsFromEveryMember() {
            var adapter = CreateAdapter(null);

            yield return null;

            var frames = Assert.Throws<InvalidOperationException>(() => _ = adapter.ReferenceFrames);
            StringAssert.Contains(nameof(WorldObjectsReferenceFrameWorld.ReferenceFrames), frames.Message);
            var observations = Assert.Throws<InvalidOperationException>(() => _ = adapter.Observations);
            StringAssert.Contains(nameof(WorldObjectsReferenceFrameWorld.Observations), observations.Message);
            var tick = Assert.Throws<InvalidOperationException>(() => _ = adapter.CurrentTick);
            StringAssert.Contains(nameof(WorldObjectsReferenceFrameWorld.CurrentTick), tick.Message);
        }

        [UnityTest]
        public IEnumerator World_SetAfterASourceReadIt_Throws() {
            var world = CreateWorld(new CountingPhysicsStepPolicy());
            var adapter = CreateAdapter(world);
            var platform = new GameObject("Platform");
            platform.transform.SetParent(adapter.transform, false);
            platform.AddComponent<TransformReferenceFrameSource>();

            yield return null;

            Assert.Throws<InvalidOperationException>(() => adapter.World = null);
            Assert.AreSame(world, adapter.World);
        }

        [UnityTest]
        public IEnumerator CountingPolicy_OverTenFixedSteps_IsAskedOncePerStepForItsOwnScene() {
            var policy = new CountingPhysicsStepPolicy();
            var asked = 0;
            policy.Asked = _ => asked++;
            CreateWorld(policy);

            yield return new WaitForFixedUpdate();

            var countBefore = policy.Count;
            var askedBefore = asked;

            for (var step = 0; step < FixedSteps; step++) {
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(FixedSteps, policy.Count - countBefore);
            Assert.AreEqual(FixedSteps, asked - askedBefore);
            Assert.AreEqual(policy.Count, policy.Requests.Count);
            foreach (var request in policy.Requests) {
                Assert.IsFalse(request.IsDefaultScene);
            }
        }

        [UnityTest]
        public IEnumerator CountingPolicy_AnswersTrueByDefaultAndThroughItsRule() {
            var policy = new CountingPhysicsStepPolicy();
            var request = new PhysicsStepRequest(default, true);

            yield return null;

            Assert.IsTrue(policy.ShouldStep(in request));
            policy.Rule = asked => !asked.IsDefaultScene;
            Assert.IsFalse(policy.ShouldStep(in request));
            Assert.AreEqual(2, policy.Count);
            Assert.IsTrue(policy.Requests[1].IsDefaultScene);
        }

        private WorldInstance CreateWorld(CountingPhysicsStepPolicy policy) {
            var settings = ScriptableObject.CreateInstance<WorldSettings>();
            settings.sceneMode = WorldSceneMode.OwnScene;
            settings.worldName = "reference frame world " + _worlds.Count;
            _objects.Add(settings);
            var world = WorldObjectsKit.CreateWorld(settings, new WorldServices { PhysicsStep = policy },
                ObservationContext.Discard);
            _worlds.Add(world);
            return world;
        }

        private WorldObjectsReferenceFrameWorld CreateAdapter(WorldInstance world) {
            var host = new GameObject("World adapter");
            host.SetActive(false);
            _objects.Add(host);
            var adapter = host.AddComponent<WorldObjectsReferenceFrameWorld>();
            if (world != null) {
                adapter.World = world;
                SceneManager.MoveGameObjectToScene(host, world.Scene);
            }

            host.SetActive(true);
            return adapter;
        }
    }
}
