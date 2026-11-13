// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections.Generic;
using Spellbound.Character.Rig;

namespace Spellbound.Character.Editor.Tests {
    public sealed class RecordingAnimationController : IAnimationController {
        private readonly Dictionary<int, float> _parameters = new();
        private readonly Dictionary<int, float> _layerWeights = new();
        private readonly Dictionary<int, int> _layerStates = new();

        public float LastFade { get; private set; }

        public int StateOn(int layer) => _layerStates.TryGetValue(layer, out var state) ? state : 0;

        public float Parameter(int parameterHash) =>
                _parameters.TryGetValue(parameterHash, out var value) ? value : 0f;

        public float LayerWeight(int layer) => _layerWeights.TryGetValue(layer, out var weight) ? weight : 0f;

        public void CrossFade(int stateHash, int layer = 0, float fadeSeconds = 0f) {
            _layerStates[layer] = stateHash;
            LastFade = fadeSeconds;
        }

        public void SetFloat(int parameterHash, float value) => _parameters[parameterHash] = value;

        public void SetLayerWeight(int layer, float weight) => _layerWeights[layer] = weight;
    }
}
