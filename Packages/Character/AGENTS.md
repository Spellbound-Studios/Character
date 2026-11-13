# Spellbound.Character

## Purpose

Gives a game a character's body and rig: the capsule, the support probe, the movement maths, the camera seam, the reference frame binding and the playable marker, then the animation seam, the rig contract and the ragdoll builder.

## Front door

`CharacterKit`. `CharacterKit.Body` holds the body's doors, such as `CharacterKit.Body.IsPlayable(prefab)`, which answers whether a prefab's root carries a component implementing `IPlayableCharacter`; `CharacterKit.Rig` holds the rig's, such as `CharacterKit.Rig.BuildRagdoll(rigRoot, animator, profile, output, out error)`, which adds the bodies, colliders and joints a `RagdollProfile` names to a hierarchy, in edit mode or play mode, and returns false with the error instead of throwing. The rig contract, `RigContract` in `Spellbound.Character.Rig`, holds what an artist's model must carry: the rig kind (`RigKind.Humanoid` or `Generic`, mapped to and from Unity's `ModelImporterAnimationType` by `RigKindMapping` in `Spellbound.Character.Editor`), the required bones (human bones for Humanoid, bone paths for Generic), the sockets, grips and piece slots, the hips (bone, height above the ground root and facing axis), the ragdoll profile, the height range in metres, and the reference figure and poses. The reference figure is a playable prefab: `Validate(report)` checks it through the playable marker, `PlayableCharacters.IsPlayable`, the function `CharacterKit.Body.IsPlayable` forwards to, and reports `character.reference-figure-not-playable` when a figure is set and is not playable; an empty figure is no finding, and every other check still runs. `Validate` also reports empty and repeated socket, grip and piece ids, a socket with no parent bone, a grip whose socket is not in the contract, an empty hips bone, a hips height that is not a finite number above zero, a facing axis that is not a direction, a height range whose bounds are not finite numbers above zero or whose minimum exceeds its maximum, and an invalid ragdoll profile, each an error, and warns with `character.required-bones-empty` when the kind's required bones are empty. `ContentHash` is the contract's change marker: it hashes every field in order and moves when any field changes; the figure, the poses and the ragdoll profile enter by name.

Beneath it: `RagdollBuilder.IgnoreSelfCollision(colliders)` turns off collision between every pair of a ragdoll's colliders; `AnimatorAnimationController` is the default `IAnimationController`, over a `UnityEngine.Animator`, and cross-fades over `fadeSeconds` through `Animator.CrossFadeInFixedTime`; `InterpolationMath` holds the scalar helpers `LerpWithSpeed`, `SmoothDampFloat` and `RemapValue`.

StateMachine, in the bridge `Spellbound.Character.StateMachine`, which compiles only when the project holds com.spellboundstudios.statemachine: `BodyContext` carries a body's transform, rigidbody, capsule, latest support probe, reference frame binding, desired move and the elapsed fixed time the binding takes, and `BodyState` is the abstract `State` whose `Body` is that context. A body's machine is configured with StateMachine's own `StateBinding<TStateKey>` and `StateMachineConfiguration<TStateKey>` and made with `CharacterStateMachineKit.Create(context, configuration)`, which forwards to `StateMachineKit.Create`. The Locomotion sample builds walk, run, jump and fall on them.

Presentation, in `Spellbound.Character.Presentation`, which compiles out on a server build: `MeshBoundsMath.FindSharedMeshBounds(gameObject)` gives the mesh bounds `ResizableCapsuleCollider.Initialize` sizes the capsule from. The editor operation `Spellbound/Character/Measure Capsule From Mesh`, in `Spellbound.Character.Presentation.Editor`, measures a selected object of a prefab through it and writes the capsule into the prefab.

<!-- spellbound:samples 3a8448c2 -->
Samples in `Samples~`, imported from the Package Manager's Samples tab:
- Locomotion: Walk, run, jump and fall over StateMachine, on a character that carries the playable marker. (`Samples~/Locomotion`)
<!-- /spellbound:samples -->

<!-- spellbound:depends 8515c256 -->
Depends on:
- `com.spellboundstudios.core` in [1.1.9,3.0.0)
Bridges, each compiled only when its package is installed:
- `Spellbound.Character.Cinemachine` when `com.unity.cinemachine` is in [3.1.6,4.0.0)
- `Spellbound.Character.StateMachine` when `com.spellboundstudios.statemachine` is in [0.0.0,2.0.0)
<!-- /spellbound:depends -->

<!-- spellbound:doors 845eb3c7 -->
Bridge doors in this project, the constructors of the types a bridge assembly adds:
- `CharacterCinemachineKit` in `Spellbound.Character.Cinemachine`
  - `static ICameraRig Rig(GameObject root)`
- `CharacterStateMachineKit` in `Spellbound.Character.StateMachine`
  - `static StateMachine<BodyContext, TStateKey> Create<TStateKey>(BodyContext context, IStateMachineConfiguration<TStateKey> configuration)`
<!-- /spellbound:doors -->

## Rules

The binding rules are the line text at https://github.com/Spellbound-Studios/.github/tree/main/line

- Body depends on Core and the engine only; Rig on those and Body.
- Cinemachine is reached only through `Spellbound.Character.Cinemachine`, which compiles when the project holds com.unity.cinemachine 3.1.6 or later below 4.0.0, and never on a server build.
- Log through `Spellbound.Core.Logging.Log`, never through `Debug.Log`.
- Code under a namespace `Spellbound.Character.*` names StateMachine's machine `global::Spellbound.StateMachine.StateMachine<TContext, TStateKey>`, because the bridge's namespace `Spellbound.Character.StateMachine` hides the bare name there.
- `ReferenceFrameBinding` reads no clock: the caller passes the time to `Bind`, `Sight` and `Update`, and the frame is released once its last sighting is older than `HoldSeconds`, or when `Update` is given a time earlier than the last sighting. `Bind` takes the body's world pose, so `LocalPose`, turned back into world space by `ReferenceFramePose.ToWorld`, keeps the body's facing. `WorldVelocityAt(point)` is the frame's own `VelocityAt(point)`. `HoldSeconds` reads a value that is not a finite non-negative number, set in code or serialized, as 0.
- To size the capsule from the character's mesh, pass `MeshBoundsMath.FindSharedMeshBounds(gameObject)` to `ResizableCapsuleCollider.Initialize(gameObject, meshBounds)`. It reads `sharedMesh.bounds` of the first `SkinnedMeshRenderer` on the object, then in its children, then of the first `MeshFilter` on the object, then in its children, and gives null when none has a mesh. `Renderer.bounds` is in world space, scaled and rotated, and sizes a different capsule. The renderer search is presentation only: it compiles out on a server build, where `Initialize` is given null.
- A capsule the server must share with clients is authored in edit mode: select the mesh's object in a prefab asset or in Prefab Mode and run `Spellbound/Character/Measure Capsule From Mesh`. It measures the selected object through `MeshBoundsMath.FindSharedMeshBounds`, as the runtime search would, sizes the capsule exactly as `DefaultColliderData.Initialize` does, so a height or radius already overridden is kept, and writes the rest with its override on into the nearest object at or above the selection whose component carries a `ResizableCapsuleCollider`; it records the change for Undo and saves the prefab. A T-posed model's measured radius is half its arm span, so such a prefab overrides the radius by hand (the Locomotion sample's prefab does, 0.3) and the operation writes only the height. A server then passes null to `Initialize` and gets the same capsule as a client. The operation lives in `Spellbound.Character.Presentation.Editor` and is absent under a Dedicated Server build profile, so a capsule is measured with a client profile active.
- `RagdollBuilder.Build` resolves every bone and checks every bone's collider before it changes the hierarchy: an unknown shape, or a bone that already holds a collider of another shape than the profile names, is an error. It then adds the components to every bone, and only after every bone has them sets the masses, collider sizes, joints and physics settings. A failed build removes only the components it added and leaves authored components and their values as they were.
- `RagdollPhysicsSettings.IgnoreSelfCollision` is applied by `RagdollBuilder.Build` in play mode only, because `Physics.IgnoreCollision` is not saved with the scene. A ragdoll built in edit mode applies it at runtime through `RagdollBuilder.IgnoreSelfCollision(colliders)`.

## Not for

- Concrete states such as walk, run, jump and fall: those are the Locomotion sample's, over StateMachine.
- Input: no assembly of the package reads a device.
- A game's numbers. A value a game tunes is a field with a default.

## Gaps

- The bridge's versionDefines range on com.spellboundstudios.statemachine is [0.0.0,2.0.0) because StateMachine is 0.0.0 until its first tag; the range narrows to start at that tag.

## Ledger

The waivers and answers of this package are in the dev project's `Assets/_Project/Ledger/Character.asset`.
