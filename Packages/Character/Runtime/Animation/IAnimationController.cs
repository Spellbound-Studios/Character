// Copyright 2026 Spellbound Studio Inc.

namespace Spellbound.Controller {
    /// <summary>
    /// A backend-agnostic seam for driving an animation system, exposing only generic primitives — crossfade to a
    /// state, set a parameter, set a layer's weight. It carries no notion of what any state, parameter, or layer
    /// <em>means</em>: "locomotion", "action", "blend speed", a masked upper body — those are the consumer's
    /// vocabulary, assigned by which hashes and layer indices it passes. State and parameter ids are opaque
    /// <c>int</c>s the consumer mints however it likes (<c>Animator.StringToHash</c>, a registry, …); the library
    /// never hashes or names anything. The consumer supplies the implementation — a plain <c>Animator</c>, a
    /// networked one — and owns clip storage, authority, and replication; none of that leaks in here.
    /// </summary>
    public interface IAnimationController {
        /// <summary>
        /// Crossfades <paramref name="layer"/> to the state <paramref name="stateHash"/> over <paramref name="fade"/>
        /// seconds (0 = the implementation's default). Whatever that state plays — a single clip or a blend tree —
        /// is authored in the backend, not here.
        /// </summary>
        void CrossFade(int stateHash, int layer = 0, float fade = 0f);

        /// <summary>
        /// Sets a float parameter (e.g. a blend axis or a playback-speed multiplier — the consumer decides which).
        /// </summary>
        void SetFloat(int paramHash, float value);

        /// <summary>
        /// Sets a layer's blend weight in 0..1 (e.g. to fade a masked layer over the base — the consumer decides).
        /// </summary>
        void SetLayerWeight(int layer, float weight);
    }
}
