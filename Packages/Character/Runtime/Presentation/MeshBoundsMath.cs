// Copyright (c) Spellbound Studios. All rights reserved.

using UnityEngine;

namespace Spellbound.Character.Presentation {
    public static class MeshBoundsMath {
        public static Bounds? FindSharedMeshBounds(GameObject gameObject) {
            if (gameObject == null)
                return null;

            var skinnedRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

            if (skinnedRenderer != null && skinnedRenderer.sharedMesh != null)
                return skinnedRenderer.sharedMesh.bounds;

            skinnedRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

            if (skinnedRenderer != null && skinnedRenderer.sharedMesh != null)
                return skinnedRenderer.sharedMesh.bounds;

            var meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshFilter != null && meshFilter.sharedMesh != null)
                return meshFilter.sharedMesh.bounds;

            meshFilter = gameObject.GetComponentInChildren<MeshFilter>();

            if (meshFilter != null && meshFilter.sharedMesh != null)
                return meshFilter.sharedMesh.bounds;

            return null;
        }
    }
}
