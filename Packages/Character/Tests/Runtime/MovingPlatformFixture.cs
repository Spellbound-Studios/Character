// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using UnityEngine;

namespace Spellbound.Character.Tests {
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider), typeof(RigidbodyReferenceFrameSource))]
    public sealed class MovingPlatformFixture : MonoBehaviour {
        public static readonly Vector3 Size = new(4f, 0.2f, 4f);

        public Vector3 Velocity { get; set; }

        public Rigidbody Body { get; private set; }

        public RigidbodyReferenceFrameSource Source { get; private set; }

        public float TopHeight => Body.position.y + Size.y * 0.5f;

        public static MovingPlatformFixture Create(Transform parent, Vector3 position, Vector3 velocity) {
            var platform = new GameObject("Moving platform");
            platform.SetActive(false);
            platform.transform.SetParent(parent, false);
            platform.transform.position = position;

            var body = platform.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            platform.AddComponent<BoxCollider>().size = Size;
            var source = platform.AddComponent<RigidbodyReferenceFrameSource>();
            var fixture = platform.AddComponent<MovingPlatformFixture>();
            fixture.Body = body;
            fixture.Source = source;
            fixture.Velocity = velocity;
            platform.SetActive(true);

            return fixture;
        }

        private void FixedUpdate() {
            Body.MovePosition(Body.position + Velocity * Time.fixedDeltaTime);
        }
    }
}
