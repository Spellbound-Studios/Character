# Changelog

## [0.0.0] - 2026-11-12

### Added

- `Spellbound.Character.Presentation`, left out of server builds, with `MeshBoundsMath.FindSharedMeshBounds(gameObject)`.
- The StateMachine bridge `Spellbound.Character.StateMachine`, compiled only when the project holds com.spellboundstudios.statemachine: `BodyContext`, `BodyState` and `CharacterStateMachineKit.Create(context, configuration)`.
- The Locomotion sample: walk, run, jump and fall over StateMachine, with its gym scene and tests.
- The editor operation `Spellbound/Character/Measure Capsule From Mesh`, which writes a prefab's capsule in edit mode.

### Changed

- Controller is split: the state machine is the StateMachine package, and this package is Character, with the assemblies `Spellbound.Character`, `Spellbound.Character.Body`, `Spellbound.Character.Rig` and `Spellbound.Character.Cinemachine`.
- The package id is `com.spellboundstudios.character`.
- `ResizableCapsuleCollider.Initialize(GameObject)` is now `Initialize(GameObject, Bounds?)`: the caller passes the mesh bounds, or null for the fallback size.
- `IAnimationController.CrossFade` takes its fade in seconds, as `fadeSeconds`.

### Removed

- The debug HUD (`ControllerDebugging`, `DebugHudProfile`, `IDebuggingInfo`).
- The old demo content and its scene.
- The dependencies on Input System, Physics, Entities, Mathematics and Collections.
