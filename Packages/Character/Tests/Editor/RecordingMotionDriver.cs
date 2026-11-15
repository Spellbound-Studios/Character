// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;

namespace Spellbound.Character.Editor.Tests {
    public sealed class RecordingMotionDriver : IMotionDriver {
        public RecordingMotionDriver(int priority, Handoff leaving) {
            Priority = priority;
            Leaving = leaving;
        }

        public int Priority { get; }

        public bool NeedsSupport => true;

        public Handoff Leaving { get; }

        public Handoff LastEntered { get; private set; }

        public int BeginCount { get; private set; }

        public int EndCount { get; private set; }

        public void Begin(in Handoff entering) {
            LastEntered = entering;
            BeginCount++;
        }

        public Handoff End() {
            EndCount++;

            return Leaving;
        }
    }
}
