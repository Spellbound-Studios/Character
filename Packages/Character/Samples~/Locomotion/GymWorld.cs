// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using Spellbound.Core;
using Spellbound.Core.Observations;
using Spellbound.Core.ReferenceFrames;
using Spellbound.Core.Simulation;
using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    [DefaultExecutionOrder(-1000)]
    public sealed class GymWorld : MonoBehaviour, IReferenceFrameWorld {
        [SerializeField, Tooltip("The name of this world's observation context. Two live worlds need two names.")]
        private string contextName = "gym";

        [SerializeField, Min(1f), Tooltip("Ticks per second of this world's clock.")]
        private float ticksPerSecond = 50f;

        private TickClock _clock;

        public ReferenceFrameDirectory ReferenceFrames { get; private set; }

        public ObservationContext Observations { get; private set; }

        public Tick CurrentTick => _clock != null ? _clock.Current : default;

        public string ContextName {
            get => contextName;
            set => contextName = value;
        }

        private void Awake() {
            ReferenceFrames = new ReferenceFrameDirectory();
            Observations = CoreKit.Observations.CreateContext(contextName);
            _clock = CoreKit.Time.StartClock(TickRate.PerSecond(ticksPerSecond), Observations);
        }

        private void OnDestroy() {
            CoreKit.Time.StopClock(_clock);
            _clock = null;
            Observations?.Dispose();
            Observations = null;
        }
    }
}
