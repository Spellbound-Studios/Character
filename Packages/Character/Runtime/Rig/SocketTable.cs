// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections.Generic;

namespace Spellbound.Character.Rig {
    public sealed class SocketTable {
        private readonly Dictionary<string, ResolvedSocket> _sockets;
        private readonly string[] _ids;
        private readonly string[] _defaulted;
        private readonly string[] _missing;

        public SocketTable(
            IReadOnlyList<ResolvedSocket> resolved,
            IReadOnlyList<string> defaulted,
            IReadOnlyList<string> missing) {
            if (resolved == null)
                throw new ArgumentNullException(nameof(resolved));

            if (defaulted == null)
                throw new ArgumentNullException(nameof(defaulted));

            if (missing == null)
                throw new ArgumentNullException(nameof(missing));

            _sockets = new Dictionary<string, ResolvedSocket>(resolved.Count, StringComparer.Ordinal);
            _ids = new string[resolved.Count];

            for (var i = 0; i < resolved.Count; i++) {
                _ids[i] = resolved[i].Id;
                _sockets.Add(resolved[i].Id, resolved[i]);
            }

            _defaulted = Copy(defaulted);
            _missing = Copy(missing);
        }

        public int Count => _ids.Length;

        public IReadOnlyList<string> Ids => _ids;

        public IReadOnlyList<string> Defaulted => _defaulted;

        public IReadOnlyList<string> Missing => _missing;

        public bool IsComplete => _missing.Length == 0;

        public bool TryGet(string id, out ResolvedSocket socket) {
            if (id != null)
                return _sockets.TryGetValue(id, out socket);

            socket = default;

            return false;
        }

        private static string[] Copy(IReadOnlyList<string> source) {
            var copy = new string[source.Count];

            for (var i = 0; i < source.Count; i++)
                copy[i] = source[i];

            return copy;
        }
    }
}
