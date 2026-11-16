// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Body {
    public interface IRescueTarget {
        Pose Fallback(Vector3 from);
    }
}
