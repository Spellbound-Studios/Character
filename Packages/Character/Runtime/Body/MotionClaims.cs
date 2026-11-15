// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Character.Body {
    public sealed class MotionClaims {
        public IMotionDriver Active => null;

        public int Count => 0;

        public MotionClaim Claim(IMotionDriver driver) {
            if (driver == null)
                throw new ArgumentNullException(nameof(driver));

            throw new NotImplementedException();
        }

        internal void Release(in MotionClaim claim) {
            throw new NotImplementedException();
        }
    }
}
