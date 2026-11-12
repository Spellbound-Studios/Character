// Copyright 2026 Spellbound Studio Inc.

using UnityEngine;
using UnityEngine.UIElements;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Shared layout for the runtime demo panels. Every panel adds its sub-panel into one absolutely-positioned,
    /// vertically-scrolling column (top-left) so any number of them stack without overlapping and stay reachable
    /// even when they run past the bottom of the screen. <see cref="GetColumn"/> finds or creates the scroll column
    /// on the document root (shared by name) and returns its content container; <see cref="MakePanel"/> returns a
    /// uniformly-styled container to fill and add to it.
    /// </summary>
    public static class DemoPanelLayout {
        private const string ScrollName = "demo-panel-scroll";

        /// <summary>
        /// Finds the shared scrolling panel column on <paramref name="root"/>, creating it on first use, and returns
        /// the container panels should be added to. Returns null if the root isn't ready yet.
        /// </summary>
        public static VisualElement GetColumn(VisualElement root) {
            if (root == null)
                return null;

            var scroll = root.Q<ScrollView>(ScrollName);

            if (scroll == null) {
                scroll = new ScrollView(ScrollViewMode.Vertical) {
                    name = ScrollName,
                    focusable = false,
                    style = {
                        position = Position.Absolute,
                        top = 10f,
                        left = 10f,
                        width = 284f,
                        maxHeight = Length.Percent(92f)
                    }
                };

                scroll.RegisterCallback<NavigationMoveEvent>(BlockNavigation, TrickleDown.TrickleDown);
                scroll.RegisterCallback<NavigationSubmitEvent>(BlockNavigation, TrickleDown.TrickleDown);
                scroll.RegisterCallback<NavigationCancelEvent>(BlockNavigation, TrickleDown.TrickleDown);

                root.Add(scroll);
            }

            return scroll.contentContainer;
        }

        /// <summary>
        /// A uniformly-styled panel container: fixed width, padding, translucent background, and a bottom margin so
        /// stacked panels are visually separated.
        /// </summary>
        public static VisualElement MakePanel() =>
                new() {
                    style = {
                        width = 260f,
                        marginBottom = 8f,
                        paddingTop = 8f, paddingBottom = 8f, paddingLeft = 8f, paddingRight = 8f,
                        backgroundColor = new Color(0f, 0f, 0f, 0.8f)
                    }
                };

        private static void BlockNavigation<TEvent>(TEvent evt) where TEvent : EventBase<TEvent>, new() {
            evt.StopPropagation();

            if (evt.target is VisualElement element)
                element.focusController?.IgnoreEvent(evt);
        }
    }
}
