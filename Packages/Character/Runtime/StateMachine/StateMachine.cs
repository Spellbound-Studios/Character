// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// An enum-keyed state machine whose slots are each filled by a swappable state SO.
    /// </summary>
    /// <typeparam name="TContext">The object the states act on — usually the owning controller.</typeparam>
    /// <typeparam name="TStateEnum">The enum of state slots; one driver is created per value.</typeparam>
    public sealed class StateMachine<TContext, TStateEnum> : IStateMachine
            where TContext : class where TStateEnum : Enum {
        /// <summary>
        /// The driver of the currently-active slot.
        /// </summary>
        public BaseStateDriver CurrentActiveDriver { get; private set; }

        /// <summary>
        /// The context passed to every state.
        /// </summary>
        public TContext ctx { get; private set; }

        /// <summary>
        /// The currently-active slot.
        /// </summary>
        public TStateEnum CurrentStateType { get; private set; }

        private readonly Dictionary<TStateEnum, BaseStateDriver> _stateDrivers;
        private readonly Dictionary<Type, BaseSoState> _statesByType = new();
        private readonly Dictionary<TStateEnum, BaseSoState> _defaultVariants = new();

        public StateMachine(TContext ctx) {
            this.ctx = ctx;
            _stateDrivers = new Dictionary<TStateEnum, BaseStateDriver>();

            InitializeStateDrivers();
        }

        #region Configuration

        /// <summary>
        /// Wires the machine from a config asset and enters its initial state.
        /// </summary>
        public void Configure(IStateMachineConfig<TStateEnum> config) {
            if (config == null) {
                Log.Error("Configure called with a null config; the state machine has no states.");

                return;
            }

            ConfigureCore(config.Bindings, config.InitialState);
        }

        /// <summary>
        /// Wires the machine from an inline binding list and enters the initial state.
        /// </summary>
        public void Configure<TBinding>(List<TBinding> bindings, TStateEnum initialState)
                where TBinding : class, IStateBinding<TStateEnum> =>
                ConfigureCore(bindings, initialState);

        /// <summary>
        /// Sets a slot's default variant and makes it the slot's current variant.
        /// </summary>
        public void SetInitialVariant(TStateEnum stateType, BaseSoState initialVariant) {
            if (!_stateDrivers.TryGetValue(stateType, out var driver)) {
                Log.Error($"No state driver found for state type: {stateType}");

                return;
            }

            if (initialVariant == null)
                return;

            var instance = RegisterState(initialVariant);
            _defaultVariants[stateType] = instance;
            driver.ChangeVariant(instance);
        }

        /// <summary>
        /// Returns this machine's own instance of a state, cloning the shared asset on first request — so the
        /// asset is never mutated and every controller runs its own copy. One instance per concrete type.
        /// </summary>
        public BaseSoState RegisterState(BaseSoState source) {
            if (source == null) {
                Log.Error("Attempted to register a null state.");

                return null;
            }

            var stateType = source.GetType();

            if (_statesByType.TryGetValue(stateType, out var existing))
                return existing;

            var instance = UnityEngine.Object.Instantiate(source);
            instance.name = source.name;
            _statesByType[stateType] = instance;
            instance.InitializeWithContext(ctx);

            return instance;
        }

        /// <summary>
        /// Replaces the driver for a slot. Advanced use only.
        /// </summary>
        public void RegisterStateDriver(TStateEnum stateType, BaseStateDriver stateDriver) {
            if (_stateDrivers.ContainsKey(stateType))
                Log.Warn($"State driver for {stateType} is already registered. Overwriting.");

            _stateDrivers[stateType] = stateDriver;
        }

        #endregion

        #region Transitions & Variants

        /// <summary>
        /// Activates a different slot.
        /// </summary>
        public void ChangeState(TStateEnum newStateType) {
            if (!_stateDrivers.TryGetValue(newStateType, out var newDriver)) {
                Log.Error($"No state driver registered for state type: {newStateType}");

                return;
            }

            if (CurrentActiveDriver == newDriver)
                return;

            CurrentActiveDriver?.OnBecomeInactive();
            CurrentActiveDriver = newDriver;
            CurrentStateType = newStateType;
            CurrentActiveDriver.OnBecomeActive();
        }

        /// <summary>
        /// Swaps the variant filling a slot, taking effect immediately if the slot is active.
        /// </summary>
        public void ApplyVariant(TStateEnum slot, BaseSoState variant) {
            if (!_stateDrivers.TryGetValue(slot, out var driver)) {
                Log.Error($"No state driver registered for state type: {slot}");

                return;
            }

            if (variant == null) {
                Log.Error($"ApplyVariant called with a null variant for slot {slot}; ignoring.");

                return;
            }

            driver.ChangeVariant(RegisterState(variant));
        }

        /// <summary>
        /// Swaps a slot to its registered variant of type <typeparamref name="TVariant"/>.
        /// </summary>
        public void ApplyVariant<TVariant>(TStateEnum slot) where TVariant : BaseSoState {
            var variant = GetState<TVariant>();

            if (variant == null) {
                Log.Error($"No registered variant of type {typeof(TVariant).Name}; list it in the config or " +
                          "apply it by reference first.");

                return;
            }

            ApplyVariant(slot, variant);
        }

        /// <summary>
        /// Restores a slot to the default variant recorded at configure time.
        /// </summary>
        public void RestoreDefault(TStateEnum slot) {
            if (!_stateDrivers.TryGetValue(slot, out var driver)) {
                Log.Error($"No state driver registered for state type: {slot}");

                return;
            }

            if (_defaultVariants.TryGetValue(slot, out var def) && def != null) {
                driver.ChangeVariant(def);

                return;
            }

            Log.Error($"No default variant recorded for slot {slot}; was the machine configured?");
        }

        #endregion

        #region Queries

        /// <summary>
        /// The active slot's current variant, or null.
        /// </summary>
        public BaseSoState CurrentState => GetCurrentRunningState();

        /// <summary>
        /// The active variant's stable hash, or 0 if none.
        /// </summary>
        public uint CurrentStateHash => GetCurrentRunningState()?.Hash ?? 0u;

        /// <summary>
        /// True if the active variant is a <typeparamref name="TVariant"/> (or a subtype of it).
        /// </summary>
        public bool IsInState<TVariant>() where TVariant : BaseSoState => GetCurrentRunningState() is TVariant;

        /// <summary>
        /// True if the active variant is exactly the asset with this hash.
        /// </summary>
        public bool IsInState(uint stateHash) => GetCurrentRunningState() is { } state && state.Hash == stateHash;

        /// <summary>
        /// Returns this machine's instance of type <typeparamref name="TVariant"/>, or null.
        /// </summary>
        public TVariant GetState<TVariant>() where TVariant : BaseSoState =>
                _statesByType.GetValueOrDefault(typeof(TVariant)) as TVariant;

        /// <summary>
        /// Returns the variant currently filling a slot.
        /// </summary>
        public BaseSoState GetCurrentVariant(TStateEnum stateType) =>
                _stateDrivers.TryGetValue(stateType, out var driver)
                        ? driver.CurrentVariant
                        : null;

        /// <summary>
        /// Returns the active slot's current variant.
        /// </summary>
        public BaseSoState GetCurrentRunningState() => CurrentActiveDriver?.CurrentVariant;

        #endregion

        #region Tick

        /// <summary>
        /// Drives the active state's update; call from MonoBehaviour.Update.
        /// </summary>
        public void UpdateStateMachine() => CurrentActiveDriver?.DriveUpdate();

        /// <summary>
        /// Drives the active state's fixed update; call from MonoBehaviour.FixedUpdate.
        /// </summary>
        public void FixedUpdateStateMachine() => CurrentActiveDriver?.DriveFixedUpdate();

        #endregion

        #region Teardown

        /// <summary>
        /// Exits the active state and destroys this machine's cloned state instances. Call from the owner's
        /// OnDestroy so the per-instance states don't leak.
        /// </summary>
        public void Dispose() {
            CurrentActiveDriver?.OnBecomeInactive();
            CurrentActiveDriver = null;

            foreach (var instance in _statesByType.Values)
                if (instance != null)
                    UnityEngine.Object.Destroy(instance);

            _statesByType.Clear();
            _defaultVariants.Clear();
        }

        #endregion

        #region Internal

        private void InitializeStateDrivers() {
            foreach (TStateEnum stateType in Enum.GetValues(typeof(TStateEnum)))
                _stateDrivers[stateType] = new BaseStateDriver();
        }

        private void ConfigureCore(IReadOnlyList<IStateBinding<TStateEnum>> bindings, TStateEnum initialState) {
            if (bindings == null) {
                Log.Error("Configure called with null bindings; the state machine has no states.");

                return;
            }

            var slotSeen = new HashSet<TStateEnum>();

            for (var i = 0; i < bindings.Count; i++) {
                var binding = bindings[i];
                var source = binding?.Variant;

                if (source == null) {
                    Log.Warn($"State binding at index {i} has a null variant; skipping.");

                    continue;
                }

                RegisterState(source);

                if (slotSeen.Add(binding.Slot))
                    SetInitialVariant(binding.Slot, source);
            }

            ChangeState(initialState);
        }

        #endregion
    }
}
