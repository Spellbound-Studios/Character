// Copyright 2025 Spellbound Studio Inc.

namespace Spellbound.Controller {
    /// <summary>
    /// The constant "driver" that points to and drives a ScriptableObject state variant.
    /// </summary>
    public class BaseStateDriver {
        protected BaseSoState CurrentStateVariant;
        protected bool IsActiveDriver;

        public BaseSoState CurrentVariant => CurrentStateVariant;
        public bool ActiveDriver => IsActiveDriver;

        /// <summary>
        /// Sets the current variant this driver should use.
        /// </summary>
        public virtual void ChangeVariant(BaseSoState newVariant) {
            if (CurrentStateVariant != null && IsActiveDriver) CurrentStateVariant.OnExit();

            CurrentStateVariant = newVariant;

            if (CurrentStateVariant != null && IsActiveDriver) CurrentStateVariant.OnEnter();
        }

        /// <summary>
        /// Called when this driver becomes the active state driver.
        /// </summary>
        public virtual void OnBecomeActive() {
            IsActiveDriver = true;
            CurrentStateVariant?.OnEnter();
        }

        /// <summary>
        /// Called when this driver is no longer the active state driver.
        /// </summary>
        public virtual void OnBecomeInactive() {
            CurrentStateVariant?.OnExit();
            IsActiveDriver = false;
        }

        /// <summary>
        /// Drives the current state variant's update logic.
        /// </summary>
        public virtual void DriveUpdate() {
            if (IsActiveDriver && CurrentStateVariant != null) CurrentStateVariant.OnUpdate();
        }

        /// <summary>
        /// Drives the current state variant's fixed update logic.
        /// </summary>
        public virtual void DriveFixedUpdate() {
            if (IsActiveDriver && CurrentStateVariant != null) CurrentStateVariant.OnFixedUpdate();
        }
    }
}