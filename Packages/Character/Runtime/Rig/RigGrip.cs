// Copyright (c) Spellbound Studios. All rights reserved.

using System;

namespace Spellbound.Character.Rig {
    [Serializable]
    public sealed class RigGrip {
        private const string SocketPlaceholder = "{socket}";

        public string Id;
        public string NodeNamePattern = "grip_{socket}";
        public string SocketId;

        public string NodeName =>
                (NodeNamePattern ?? string.Empty).Replace(SocketPlaceholder, SocketId ?? string.Empty);
    }
}
