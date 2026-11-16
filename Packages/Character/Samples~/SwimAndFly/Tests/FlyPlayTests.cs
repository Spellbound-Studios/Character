// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Character.Samples.SwimAndFly.Tests {
    public class FlyPlayTests {
        private const int FixedSteps = 25;
        private const float MinimumRiseMetres = 0.5f;

        private GameObject _gameObject;

        [TearDown]
        public void DestroyCharacter() => Object.Destroy(_gameObject);

        [UnityTest]
        public IEnumerator FlyHeld_RaisesTheBody() {
            _gameObject = new GameObject("Swim and fly character");
            _gameObject.AddComponent<CapsuleCollider>();
            var character = _gameObject.AddComponent<SwimAndFlyCharacter>();
            character.SwimAndFlyInput = new ScriptedSwimAndFlyInput { FlyHeld = true };
            var body = _gameObject.GetComponent<Rigidbody>();
            var start = body.position.y;

            for (var step = 0; step < FixedSteps; step++)
                yield return new WaitForFixedUpdate();

            Assert.AreSame(character.Fly, character.Motion.Active);
            Assert.Greater(body.position.y, start + MinimumRiseMetres);
        }
    }
}
