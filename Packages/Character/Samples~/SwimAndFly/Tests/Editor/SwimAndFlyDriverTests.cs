// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Character.Samples.SwimAndFly.Tests;
using UnityEngine;

namespace Spellbound.Character.Samples.SwimAndFly.Editor.Tests {
    public class SwimAndFlyDriverTests {
        private const float StepSeconds = 0.02f;
        private const int SettleSteps = 400;
        private const float Tolerance = 0.001f;
        private const float HeightTolerance = 0.01f;

        private GameObject _gameObject;
        private Rigidbody _body;
        private ScriptedMoveSource _source;

        [SetUp]
        public void CreateBody() {
            _gameObject = new GameObject("Swim and fly body");
            _body = _gameObject.AddComponent<Rigidbody>();
            _body.useGravity = false;
            _source = new ScriptedMoveSource { DesiredUp = new Vector3(0f, 1f, 0f) };
        }

        [TearDown]
        public void DestroyBody() => Object.DestroyImmediate(_gameObject);

        [Test]
        public void FlyClaim_TakesOverFromSwim_WithTheSwimmingVelocity() {
            var swim = new SwimDriver();
            var fly = new FlyDriver();
            var claims = new MotionClaims();
            claims.Claim(swim);
            _source.DesiredMove = new Vector3(1f, 0f, 0f);

            swim.Move(_body, _source, StepSeconds);
            var swimming = _body.linearVelocity;
            claims.Claim(fly);

            Assert.AreSame(fly, claims.Active);
            Assert.Greater(swimming.magnitude, 0f);
            Assert.Less(Vector3.Distance(swimming, fly.Entered.WorldVelocity), Tolerance);
        }

        [Test]
        public void FlyReleased_SwimResumes_WithTheFlyingVelocity() {
            var swim = new SwimDriver();
            var fly = new FlyDriver();
            var claims = new MotionClaims();
            claims.Claim(swim);
            var flyClaim = claims.Claim(fly);
            _source.DesiredMove = new Vector3(0f, 1f, 1f);

            fly.Move(_body, _source, StepSeconds);
            var flying = _body.linearVelocity;
            flyClaim.Release();

            Assert.AreSame(swim, claims.Active);
            Assert.Greater(flying.magnitude, 0f);
            Assert.Less(Vector3.Distance(flying, swim.Entered.WorldVelocity), Tolerance);
        }

        [Test]
        public void SwimDriver_SettlesToTheWaterHeight() {
            var swim = new SwimDriver { WaterHeight = 2f };
            _body.position = new Vector3(0f, -3f, 0f);

            for (var step = 0; step < SettleSteps; step++) {
                swim.Move(_body, _source, StepSeconds);
                _body.position += _body.linearVelocity * StepSeconds;
            }

            Assert.AreEqual(2f, _body.position.y, HeightTolerance);
        }
    }
}
