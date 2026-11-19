// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using Spellbound.Core.Simulation;
using UnityEngine;

namespace Spellbound.Character.WorldObjects.Tests {
    [DefaultExecutionOrder(-31999)]
    public sealed class StepStartRecorder : MonoBehaviour {
        public List<string> Sequence;
        public TickClock Clock;

        private void FixedUpdate() {
            Sequence.Add("start tick=" + Clock.Current.Value);
        }
    }
}
