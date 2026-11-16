// Copyright (c) Spellbound Studios. All rights reserved.

using System;
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

        [SerializeField, Tooltip("Where a character goes when a rescue finds nothing nearer. Empty = this world's " +
                                 "own position.")]
        private Transform spawnMarker;

        [SerializeField, Tooltip("Unit cubes, scaled by their transforms, that the gym's containment space counts " +
                                 "as solid.")]
        private Transform[] solidRegions = Array.Empty<Transform>();

        [SerializeField, Tooltip("The characters this world hands its space, its rescue target and itself to when " +
                                 "it wakes.")]
        private GameObject[] characters = Array.Empty<GameObject>();

        private TickClock _clock;

        public ReferenceFrameDirectory ReferenceFrames { get; private set; }

        public ObservationContext Observations { get; private set; }

        public Tick CurrentTick => _clock != null ? _clock.Current : default;

        public IContainmentSpace Space { get; set; }

        public IRescueTarget RescueTarget { get; set; }

        public string ContextName {
            get => contextName;
            set => contextName = value;
        }

        public void Admit(GameObject character) {
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            if (character.TryGetComponent(out SupportGate gate)) {
                gate.Space = Space;
                gate.RescueTarget = RescueTarget;
                gate.World = this;
            }

            if (character.TryGetComponent(out Containment containment)) {
                containment.Space = Space;
                containment.RescueTarget = RescueTarget;
                containment.MoveSource = character.GetComponent<IMoveSource>();
                containment.World = this;
            }
        }

        private void Awake() {
            ReferenceFrames = new ReferenceFrameDirectory();
            Observations = CoreKit.Observations.CreateContext(contextName);
            _clock = CoreKit.Time.StartClock(TickRate.PerSecond(ticksPerSecond), Observations);
            Space ??= CreateSpace();
            RescueTarget ??= new GymRescueTarget(spawnMarker != null ? spawnMarker : transform);

            foreach (var character in characters) {
                if (character != null)
                    Admit(character);
            }
        }

        private void OnDestroy() {
            CoreKit.Time.StopClock(_clock);
            _clock = null;
            Observations?.Dispose();
            Observations = null;
        }

        private GymContainmentSpace CreateSpace() {
            var space = new GymContainmentSpace();

            foreach (var region in solidRegions) {
                if (region != null)
                    space.Solids.Add(new Bounds(region.position, region.lossyScale));
            }

            return space;
        }
    }
}
