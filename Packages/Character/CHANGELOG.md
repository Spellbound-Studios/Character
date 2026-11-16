# Changelog

## [Unreleased]

- Containment seams in `Spellbound.Character.Body`: `ISupportProbe` with `PhysicsSupportProbe` casting on the body's own physics scene, `IContainmentSpace` with `PhysicsContainmentSpace`, `IRescueTarget` with `SettingsRescueTarget`, `IMoveSource`, `ContainmentReason`, `RescueRung`, `RescueStep` and the `ContainmentSettings` asset, whose `Validate` reports a stride, search, fall, hold, support reach or priority that cannot work. `CharacterObservations` declares the `character.support.held` and `character.containment.repair` guards and the `character.containment.rescue` occurrence.
- `SupportGate` holds a body kinematic, with no gravity, under a motion claim while its space is not ready around the body and the active driver needs support, releases it once the space is ready, and moves a body held past `supportHoldTimeoutSeconds` to the rescue target's fallback under the support guard.
- `RescueLadder` resolves a rescue to the last supported position, then open space above along up, then the rescue target's fallback; `Rescuer` moves the body there inside one fixed step under a motion claim at the settings' rescue priority and releases with a handoff at the target, so the resumed driver begins there.
- `Containment` checks an active body every `checkStride` fixed steps for a fall below the floor, an embedding and a sustained fall, remembers positions with ground within `supportReachMetres` under the body's colliders, probing with a ray from the bounds centre, rescues through the ladder under the repair guard and records each rescue; `LastRescues` keeps the last sixteen.
- `BodyContext` takes an `IMoveSource` for its desired move and up, and the Locomotion sample's character is its own move source.
- `Spellbound.Character.GeoForge`, compiled only when GeoForge is installed, gives `GeoForgeContainmentSpace`, which answers readiness from the volume's box readiness and classifies a body's bounds from the solidity of its corners.
- `ContainmentSettingsEditor` draws the containment settings as bound UI Toolkit fields and shows the floor height only while the floor is on; `ContainmentGizmos` draws each remembered rescue as a line to its target with a sphere coloured by rung and a label with the reason.
- The Locomotion gym's rescue cases: `GymContainmentSpace` with a settable readiness and solid boxes, a pit under the floor height, a solid block, and `SupportGate` and `Containment` on `LocomotionCharacter.prefab` with `GymContainmentSettings.asset`; `GymWorld` hands its space, a rescue target at the spawn marker and itself to both. Play-mode tests cover the pit, the embedded start, the held start, the hold timeout and two characters in one gym.
- The Swim And Fly sample: `SwimDriver` and `FlyDriver` over a `SampleMotionDriver` base, and `SwimAndFlyCharacter`, which claims flying while the fly input is held and swimming otherwise and carries `Containment` on its prefab. It needs Core and the Input System and compiles without StateMachine.
- `RigidbodyReferenceFrameSource.Move(target)` moves a kinematic body and reports the step's commanded linear and angular velocity in that step, so a rider no longer reads a kinematic frame one step behind its driver; a step with no `Move` reports the body's own velocity. `ReferenceFrameRider.World` set while the rider is enabled takes effect, and a rider that has no world logs the error once, at its first probe.
- Reference frame sources register into their world's directory: `ReferenceFrameSource` registers on enable and unregisters on disable, finding its world in `World`, then its serialized world component, then its parents, and logs an error and stays unregistered when it has no world or its id is refused. `ReferenceFrameSource.Id` is settable before enable, and a zero id becomes `ReferenceFrameIds.FromHierarchyPath(transform)`; setting `World` or `Id` while registered throws. `RigidbodyReferenceFrameSource` reads its rigidbody and `TransformReferenceFrameSource` gives finite-difference velocities over the fixed step.
- `RideableColliderCheck`, registered as `character.rideable-collider-without-frame` on `RidingSettings`, warns for every collider in a loaded scene that is on a rideable layer, is not a trigger and has no `ReferenceFrameSource` above it. `RidingSettings.Validate` warns `character.rideable-layers-empty` when no layer is rideable.

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
