// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Rig;
using UnityEngine;

namespace Spellbound.Character.Tests {
    public sealed class SocketTableHolder : MonoBehaviour {
        private SocketTable _sockets;

        public SocketTable Sockets => _sockets;

        public void Hold(SocketTable sockets) {
            _sockets = sockets;
        }
    }
}
