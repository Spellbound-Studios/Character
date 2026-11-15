// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Character.Body {
    public readonly struct MotionClaim {
        private readonly MotionClaims _claims;

        internal MotionClaim(MotionClaims claims, IMotionDriver driver) {
            _claims = claims;
            Driver = driver;
        }

        public IMotionDriver Driver { get; }

        public bool IsActive => _claims != null && Driver != null && ReferenceEquals(_claims.Active, Driver);

        public void Release() {
            _claims?.Release(in this);
        }
    }
}
