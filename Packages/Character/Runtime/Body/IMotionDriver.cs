// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Character.Body {
    public interface IMotionDriver {
        int Priority { get; }
        bool NeedsSupport { get; }
        void Begin(in Handoff entering);
        Handoff End();
    }
}
