// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Character.Body {
    public enum ContainmentReason : byte {
        None = 0,
        BelowFloor = 1,
        Embedded = 2,
        SustainedFall = 3,
        SupportTimeout = 4
    }
}
