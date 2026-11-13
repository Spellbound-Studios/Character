// Copyright (c) Spellbound Studios. All rights reserved.

using NUnit.Framework;

namespace Spellbound.Character.Editor.Tests {
    public class CharacterEditorTests {
        [Test]
        public void FrontDoorTypeExists() {
            Assert.IsNotNull(typeof(CharacterKit));
        }
    }
}
