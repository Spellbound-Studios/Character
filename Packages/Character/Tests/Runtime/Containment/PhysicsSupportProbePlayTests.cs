// Copyright (c) Spellbound Studios. All rights reserved.

using System.Collections;
using NUnit.Framework;
using Spellbound.Character.Body;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Spellbound.Character.Tests {
    public class PhysicsSupportProbePlayTests {
        private const float ProbeDistance = 2f;
        private const float ProbeRadius = 0.25f;

        private static readonly Vector3 BodyOrigin = new(0f, 1f, 0f);

        private GameObject _floor;
        private GameObject _body;
        private Scene _emptyScene;

        [SetUp]
        public void CreateFloorAndBody() {
            _floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _floor.name = "support probe floor";
            _floor.transform.localScale = new Vector3(4f, 0.2f, 4f);
            _body = new GameObject("support probe body");
            _body.transform.position = BodyOrigin;
        }

        [UnityTearDown]
        public IEnumerator DestroyFloorAndBody() {
            Object.Destroy(_floor);
            Object.Destroy(_body);
            if (_emptyScene.IsValid())
                yield return SceneManager.UnloadSceneAsync(_emptyScene);
        }

        [UnityTest]
        public IEnumerator Probe_HitsTheFloorInTheBodysScene() {
            yield return new WaitForFixedUpdate();

            ISupportProbe probe = new PhysicsSupportProbe();
            var result = probe.Probe(_body.scene.GetPhysicsScene(), BodyOrigin, Vector3.down, ProbeDistance,
                ProbeRadius, ~0);

            Assert.IsTrue(result.HasHit);
            Assert.AreEqual(0.9f, result.Distance, 0.01f);
            Assert.AreEqual(0.1f, result.Point.y, 0.01f);
            Assert.AreEqual(1f, result.Normal.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator Probe_MissesInAnEmptyScene() {
            _emptyScene = SceneManager.CreateScene("support probe empty scene",
                new CreateSceneParameters(LocalPhysicsMode.Physics3D));

            yield return new WaitForFixedUpdate();

            ISupportProbe probe = new PhysicsSupportProbe();
            var result = probe.Probe(_emptyScene.GetPhysicsScene(), BodyOrigin, Vector3.down, ProbeDistance,
                ProbeRadius, ~0);

            Assert.IsFalse(result.HasHit);
        }

        [UnityTest]
        public IEnumerator Probe_IgnoresLayersOutsideTheMask() {
            yield return new WaitForFixedUpdate();

            ISupportProbe probe = new PhysicsSupportProbe();
            var result = probe.Probe(_body.scene.GetPhysicsScene(), BodyOrigin, Vector3.down, ProbeDistance,
                ProbeRadius, ~(1 << _floor.layer));

            Assert.IsFalse(result.HasHit);
        }

        [UnityTest]
        public IEnumerator Probe_WithNoRadius_CastsARayToTheFloor() {
            yield return new WaitForFixedUpdate();

            ISupportProbe probe = new PhysicsSupportProbe();
            var result = probe.Probe(_body.scene.GetPhysicsScene(), BodyOrigin, Vector3.down, ProbeDistance, 0f,
                ~0);

            Assert.IsTrue(result.HasHit);
            Assert.AreEqual(0.9f, result.Distance, 0.01f);
            Assert.AreEqual(0.1f, result.Point.y, 0.01f);
            Assert.AreEqual(1f, result.Normal.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator Probe_WithNoRadius_IgnoresLayersOutsideTheMask() {
            yield return new WaitForFixedUpdate();

            ISupportProbe probe = new PhysicsSupportProbe();
            var result = probe.Probe(_body.scene.GetPhysicsScene(), BodyOrigin, Vector3.down, ProbeDistance, 0f,
                ~(1 << _floor.layer));

            Assert.IsFalse(result.HasHit);
        }
    }
}
