// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Character.Samples.Locomotion.Tests {
    public class MoveSourceTests {
        private const float Tolerance = 0.0001f;

        private TestGym _gym;
        private ScriptedLocomotionInput _input;

        [SetUp]
        public void CreateGym() {
            _gym = new TestGym();
            _input = new ScriptedLocomotionInput();
        }

        [TearDown]
        public void DestroyGym() => _gym.Dispose();

        [UnityTest]
        public IEnumerator Character_IsTheContextsMoveSource_WithItsInputMoveAndTheWorldsUp() {
            _input.Move = Vector2.right;
            var character = _gym.CreateCharacter(_input);

            yield return TestGym.WaitFixedSteps(3);

            Assert.AreSame(character, character.Body.MoveSource);
            Assert.Less(Vector3.Distance(new Vector3(1f, 0f, 0f), character.DesiredMove), Tolerance);
            Assert.Less(Vector3.Distance(character.DesiredMove, character.Body.DesiredMove), Tolerance);
            Assert.Less(Vector3.Distance(new Vector3(0f, 1f, 0f), character.DesiredUp), Tolerance);
            Assert.Less(Vector3.Distance(character.DesiredUp, character.Body.DesiredUp), Tolerance);
        }
    }
}
