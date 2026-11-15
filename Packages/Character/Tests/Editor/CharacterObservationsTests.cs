// Copyright (c) Spellbound Studios. All rights reserved.

using System.Linq;
using NUnit.Framework;
using Spellbound.Character.Body;
using Spellbound.Core;
using Spellbound.Core.Observations;

namespace Spellbound.Character.Editor.Tests {
    public class CharacterObservationsTests {
        [Test]
        public void ReferenceFrameBound_IsDeclaredAsAnOccurrenceUnderCharacter() {
            Assert.AreEqual("character.reference_frame.bound", CharacterObservations.ReferenceFrameBound.Key.Name);

            var declaration = CoreKit.Observations.Declared()
                    .SingleOrDefault(candidate => candidate.Key.Name == "character.reference_frame.bound");

            Assert.IsNotNull(declaration);
            Assert.AreEqual(ObservationKind.Occurrence, declaration.Kind);
            Assert.AreEqual("character", declaration.Key.Package);
            Assert.AreEqual("A body started riding a moving reference frame.", declaration.Description);
        }
    }
}
