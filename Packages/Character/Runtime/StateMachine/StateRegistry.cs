// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Registries;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// Resolves a <see cref="BaseSoState"/> from its stable uint hash, and packs / unpacks that hash for saves
    /// and network — "send one id, get the state." A thin facade over a Core <see cref="HashRegistry{TEntry}"/>
    /// populated once from every state asset under a <c>Resources/States</c> folder (same pattern as the other
    /// Spellbound registries).
    /// </summary>
    public static class StateRegistry {
        private const string ResourceFolder = "States";

        private static readonly HashRegistry<BaseSoState> Registry = new();
        private static bool _isLoaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void WarmUp() => EnsureLoaded();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForPlaySession() {
            Registry.Clear();
            _isLoaded = false;
        }

        public static int Count {
            get {
                EnsureLoaded();

                return Registry.Count;
            }
        }

        public static IReadOnlyList<BaseSoState> All {
            get {
                EnsureLoaded();

                return Registry.All;
            }
        }

        public static bool TryGet(uint hash, out BaseSoState state) {
            EnsureLoaded();

            return Registry.TryGet(hash, out state);
        }

        public static BaseSoState Get(uint hash) {
            EnsureLoaded();

            return Registry.Get(hash);
        }

        public static bool Contains(uint hash) {
            EnsureLoaded();

            return Registry.Contains(hash);
        }

        public static void WriteState(ref Span<byte> buffer, BaseSoState state) =>
                RegistryPacker.Write(ref buffer, state);

        public static BaseSoState ReadState(ref ReadOnlySpan<byte> buffer) {
            EnsureLoaded();

            return RegistryPacker.Read(ref buffer, Registry);
        }

        private static void EnsureLoaded() {
            if (_isLoaded)
                return;

            // Set first so the scan runs once even if the States folder is missing or empty.
            _isLoaded = true;
            Registry.AddRange(Resources.LoadAll<BaseSoState>(ResourceFolder));
        }
    }
}
