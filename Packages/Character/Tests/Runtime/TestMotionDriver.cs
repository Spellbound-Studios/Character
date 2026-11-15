// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;

namespace Spellbound.Character.Tests {
    public sealed class TestMotionDriver : IMotionDriver {
        public TestMotionDriver(int priority, bool needsSupport) {
            Priority = priority;
            NeedsSupport = needsSupport;
        }

        public int Priority { get; }

        public bool NeedsSupport { get; }

        public Handoff Entered { get; private set; }

        public Handoff Leaving { get; set; }

        public int BeginCount { get; private set; }

        public int EndCount { get; private set; }

        public void Begin(in Handoff entering) {
            Entered = entering;
            BeginCount++;
        }

        public Handoff End() {
            EndCount++;
            return Leaving;
        }
    }
}
