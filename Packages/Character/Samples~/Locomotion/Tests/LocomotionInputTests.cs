// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Spellbound.Character.Samples.Locomotion.Tests {
    public class LocomotionInputTests {
        private TestGym _gym;

        [SetUp]
        public void CreateGym() => _gym = new TestGym();

        [TearDown]
        public void DestroyGym() => _gym.Dispose();

        [UnityTest]
        public IEnumerator ILocomotionInput_CustomImplementation() {
            var character = _gym.CreateCharacter<HeldForwardInput>();
            var start = character.transform.position;

            yield return TestGym.WaitFixedSteps(50);

            Assert.IsInstanceOf<HeldForwardInput>(character.LocomotionInput);
            Assert.IsTrue(character.Machine.IsInState<WalkState>(), TestGym.Describe("WalkState", character));
            Assert.Greater(character.transform.position.z - start.z, 0.5f,
                "The custom input did not carry the body forward.");
        }
    }
}
