# Spellbound.Character

A character's body and rig: the capsule, the support probe, the movement maths, the camera seam, the reference frame binding and the playable marker, then the animation seam, the rig contract and the ragdoll builder.

## Install

Add this line to `Packages/manifest.json`, replacing `<version>` with a release tag:

```
"com.spellboundstudios.character": "https://github.com/Spellbound-Studios/Character.git?path=/Packages/Character#v<version>"
```

The Spellbound window in Core installs it for you.

## Front door

`CharacterKit` is the entry point to everything this package offers. `CharacterKit.Body` reaches the body, whose `ReferenceFrameBinding` the game drives: it calls `Bind` when the body gains a frame and `Sight` on every later sighting, and `Update` releases the frame after `HoldSeconds` without one. `CharacterKit.Rig` reaches the rig.

## Cinemachine

The assembly `Spellbound.Character.Cinemachine` holds the camera rig over Cinemachine and compiles only when the project holds com.unity.cinemachine 3.1.6 or later below 4.0.0.

## StateMachine

The assembly `Spellbound.Character.StateMachine` gives StateMachine a body to drive and compiles only when the project holds com.spellboundstudios.statemachine. Its door is `CharacterStateMachineKit.Create(context, configuration)`, which makes a machine over a `BodyContext` from a `StateMachineConfiguration<TStateKey>`. The Locomotion sample builds walk, run, jump and fall on it.

## Rules

The binding rules are the line text at https://github.com/Spellbound-Studios/.github/tree/main/line
