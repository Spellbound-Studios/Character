// Copyright (c) Spellbound Studios. All rights reserved.

using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Spellbound.Character.Body;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Editor.Tests {
    public class ContainmentFilterTests {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        private ContainmentSettings _settings;
        private GameObject _bodyObject;
        private Rigidbody _body;
        private Containment _containment;

        [SetUp]
        public void CreateBody() {
            _settings = ScriptableObject.CreateInstance<ContainmentSettings>();
            _settings.floorHeight = 100f;
            _bodyObject = new GameObject("filtered body");
            _bodyObject.AddComponent<BoxCollider>();
            _body = _bodyObject.AddComponent<Rigidbody>();
            _body.useGravity = false;
            _containment = _bodyObject.AddComponent<Containment>();
            typeof(Containment).GetField("settings", Private).SetValue(_containment, _settings);
            typeof(Containment).GetField("skipSleeping", Private).SetValue(_containment, false);
            typeof(Containment).GetMethod("OnEnable", Private).Invoke(_containment, null);
        }

        [TearDown]
        public void DestroyBody() {
            Object.DestroyImmediate(_bodyObject);
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void IsSkipped_AnAwakeDynamicBody_IsChecked() {
            Assert.IsFalse(_containment.IsSkipped());
        }

        [Test]
        public void IsSkipped_AKinematicBody_IsSkipped() {
            _body.isKinematic = true;

            Assert.IsTrue(_containment.IsSkipped());
        }

        [Test]
        public void IsSkipped_AKinematicBodyWithKinematicSkipOff_IsChecked() {
            _body.isKinematic = true;
            typeof(Containment).GetField("skipKinematic", Private).SetValue(_containment, false);

            Assert.IsFalse(_containment.IsSkipped());
        }

        [Test]
        public void IsSkipped_ABodyHeldByItsSupportGate_IsSkipped() {
            typeof(Containment).GetField("skipKinematic", Private).SetValue(_containment, false);
            var gate = _bodyObject.AddComponent<SupportGate>();
            typeof(SupportGate).GetProperty(nameof(SupportGate.IsHeld)).SetValue(gate, true);
            typeof(Containment).GetMethod("OnEnable", Private).Invoke(_containment, null);

            Assert.IsTrue(_containment.IsSkipped());
        }

        [Test]
        public void Check_OnASkippedBody_NoAllocation() {
            _body.isKinematic = true;
            TestDelegate action = () => _containment.Check();

            for (var warm = 0; warm < 3; warm++)
                action();

            var constraint = ((IResolveConstraint)Is.Not.AllocatingGCMemory()).Resolve();
            constraint.ApplyTo(action);
            Assert.That(action, constraint);
            Assert.AreEqual(0, _containment.LastRescues.Count);
        }
    }
}
