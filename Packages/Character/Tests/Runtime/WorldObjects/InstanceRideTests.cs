// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using global::Spellbound.WorldObjects;
using NUnit.Framework;
using Spellbound.Character.Tests;
using Spellbound.Core;
using Spellbound.Core.Observations;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Core.Simulation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Spellbound.Character.WorldObjects.Tests {
    public class InstanceRideTests {
        private const float Tolerance = 1e-4f;

        private static readonly Vector3 PlatformPosition = new(0f, 0f, 0f);
        private static readonly Pose SpawnPose = new(new Vector3(1f, 0.6f, -0.5f), Quaternion.Euler(0f, 30f, 0f));

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
        public IEnumerator Bind_WritesTheFrameLocalPose_WithTheSameWorldPose() {
            var world = CreateWorld(out var presetHash);
            var platform = CreatePlatform(world);
            var instance = world.Writer().Spawn(presetHash, ReferenceFramePose.InWorld(SpawnPose));

            yield return new WaitForFixedUpdate();

            var ride = CharacterWorldObjectsKit.Ride(world, instance);
            ride.Bind(platform.Source, Time.fixedTimeAsDouble);

            Assert.IsTrue(ride.IsBound);
            Assert.IsTrue(world.TryGetView(instance, out var view));
            Assert.AreEqual(platform.Source.Id, view.Pose.Frame);
            AssertSamePose(SpawnPose, world.Reader.ToWorld(view.Pose));
        }

        [UnityTest]
        public IEnumerator BoundInstance_FollowsTheFrameWhenItMoves() {
            var world = CreateWorld(out var presetHash);
            var platform = CreatePlatform(world);
            var instance = world.Writer().Spawn(presetHash, ReferenceFramePose.InWorld(SpawnPose));

            yield return new WaitForFixedUpdate();

            var ride = CharacterWorldObjectsKit.Ride(world, instance);
            ride.Bind(platform.Source, Time.fixedTimeAsDouble);
            var platformPose = platform.Source.WorldPose;
            platform.Source.Move(new Pose(platformPose.position + Vector3.right, platformPose.rotation));

            yield return new WaitForFixedUpdate();

            Assert.IsTrue(world.TryGetView(instance, out var view));
            var moved = world.Reader.ToWorld(view.Pose);
            AssertSamePose(new Pose(SpawnPose.position + Vector3.right, SpawnPose.rotation), moved);
        }

        [UnityTest]
        public IEnumerator Release_WritesTheCurrentWorldPoseInTheWorldFrame() {
            var world = CreateWorld(out var presetHash);
            var platform = CreatePlatform(world);
            var instance = world.Writer().Spawn(presetHash, ReferenceFramePose.InWorld(SpawnPose));

            yield return new WaitForFixedUpdate();

            var ride = CharacterWorldObjectsKit.Ride(world, instance);
            ride.Bind(platform.Source, Time.fixedTimeAsDouble);
            var platformPose = platform.Source.WorldPose;
            platform.Source.Move(new Pose(platformPose.position + Vector3.right, platformPose.rotation));

            yield return new WaitForFixedUpdate();

            Assert.IsTrue(world.TryGetView(instance, out var bound));
            var current = world.Reader.ToWorld(bound.Pose);
            ride.Release();

            Assert.IsFalse(ride.IsBound);
            Assert.IsTrue(world.TryGetView(instance, out var released));
            Assert.IsTrue(released.Pose.Frame.IsWorld);
            AssertSamePose(current, new Pose(released.Pose.Position, released.Pose.Rotation));
        }

        [UnityTest]
        public IEnumerator Bind_OnAnInstanceTheWorldDoesNotHold_Throws() {
            var world = CreateWorld(out _);
            var platform = CreatePlatform(world);

            yield return new WaitForFixedUpdate();

            var ride = CharacterWorldObjectsKit.Ride(world, new InstanceId(987654321UL));

            Assert.Throws<ArgumentException>(() => ride.Bind(platform.Source, Time.fixedTimeAsDouble));
            Assert.IsFalse(ride.IsBound);
        }

        [Test]
        public void Ride_WithoutAWorld_Throws() {
            Assert.Throws<ArgumentNullException>(() => CharacterWorldObjectsKit.Ride(null, new InstanceId(1UL)));
        }

        private WorldInstance CreateWorld(out uint presetHash) {
            var preset = ScriptableObject.CreateInstance<ObjectPreset>();
            preset.name = "crate";
            _objects.Add(preset);
            var presets = WorldObjectsKit.CreatePresets(new[] { preset });
            presetHash = presets.All[0].Hash;

            var settings = ScriptableObject.CreateInstance<WorldSettings>();
            settings.sceneMode = WorldSceneMode.OwnScene;
            settings.presets.Add(preset);
            _objects.Add(settings);

            var context = CoreKit.Observations.CreateContext("ride_" + Guid.NewGuid().ToString("N"));
            _contexts.Add(context);
            var world = WorldObjectsKit.CreateWorld(settings,
                new WorldServices { PhysicsStep = new CountingPhysicsStepPolicy() }, context);
            _worlds.Add(world);
            return world;
        }

        private MovingPlatformFixture CreatePlatform(WorldInstance world) {
            var root = new GameObject("Reference frame world");
            _objects.Add(root);
            root.AddComponent<WorldObjectsReferenceFrameWorld>().World = world;
            SceneManager.MoveGameObjectToScene(root, world.Scene);
            var platform = MovingPlatformFixture.Create(root.transform, PlatformPosition, Vector3.zero);
            platform.enabled = false;
            return platform;
        }

        private static void AssertSamePose(Pose expected, Pose actual) {
            Assert.AreEqual(0f, Vector3.Distance(expected.position, actual.position), Tolerance,
                "position " + actual.position.ToString("F5") + " against " + expected.position.ToString("F5"));
            Assert.AreEqual(0f, Quaternion.Angle(expected.rotation, actual.rotation), 0.01f, "rotation");
        }
    }
}
