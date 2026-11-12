// Copyright 2025 Spellbound Studio Inc.

using Spellbound.Core.Registries;

namespace Spellbound.Controller {
    /// <summary>
    /// Base type for ScriptableObject states. Each concrete state is an asset that the state machine holds and
    /// swaps. Inherits its stable identity (asset GUID + FNV-1a hash) from <see cref="HashedScriptableObject"/>,
    /// so a state can be referenced by a single value. See <see cref="StateRegistry"/>.
    /// </summary>
    public abstract class BaseSoState : HashedScriptableObject {
        public object Ctx;

        /// <summary>
        /// Called once when the state machine initializes to cache the context.
        /// </summary>
        public virtual void InitializeWithContext(object ctx) {
            Ctx = ctx;
            OnCtxInitialized();
            OnStateInitialize();
        }

        /// <summary>
        /// Override this to handle context caching setup (like casting to your specific context type).
        /// </summary>
        protected virtual void OnCtxInitialized() { }

        protected virtual void OnStateInitialize() { }

        public void OnEnter() => EnterStateLogic();
        public void OnUpdate() => UpdateStateLogic();
        public void OnFixedUpdate() => FixedUpdateStateLogic();
        public void OnExit() => ExitStateLogic();

        /// <summary>
        /// Inheritors must override and implement their own logic.
        /// </summary>
        protected abstract void EnterStateLogic();

        protected abstract void UpdateStateLogic();
        protected abstract void FixedUpdateStateLogic();
        protected abstract void ExitStateLogic();
    }
}
