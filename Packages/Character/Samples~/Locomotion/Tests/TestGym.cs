// Copyright (c) Spellbound Studios. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Spellbound.StateMachine;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spellbound.Character.Samples.Locomotion.Tests {
    public sealed class TestGym : IDisposable {
        private readonly List<Object> _created = new();

        public TestGym() {
            Ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Ground.name = "ground";
            Ground.transform.position = new Vector3(0f, -0.5f, 0f);
            Ground.transform.localScale = new Vector3(40f, 1f, 40f);
            _created.Add(Ground);
        }

        public GameObject Ground { get; }

        public LocomotionCharacter CreateCharacter(ILocomotionInput input) => Build(null, input);

        public LocomotionCharacter CreateCharacter<TInput>() where TInput : Component, ILocomotionInput =>
                Build(typeof(TInput), null);

        public LocomotionCharacter CreateCharacterFromPrefab(LocomotionCharacter prefab, ILocomotionInput input) {
            var character = Object.Instantiate(prefab, new Vector3(0f, 0.05f, 0f), Quaternion.identity);
            _created.Add(character.gameObject);
            character.LocomotionInput = input;

            return character;
        }

        public static LocomotionCharacter FindImportedPrefab() {
#if UNITY_EDITOR
            var guids = UnityEditor.AssetDatabase.FindAssets("LocomotionCharacter t:Prefab", new[] { "Assets" });

            foreach (var guid in guids) {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                if (path.EndsWith("/Locomotion/LocomotionCharacter.prefab"))
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<LocomotionCharacter>(path);
            }
#endif
            return null;
        }

        public static IEnumerator WaitFixedSteps(int steps, Func<bool> until = null) {
            for (var step = 0; step < steps; step++) {
                if (until != null && until())
                    yield break;

                yield return new WaitForFixedUpdate();
            }
        }

        public static string Describe(string expected, LocomotionCharacter character) {
            var current = character.Machine.CurrentState;
            var name = current != null ? current.GetType().Name : "no state";

            return $"{expected} was expected, but the machine is in {name}.";
        }

        public void Dispose() {
            for (var index = _created.Count - 1; index >= 0; index--) {
                if (_created[index] != null)
                    Object.DestroyImmediate(_created[index]);
            }

            _created.Clear();
        }

        private LocomotionCharacter Build(Type inputComponent, ILocomotionInput input) {
            var body = new GameObject("character");
            body.SetActive(false);
            body.transform.position = new Vector3(0f, 0.05f, 0f);
            _created.Add(body);

            if (inputComponent != null)
                body.AddComponent(inputComponent);

            var character = body.AddComponent<LocomotionCharacter>();
            character.LocomotionInput = input;
            character.Configuration = new StateMachineConfiguration<LocomotionStateKey>(
                LocomotionStateKey.Fall,
                Bind<WalkState>(LocomotionStateKey.Walk),
                Bind<RunState>(LocomotionStateKey.Run),
                Bind<JumpState>(LocomotionStateKey.Jump),
                Bind<FallState>(LocomotionStateKey.Fall));

            body.SetActive(true);

            return character;
        }

        private StateBinding<LocomotionStateKey> Bind<TState>(LocomotionStateKey slot) where TState : State {
            var state = ScriptableObject.CreateInstance<TState>();
            _created.Add(state);

            return new StateBinding<LocomotionStateKey>(slot, state);
        }
    }
}
