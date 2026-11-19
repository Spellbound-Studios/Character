// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using global::Spellbound.WorldObjects;

namespace Spellbound.Character.WorldObjects.Tests {
    public sealed class CountingPhysicsStepPolicy : IPhysicsStepPolicy {
        public Func<PhysicsStepRequest, bool> Rule { get; set; } = _ => true;

        public List<PhysicsStepRequest> Requests { get; } = new();

        public int Count => Requests.Count;

        public Action<PhysicsStepRequest> Asked { get; set; }

        public bool ShouldStep(in PhysicsStepRequest request) {
            Requests.Add(request);
            Asked?.Invoke(request);
            return Rule(request);
        }
    }
}
