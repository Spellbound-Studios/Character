// Copyright (c) Spellbound Studios. All rights reserved.

using System.Linq;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core;
using Spellbound.Core.Observations;

namespace Spellbound.Character.Editor.Tests {
    public class ContainmentObservationsTests {
        [Test]
        public void ContainmentRescue_IsDeclaredAsAnOccurrenceUnderCharacter() {
            Assert.AreEqual("character.containment.rescue", CharacterObservations.ContainmentRescue.Key.Name);

            var declaration = FindDeclaration("character.containment.rescue");

            Assert.AreEqual(ObservationKind.Occurrence, declaration.Kind);
            Assert.IsFalse(declaration.IsGuard);
        }

        [Test]
        public void ContainmentRepair_IsDeclaredAsAGuardWithTheDefaultPolicy() {
            Assert.AreEqual("character.containment.repair", CharacterObservations.ContainmentRepair.Key.Name);
            AssertDefaultPolicy(CharacterObservations.ContainmentRepair.Defaults);

            Assert.IsTrue(FindDeclaration("character.containment.repair").IsGuard);
        }

        [Test]
        public void SupportHeld_IsDeclaredAsAGuardWithTheDefaultPolicy() {
            Assert.AreEqual("character.support.held", CharacterObservations.SupportHeld.Key.Name);
            AssertDefaultPolicy(CharacterObservations.SupportHeld.Defaults);

            Assert.IsTrue(FindDeclaration("character.support.held").IsGuard);
        }

        private static ObservationDeclaration FindDeclaration(string name) {
            var declaration = CoreKit.Observations.Declared().SingleOrDefault(candidate => candidate.Key.Name == name);

            Assert.IsNotNull(declaration, name);
            Assert.AreEqual("character", declaration.Key.Package);
            Assert.IsFalse(string.IsNullOrEmpty(declaration.Description));
            return declaration;
        }

        private static void AssertDefaultPolicy(GuardPolicyValues values) {
            Assert.AreEqual(GuardPolicyValues.Default.Repairs, values.Repairs);
            Assert.AreEqual(GuardPolicyValues.Default.WindowSeconds, values.WindowSeconds);
            Assert.AreEqual(GuardPolicyValues.Default.HoldSeconds, values.HoldSeconds);
        }
    }
}
