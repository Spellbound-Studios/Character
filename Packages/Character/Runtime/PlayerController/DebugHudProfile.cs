// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Controller {
    [CreateAssetMenu(fileName = "DebugHUDProfile", menuName = "Spellbound/CharacterController/DebugHUDProfile")]
    public class DebugHudProfile : ScriptableObject {
        [Serializable]
        public class FieldOption {
            public string key;
            public bool enabled = true;

            [NonSerialized] public float LastActiveTime;
        }

        public List<FieldOption> fieldToggles = new();

        [NonSerialized] private Dictionary<string, FieldOption> _index;

        public Dictionary<string, FieldOption> Index {
            get {
                if (_index != null)
                    return _index;

                RebuildIndex();

                return _index;
            }
        }

        /// <summary>
        /// Rebuilds the index dictionary from the current field toggles list.
        /// </summary>
        private void RebuildIndex() {
            _index = new Dictionary<string, FieldOption>(fieldToggles.Count);

            for (var i = fieldToggles.Count - 1; i >= 0; i--) {
                var fo = fieldToggles[i];

                // Remove null or empty entries during rebuild
                if (fo == null || string.IsNullOrEmpty(fo.key)) {
                    fieldToggles.RemoveAt(i);

                    continue;
                }

                // Handle duplicate keys by keeping the first one
                if (!_index.TryAdd(fo.key, fo)) {
                    // Remove duplicate
                    fieldToggles.RemoveAt(i);
                }
            }
        }

        public FieldOption GetOrAdd(string key) {
            var idx = Index;

            if (idx.TryGetValue(key, out var fo)) {
                // Mark as recently active
                fo.LastActiveTime = Time.time;

                return fo;
            }

            fo = new FieldOption {
                key = key,
                enabled = true,
                LastActiveTime = Time.time
            };

            fieldToggles.Add(fo);
            idx[key] = fo;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            return fo;
        }

        /// <summary>
        /// Clears the index cache, forcing it to rebuild on next access.
        /// </summary>
        public void ClearIndexCache() => _index = null;

        /// <summary>
        /// Removes fields that are no longer active.
        /// </summary>
        public int RemoveInactiveFields(HashSet<string> activeFields) {
            var removedCount = 0;

            for (var i = fieldToggles.Count - 1; i >= 0; i--) {
                var fo = fieldToggles[i];

                if (fo != null && !string.IsNullOrEmpty(fo.key) && activeFields.Contains(fo.key))
                    continue;

                fieldToggles.RemoveAt(i);
                removedCount++;
            }

            if (removedCount <= 0)
                return removedCount;

            ClearIndexCache();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

            return removedCount;
        }

        /// <summary>
        /// Gets count of total fields in profile.
        /// </summary>
        public int GetFieldCount() => fieldToggles.Count;

        /// <summary>
        /// Gets count of enabled fields.
        /// </summary>
        public int GetEnabledFieldCount() {
            var count = 0;

            foreach (var fo in fieldToggles) {
                if (fo != null && fo.enabled)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Validates and cleans up the profile data.
        /// </summary>
        public void ValidateProfile() {
            var changed = false;

            // Remove null or empty entries
            for (var i = fieldToggles.Count - 1; i >= 0; i--) {
                var fo = fieldToggles[i];

                if (fo != null && !string.IsNullOrEmpty(fo.key))
                    continue;

                fieldToggles.RemoveAt(i);
                changed = true;
            }

            // Remove duplicates
            var seen = new HashSet<string>();

            for (var i = fieldToggles.Count - 1; i >= 0; i--) {
                var fo = fieldToggles[i];

                if (seen.Add(fo.key))
                    continue;

                fieldToggles.RemoveAt(i);
                changed = true;
            }

            if (!changed)
                return;

            ClearIndexCache();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        public void SaveNow() {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Editor-only method to manually clean up stale entries.
        /// </summary>
        [ContextMenu("Clean Up Profile")]
        public void EditorCleanUp() {
            ValidateProfile();
            Log.Info($"Profile cleaned up. {GetFieldCount()} fields remaining, {GetEnabledFieldCount()} enabled.");
        }

        private void OnValidate() =>
                // Automatically validate when modified in editor
                ValidateProfile();
#endif
    }
}