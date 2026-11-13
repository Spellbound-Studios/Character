// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Character.Rig {
    public interface IAnimationController {
        void CrossFade(int stateHash, int layer = 0, float fadeSeconds = 0f);

        void SetFloat(int parameterHash, float value);

        void SetLayerWeight(int layer, float weight);
    }
}
