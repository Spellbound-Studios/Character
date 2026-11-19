// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using System.Globalization;
using Spellbound.Character.Body;
using Spellbound.Core.Simulation;
using UnityEngine;

namespace Spellbound.Character.WorldObjects.Tests {
    [DefaultExecutionOrder(0)]
    public sealed class StepSequenceRecorder : MonoBehaviour {
        public List<string> Sequence;
        public TickClock Clock;
        public ReferenceFrameRider Rider;

        private void FixedUpdate() {
            Sequence.Add("scripts tick=" + Clock.Current.Value + " bound=" + Rider.Binding.IsBound + " step=" +
                Time.fixedDeltaTime.ToString("R", CultureInfo.InvariantCulture));
        }
    }
}
