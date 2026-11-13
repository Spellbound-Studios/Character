// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public static class PlayableCharacters {
        public static bool IsPlayable(GameObject prefab) {
            if (prefab == null)
                return false;

            foreach (var component in prefab.GetComponents<Component>()) {
                if (component is IPlayableCharacter)
                    return true;
            }

            return false;
        }
    }
}
