// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using Spellbound.Core.Ground;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Character.Body {
    public sealed class RescueLadder {
        private const float RangeTolerance = 0.0001f;

        private readonly ContainmentSettings _settings;
        private readonly IContainmentSpace _space;
        private readonly IRescueTarget _fallback;
        private Vector3 _lastSupported;
        private bool _hasLastSupported;

        public RescueLadder(ContainmentSettings settings, IContainmentSpace space, IRescueTarget fallback) {
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            _space = space ?? throw new ArgumentNullException(nameof(space));
            _fallback = fallback ?? throw new ArgumentNullException(nameof(fallback));
        }

        public void RememberSupported(Vector3 position) {
            _lastSupported = position;
            _hasLastSupported = true;
        }

        public RescueStep Resolve(Vector3 from, Vector3 worldUp, Vector3 bodyExtents, ContainmentReason reason) {
            if (reason == ContainmentReason.SustainedFall)
                return new RescueStep(_fallback.Fallback(from), RescueRung.Fallback, reason);

            var size = bodyExtents * 2f;

            if (_hasLastSupported && IsEmpty(_lastSupported, size))
                return new RescueStep(new Pose(_lastSupported, Quaternion.identity), RescueRung.LastSupported, reason);

            if (TryFindOpenAbove(from, worldUp, size, out var open))
                return new RescueStep(new Pose(open, Quaternion.identity), RescueRung.OpenAbove, reason);

            Log.Warn($"[RescueLadder] No open space within {_settings.maxOpenSearchMetres} metres above {from}; " +
                     "the body goes to the fallback.");

            return new RescueStep(_fallback.Fallback(from), RescueRung.Fallback, reason);
        }

        private bool TryFindOpenAbove(Vector3 from, Vector3 worldUp, Vector3 size, out Vector3 open) {
            open = from;
            var stepMetres = _settings.openSearchStepMetres;
            var rangeMetres = _settings.maxOpenSearchMetres;

            if (!(stepMetres > 0f) || !(rangeMetres > 0f) || worldUp.sqrMagnitude <= 0f)
                return false;

            var direction = worldUp.normalized;
            var steps = Mathf.FloorToInt(rangeMetres / stepMetres + RangeTolerance);

            for (var index = 1; index <= steps; index++) {
                var candidate = from + direction * (stepMetres * index);

                if (!IsEmpty(candidate, size))
                    continue;

                open = candidate;

                return true;
            }

            return false;
        }

        private bool IsEmpty(Vector3 center, Vector3 size) =>
                _space.Classify(new Bounds(center, size)) == Occupancy.Empty;
    }
}
