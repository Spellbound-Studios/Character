// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using UnityEngine;

namespace Spellbound.Character.Rig {
    public readonly struct ResolvedSocket {
        private readonly Pose _defaultLocalPose;

        public ResolvedSocket(string id, Transform node) {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            Id = id;
            Node = node;
            Parent = node.parent;
            _defaultLocalPose = Pose.identity;
        }

        public ResolvedSocket(string id, Transform parent, Pose defaultLocalPose) {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            Id = id;
            Node = null;
            Parent = parent;
            _defaultLocalPose = defaultLocalPose;
        }

        public string Id { get; }

        public Transform Node { get; }

        public Transform Parent { get; }

        public bool HasNode => Node != null;

        public Pose LocalPose => HasNode ? new Pose(Node.localPosition, Node.localRotation) : _defaultLocalPose;

        public Pose PoseIn(Transform root) {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            Vector3 worldPosition;
            Quaternion worldRotation;

            if (HasNode) {
                worldPosition = Node.position;
                worldRotation = Node.rotation;
            }
            else {
                worldPosition = Parent.TransformPoint(_defaultLocalPose.position);
                worldRotation = Parent.rotation * _defaultLocalPose.rotation;
            }

            var rotation = Quaternion.Inverse(root.rotation) * worldRotation;

            return new Pose(root.InverseTransformPoint(worldPosition), rotation);
        }
    }
}
