// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Character.Body {
    public sealed class MotionClaims {
        private readonly List<IMotionDriver> _drivers = new();

        public IMotionDriver Active { get; private set; }

        public int Count => _drivers.Count;

        public MotionClaim Claim(IMotionDriver driver) {
            if (driver == null)
                throw new ArgumentNullException(nameof(driver));

            if (_drivers.Contains(driver))
                throw new InvalidOperationException("The motion driver already holds a claim.");

            _drivers.Add(driver);

            if (Active == null || driver.Priority > Active.Priority) {
                var handoff = Active?.End() ?? Handoff.AtRest(default, Vector3.up);
                Active = driver;
                driver.Begin(in handoff);
            }

            return new MotionClaim(this, driver);
        }

        internal void Release(in MotionClaim claim) {
            var driver = claim.Driver;

            if (driver == null || !_drivers.Remove(driver)) {
                Log.Error("[MotionClaims] A claim was released that is not held.");

                return;
            }

            if (!ReferenceEquals(driver, Active))
                return;

            var handoff = driver.End();
            Active = HighestPriority();
            Active?.Begin(in handoff);
        }

        private IMotionDriver HighestPriority() {
            IMotionDriver highest = null;

            foreach (var driver in _drivers) {
                if (highest == null || driver.Priority > highest.Priority)
                    highest = driver;
            }

            return highest;
        }
    }
}
