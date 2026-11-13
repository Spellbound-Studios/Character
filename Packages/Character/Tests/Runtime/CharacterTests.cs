// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;

namespace Spellbound.Character.Tests {
    public class CharacterTests {
        [Test]
        public void FrontDoorTypeExists() {
            Assert.IsNotNull(typeof(CharacterKit));
        }
    }
}
