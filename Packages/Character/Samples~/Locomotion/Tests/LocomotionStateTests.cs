// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spellbound.Character.Samples.Locomotion.Tests {
    public class LocomotionStateTests {
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
        public IEnumerator WalkState_IsEntered_OnSupportWithMoveInput() {
            _input.Move = Vector2.up;
            var character = _gym.CreateCharacter(_input);
            var start = character.transform.position;

            yield return TestGym.WaitFixedSteps(50);

            Assert.IsTrue(character.Machine.IsInState<WalkState>(), TestGym.Describe("WalkState", character));
            Assert.Greater(character.transform.position.z - start.z, 0.5f, "WalkState did not carry the body forward.");
        }

        [UnityTest]
        public IEnumerator RunState_IsEntered_WhileRunIsHeldOnSupport() {
            _input.Move = Vector2.up;
            _input.Run = true;
            var character = _gym.CreateCharacter(_input);

            yield return TestGym.WaitFixedSteps(50);

            Assert.IsTrue(character.Machine.IsInState<RunState>(), TestGym.Describe("RunState", character));
        }

        [UnityTest]
        public IEnumerator JumpState_IsEntered_OnJumpPressedFromSupport() {
            var character = _gym.CreateCharacter(_input);

            yield return TestGym.WaitFixedSteps(25, () => character.Machine.IsInState<WalkState>());

            Assert.IsTrue(character.Machine.IsInState<WalkState>(),
                "JumpState needs support first: " + TestGym.Describe("WalkState", character));

            _input.JumpPressed = true;
            yield return null;
            _input.JumpPressed = false;

            yield return TestGym.WaitFixedSteps(5, () => character.Machine.IsInState<JumpState>());

            Assert.IsTrue(character.Machine.IsInState<JumpState>(), TestGym.Describe("JumpState", character));
            Assert.Greater(character.Body.Rigidbody.linearVelocity.y, 0f, "JumpState did not lift the body.");
        }

        [UnityTest]
        public IEnumerator RunState_GoesBackToWalk_WhenTheMoveStops() {
            _input.Move = Vector2.up;
            _input.Run = true;
            var character = _gym.CreateCharacter(_input);

            yield return TestGym.WaitFixedSteps(50, () => character.Machine.IsInState<RunState>());

            Assert.IsTrue(character.Machine.IsInState<RunState>(),
                "WalkState needs RunState first: " + TestGym.Describe("RunState", character));

            _input.Move = Vector2.zero;

            yield return TestGym.WaitFixedSteps(5, () => character.Machine.IsInState<WalkState>());

            Assert.IsTrue(character.Machine.IsInState<WalkState>(), TestGym.Describe("WalkState", character));
        }

        [UnityTest]
        public IEnumerator FallState_StaysWhileAirborneOverNoGround() {
            Object.DestroyImmediate(_gym.Ground);
            _input.Move = Vector2.up;
            var character = _gym.CreateCharacter(_input);

            for (var step = 0; step < 10; step++) {
                yield return new WaitForFixedUpdate();

                Assert.IsTrue(character.Machine.IsInState<FallState>(),
                    $"Step {step}: " + TestGym.Describe("FallState", character));
            }
        }

        [UnityTest]
        public IEnumerator FallState_StaysWhenJumpIsPressedInTheAir() {
            Object.DestroyImmediate(_gym.Ground);
            var character = _gym.CreateCharacter(_input);

            yield return new WaitForFixedUpdate();

            Assert.IsTrue(character.Machine.IsInState<FallState>(), TestGym.Describe("FallState", character));

            _input.JumpPressed = true;
            yield return null;
            _input.JumpPressed = false;

            for (var step = 0; step < 10; step++) {
                yield return new WaitForFixedUpdate();

                Assert.IsTrue(character.Machine.IsInState<FallState>(),
                    $"Step {step}: " + TestGym.Describe("FallState", character));
            }
        }

        [UnityTest]
        [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.OSXEditor, RuntimePlatform.LinuxEditor)]
        public IEnumerator LocomotionCharacterPrefab_WalksOnSupportWithMoveInput() {
            var prefab = TestGym.FindImportedPrefab();

            Assert.IsNotNull(prefab, "The imported sample's LocomotionCharacter.prefab was not found under Assets.");

            _input.Move = Vector2.up;
            var character = _gym.CreateCharacterFromPrefab(prefab, _input);
            var start = character.transform.position;

            yield return TestGym.WaitFixedSteps(50);

            Assert.IsTrue(character.Machine.IsInState<WalkState>(), TestGym.Describe("WalkState", character));
            Assert.Greater(character.transform.position.z - start.z, 0.5f,
                "The prefab's WalkState did not carry the body forward.");
        }

        [UnityTest]
        public IEnumerator FallState_IsEntered_WhenSupportIsLost() {
            var character = _gym.CreateCharacter(_input);

            yield return TestGym.WaitFixedSteps(25, () => character.Machine.IsInState<WalkState>());

            Assert.IsTrue(character.Machine.IsInState<WalkState>(),
                "FallState needs support first: " + TestGym.Describe("WalkState", character));

            Object.DestroyImmediate(_gym.Ground);

            yield return TestGym.WaitFixedSteps(10, () => character.Machine.IsInState<FallState>());

            Assert.IsTrue(character.Machine.IsInState<FallState>(), TestGym.Describe("FallState", character));
        }
    }
}
