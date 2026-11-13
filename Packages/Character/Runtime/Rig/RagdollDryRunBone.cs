// Copyright (c) Spellbound Studios. All rights reserved.

namespace Spellbound.Character.Rig {
    public readonly struct RagdollDryRunBone {
        public RagdollDryRunBone(string name, string path, RagdollColliderShape shape, float massShare) {
            Name = name;
            Path = path;
            Shape = shape;
            MassShare = massShare;
        }

        public string Name { get; }

        public string Path { get; }

        public RagdollColliderShape Shape { get; }

        public float MassShare { get; }
    }
}
