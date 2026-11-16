// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public interface IMoveSource {
        Vector3 DesiredMove { get; }

        Vector3 DesiredUp { get; }
    }
}
