// Copyright (c) Spellbound Studios. All rights reserved.

using System.Reflection;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Character.Tests;
using Spellbound.Core.Observations;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Core.Simulation;
using UnityEditor;
using UnityEngine;

namespace Spellbound.Character.Editor.Tests {
    public class ReferenceFrameWorldSeamTests {
        private GameObject _gameObject;

        [SetUp]
        public void CreateGameObject() {
            _gameObject = new GameObject("reference frame seam");
        }

        [TearDown]
        public void DestroyGameObject() {
            Object.DestroyImmediate(_gameObject);
        }

        [Test]
        public void IReferenceFrameWorld_CustomImplementation() {
            var world = _gameObject.AddComponent<TestReferenceFrameWorld>();
            world.CurrentTick = new Tick(12);
            IReferenceFrameWorld seam = world;

            Assert.IsNotNull(seam.ReferenceFrames);
            Assert.AreEqual(0, seam.ReferenceFrames.Count);
            Assert.AreSame(ObservationContext.Discard, seam.Observations);
            Assert.AreEqual(new Tick(12), seam.CurrentTick);
        }

        [Test]
        public void Source_BeforeEnable_ReadsTheWorldIdAndIsNotRegistered() {
            var source = _gameObject.AddComponent<TestReferenceFrameSource>();

            Assert.AreEqual(ReferenceFrameId.World, source.Id);
            Assert.AreEqual(0ul, source.Id.Value);
            Assert.IsFalse(source.IsRegistered);
            Assert.IsNull(source.World);
        }

        [Test]
        public void ReferenceFrameSource_CustomImplementation() {
            _gameObject.transform.SetPositionAndRotation(new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 90f, 0f));
            var source = _gameObject.AddComponent<TestReferenceFrameSource>();
            source.LinearVelocity = new Vector3(0f, 0f, 4f);
            IReferenceFrame frame = source;

            Assert.AreEqual(new Vector3(1f, 2f, 3f), frame.WorldPose.position);
            Assert.AreEqual(Vector3.zero, frame.AngularVelocity);
            Assert.AreEqual(new Vector3(0f, 0f, 4f), frame.VelocityAt(Vector3.zero));
        }

        [Test]
        public void Source_TakesItsWorldInCode() {
            var world = _gameObject.AddComponent<TestReferenceFrameWorld>();
            var source = _gameObject.AddComponent<TestReferenceFrameSource>();

            source.World = world;

            Assert.AreSame(world, source.World);
        }

        [Test]
        public void Source_SerializesItsIdAndItsWorldComponent() {
            var serialized = new SerializedObject(_gameObject.AddComponent<TestReferenceFrameSource>());

            Assert.AreEqual(SerializedPropertyType.Integer, serialized.FindProperty("referenceFrameId").propertyType);
            Assert.AreEqual(SerializedPropertyType.ObjectReference, serialized.FindProperty("world").propertyType);
        }

        [Test]
        public void Source_RunsBeforeTheRider() {
            Assert.AreEqual(-100, typeof(ReferenceFrameSource).GetCustomAttribute<DefaultExecutionOrder>().order);
            Assert.AreEqual(-50, typeof(ReferenceFrameRider).GetCustomAttribute<DefaultExecutionOrder>().order);
        }

        [Test]
        public void Rider_RequiresARigidbody() {
            _gameObject.AddComponent<ReferenceFrameRider>();

            Assert.IsNotNull(_gameObject.GetComponent<Rigidbody>());
        }

        [Test]
        public void Rider_DefaultsAreTheProbeNumbers() {
            var serialized = new SerializedObject(_gameObject.AddComponent<ReferenceFrameRider>());

            Assert.AreEqual(1, serialized.FindProperty("probeStride").intValue);
            Assert.AreEqual(0.25f, serialized.FindProperty("probeLift").floatValue);
            Assert.AreEqual(1f, serialized.FindProperty("probeMetres").floatValue);
            Assert.AreEqual(~0, serialized.FindProperty("probeLayers").intValue);
            Assert.IsTrue(serialized.FindProperty("skipSleeping").boolValue);
            Assert.IsTrue(serialized.FindProperty("skipKinematic").boolValue);
            Assert.IsNull(serialized.FindProperty("settings").objectReferenceValue);
            Assert.IsNull(serialized.FindProperty("world").objectReferenceValue);
        }

        [Test]
        public void Rider_HoldsAnUnboundBindingAndTakesItsWorldInCode() {
            var rider = _gameObject.AddComponent<ReferenceFrameRider>();
            var world = _gameObject.AddComponent<TestReferenceFrameWorld>();

            Assert.IsNotNull(rider.Binding);
            Assert.IsFalse(rider.Binding.IsBound);
            Assert.IsNull(rider.World);

            rider.World = world;

            Assert.AreSame(world, rider.World);
        }

        [Test]
        public void Binding_TurnAndWorldVelocityHaveTheirShape() {
            var turn = typeof(ReferenceFrameBinding).GetMethod(nameof(ReferenceFrameBinding.Turn),
                new[] { typeof(Quaternion), typeof(double) });
            var worldVelocity = typeof(ReferenceFrameBinding).GetMethod(nameof(ReferenceFrameBinding.WorldVelocity),
                new[] { typeof(Vector3), typeof(Vector3) });

            Assert.IsNotNull(turn);
            Assert.AreEqual(typeof(Quaternion), turn.ReturnType);
            Assert.AreEqual("bodyRotation", turn.GetParameters()[0].Name);
            Assert.AreEqual("stepSeconds", turn.GetParameters()[1].Name);
            Assert.IsNotNull(worldVelocity);
            Assert.AreEqual(typeof(Vector3), worldVelocity.ReturnType);
            Assert.AreEqual("relative", worldVelocity.GetParameters()[0].Name);
            Assert.AreEqual("point", worldVelocity.GetParameters()[1].Name);
        }
    }
}
