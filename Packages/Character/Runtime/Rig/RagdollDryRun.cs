// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Spellbound.Character.Rig {
    public sealed class RagdollDryRun {
        private readonly RagdollDryRunBone[] _bones;
        private readonly string[] _missing;

        public RagdollDryRun(string error, IReadOnlyList<RagdollDryRunBone> bones, IReadOnlyList<string> missing) {
            if (bones == null)
                throw new ArgumentNullException(nameof(bones));

            if (missing == null)
                throw new ArgumentNullException(nameof(missing));

            Error = error ?? string.Empty;
            _bones = new RagdollDryRunBone[bones.Count];

            for (var i = 0; i < bones.Count; i++)
                _bones[i] = bones[i];

            _missing = new string[missing.Count];

            for (var i = 0; i < missing.Count; i++)
                _missing[i] = missing[i];
        }

        public string Error { get; }

        public bool WouldBuild => Error.Length == 0;

        public IReadOnlyList<RagdollDryRunBone> Bones => _bones;

        public IReadOnlyList<string> Missing => _missing;

        public string ToText() {
            var text = new StringBuilder();

            if (WouldBuild)
                text.Append("would build ")
                    .Append(_bones.Length.ToString(CultureInfo.InvariantCulture))
                    .Append(" bones");
            else
                text.Append("cannot build: ").Append(Error);

            foreach (var bone in _bones) {
                text.Append('\n')
                    .Append(bone.Name)
                    .Append(' ')
                    .Append(bone.Shape.ToString())
                    .Append(' ')
                    .Append(bone.MassShare.ToString("0.00", CultureInfo.InvariantCulture))
                    .Append(' ')
                    .Append(bone.Path);
            }

            if (_missing.Length > 0)
                text.Append("\nmissing: ").Append(string.Join(", ", _missing));

            return text.ToString();
        }
    }
}
