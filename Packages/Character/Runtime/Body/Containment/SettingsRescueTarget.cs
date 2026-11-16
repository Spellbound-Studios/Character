// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Body {
    public sealed class SettingsRescueTarget : IRescueTarget {
        private readonly ContainmentSettings _settings;

        public SettingsRescueTarget(ContainmentSettings settings) {
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
        }

        public Pose Fallback(Vector3 from) => new(_settings.fallbackAnchor, Quaternion.identity);
    }
}
