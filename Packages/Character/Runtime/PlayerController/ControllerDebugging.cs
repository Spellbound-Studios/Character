// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Spellbound.Controller {
    /// <summary>
    /// This script should only be attached if you're interested in debugging to canvas. It is purely for debugging
    /// purposes and does not use the best practices for performance.
    /// </summary>
    public class ControllerDebugging : MonoBehaviour {
        public static ControllerDebugging Instance;

        [Header("HUD Settings"), SerializeField]
        private float fontSize = 20f;

        [SerializeField] private Color fontColor = Color.white;
        [SerializeField] private DebugHudProfile profile;
        [SerializeField] private List<FieldOption> fieldToggles = new();

        [Header("Gizmos"), SerializeField] private bool showRaycastGizmos = true;

        private readonly Dictionary<string, Func<string>> _getters = new();
        private readonly Dictionary<string, TMP_Text> _labels = new();
        private readonly List<Action> _gizmos = new();
        private readonly Dictionary<string, FieldOption> _toggleIndex = new();

        private Canvas _debugCanvas;
        private RectTransform _container;

        private bool _togglesDirty;
        private bool _layoutDirty;
        private bool _profileDirty;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);

                return;
            }

            Instance = this;

            if (!profile) {
                Log.Error("Profile object is missing. Please locate in prefab folder and drag and drop.");
            }

            CreateCanvas();
            _container = EnsureContainer(_debugCanvas.transform);

            SyncFromProfile();
            RebuildToggleIndex();
        }

        private void OnEnable() {
            RegisterAllProviders();
            ApplyToggleVisibility();
        }

        private void OnDisable() {
            _getters.Clear();
            _labels.Clear();
            _gizmos.Clear();

            if (!_container)
                return;

            foreach (Transform child in _container)
                Destroy(child.gameObject);
        }

        private void Update() {
            foreach (var kv in _getters) {
                if (!IsEnabled(kv.Key))
                    continue;

                if (!_labels.TryGetValue(kv.Key, out var label) || !label)
                    continue;

                string value;

                try {
                    value = kv.Value?.Invoke() ?? "";
                }
                catch {
                    value = "(err)";
                }

                label.text = $"{kv.Key}: {value}";

                if (Mathf.Approximately(label.fontSize, fontSize) && label.color == fontColor)
                    continue;

                label.fontSize = fontSize;
                label.color = fontColor;
                _layoutDirty = true;
            }
        }

        private void LateUpdate() {
            if (_togglesDirty) {
                _togglesDirty = false;
                ApplyToggleVisibility();
            }

            if (_profileDirty) {
                _profileDirty = false;
                PushToProfile();
            }

            if (!_layoutDirty || !_container)
                return;

            _layoutDirty = false;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_container);
            Canvas.ForceUpdateCanvases();
        }

        private void CreateCanvas() {
            var canvasGo = new GameObject("DebugCanvas");

            _debugCanvas = canvasGo.AddComponent<Canvas>();
            _debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var canvasScaler = canvasGo.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            // Make it persist across scenes (optional)
            DontDestroyOnLoad(canvasGo);

            EnsureEventSystem();
        }

        private static void EnsureEventSystem() {
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
                return;

            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        #region Debugging Backend

        // #########################
        // Debugging Infrastructure.
        // #########################

        private void OnDrawGizmosSelected() {
            if (!showRaycastGizmos)
                return;

            foreach (var t in _gizmos)
                t?.Invoke();
        }

        private RectTransform EnsureContainer(Transform canvasRoot) {
            var panel = new GameObject(
                "DebugHUD",
                typeof(RectTransform),
                typeof(Image),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter)
            );
            var rt = (RectTransform)panel.transform;
            rt.SetParent(canvasRoot, false);

            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(12f, -12f);
            rt.sizeDelta = new Vector2(420f, 0f);

            var img = panel.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.45f);

            var v = panel.GetComponent<VerticalLayoutGroup>();
            v.childAlignment = TextAnchor.UpperLeft;
            v.childForceExpandWidth = false;
            v.childForceExpandHeight = false;
            v.padding = new RectOffset(8, 8, 8, 8);
            v.spacing = 4f;

            var fit = panel.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return rt;
        }

        private TMP_Text CreateRow(string key) {
            var go = new GameObject(
                $"{key}",
                typeof(RectTransform),
                typeof(TextMeshProUGUI),
                typeof(LayoutElement));
            go.transform.SetParent(_container, false);

            var le = go.GetComponent<LayoutElement>();
            le.minHeight = fontSize + 4f;
            le.preferredHeight = -1;
            le.flexibleHeight = 1f;
            le.flexibleWidth = 0f;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.color = fontColor;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            tmp.text = $"{key}:";

            tmp.enableAutoSizing = false;
            tmp.overflowMode = TextOverflowModes.Overflow;

            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(0f, fontSize + 4f);

            return tmp;
        }

        private void RegisterAllProviders() {
            _getters.Clear();
            _labels.Clear();
            _gizmos.Clear();

            foreach (Transform child in _container)
                Destroy(child.gameObject);

            var activeFields = new HashSet<string>();

            var comps = GetComponents<IDebuggingInfo>();

            foreach (var t in comps)
                t.RegisterDebugInfo(this);

            foreach (var key in _getters.Keys) activeFields.Add(key);

            CleanupProfile(activeFields);
        }

        /// <summary>
        /// Removes stale entries from the profile that are no longer active.
        /// </summary>
        private void CleanupProfile(HashSet<string> activeFields) {
            if (!profile)
                return;

            var removedCount = profile.RemoveInactiveFields(activeFields);

            if (removedCount <= 0)
                return;

            SyncFromProfile();
            RebuildToggleIndex();

#if UNITY_EDITOR
            _profileDirty = true;
            Log.Info($"[DebugHUD] Removed {removedCount} stale debug field(s) from profile");
#endif
        }

        public void Field(string key, Func<string> getter) {
            if (string.IsNullOrEmpty(key) || getter == null)
                return;

            _getters[key] = getter;

            EnsureToggle(key);

            EnsureRowVisibility(key, IsEnabled(key));
        }

        public void Gizmo(Action draw) {
            if (draw != null)
                _gizmos.Add(draw);
        }

        private void RebuildToggleIndex() {
            _toggleIndex.Clear();

            foreach (var fo in fieldToggles) {
                if (fo != null && !string.IsNullOrEmpty(fo.key))
                    _toggleIndex[fo.key] = fo;
            }
        }

        private void EnsureToggle(string key) {
            if (_toggleIndex.ContainsKey(key))
                return;

            var opt = new FieldOption {
                key = key,
                enabled = true
            };

            fieldToggles.Add(opt);
            _toggleIndex[key] = opt;

            if (!profile)
                return;

            profile.GetOrAdd(key);
#if UNITY_EDITOR
            _profileDirty = true;
#endif
        }

        private bool IsEnabled(string key) => !_toggleIndex.TryGetValue(key, out var opt) || opt.enabled;

        private void EnsureRowVisibility(string key, bool shouldExist) {
            var has = _labels.TryGetValue(key, out var label) && label;

            switch (shouldExist) {
                case true when !has:
                    _labels[key] = CreateRow(key);
                    _layoutDirty = true;

                    break;
                case false when has:
                    Destroy(label.gameObject);
                    _labels.Remove(key);
                    _layoutDirty = true;

                    break;
            }
        }

        private void ApplyToggleVisibility() {
            RebuildToggleIndex();

            foreach (var key in _getters.Keys)
                EnsureRowVisibility(key, IsEnabled(key));

            _layoutDirty = true;
        }

        private void SyncFromProfile() {
            if (!profile)
                return;

            fieldToggles.Clear();

            foreach (var profileField in profile.fieldToggles) {
                if (profileField != null && !string.IsNullOrEmpty(profileField.key)) {
                    fieldToggles.Add(new FieldOption {
                        key = profileField.key,
                        enabled = profileField.enabled
                    });
                }
            }

            RebuildToggleIndex();
        }

        private void PushToProfile() {
            if (!profile)
                return;

            foreach (var fo in fieldToggles) {
                if (fo == null || string.IsNullOrEmpty(fo.key))
                    continue;

                var p = profile.GetOrAdd(fo.key);
                p.enabled = fo.enabled;
            }
#if UNITY_EDITOR
            profile.SaveNow();
#endif
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (!Application.isPlaying)
                return;

            _togglesDirty = true;
            _profileDirty = true;
        }
#endif

        [Serializable]
        private class FieldOption {
            public string key;
            public bool enabled = true;
        }

        #endregion
    }
}