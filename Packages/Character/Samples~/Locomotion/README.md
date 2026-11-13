# Locomotion

This sample shows walk, run, jump and fall over StateMachine, on a character that carries the playable marker, and uses only the public surface. It is made for the Universal Render Pipeline.

Open `Gym/LocomotionGym.unity` and press Play. Move with WASD or the left stick, hold Left Shift or press the left stick to run, and press Space or the south button to jump. The gym has a flight of steps to walk up and a ledge to fall off.

`LocomotionCharacter.prefab` carries `LocomotionCharacter`, which builds a `LocomotionContext` (the bridge's `BodyContext` plus the run and jump requests, the animation seam and a way to change state), makes its machine with `CharacterStateMachineKit.Create`, and fills its own `StateRegistry` from `States/LocomotionStateCatalogue.asset`, leaving `StateMachineKit.Registry` to the game. Each fixed step it probes for support through the capsule and hands the result to the current state.

The four states are assets in `States`, each with its own speeds and timings in the inspector:

- `WalkState` moves the body at walking speed while it is supported, and goes to Run while run is held and the body is asked to move.
- `RunState` moves the body at running speed, and goes back to Walk when run is released or the move stops.
- Both go to Jump when jump is pressed, and to Fall when support is lost.
- `JumpState` lifts the body and goes to Fall once it stops rising.
- `FallState` steers the body in the air and goes to Walk when support returns.

Drag another state asset onto a slot of the character's configuration to change how it moves without touching code. The capsule is authored on the prefab, so a server builds the same one: its radius is overridden by hand to 0.3, and its height was written by `Spellbound/Character/Measure Capsule From Mesh` with the model's `Alpha_Surface` selected, which keeps an overridden value. A T-posed model's measured radius is half its arm span, so a prefab overrides the radius by hand and the operation writes only the height.

The body's `ReferenceFrameBinding` is the game's to feed: call `Bind(frame, worldPose, time)` when the character lands on something that moves, and `Sight(time)` each fixed step it stays on it. `LocomotionCharacter` calls `Update` each fixed step, and the states add the bound frame's velocity to the body's. The gym has no moving platform.

Input reaches the character through `ILocomotionInput`. The prefab uses `InputSystemLocomotionInput`, which reads `Locomotion.inputactions`; put any other component that implements `ILocomotionInput` on the character, or set `LocomotionCharacter.LocomotionInput`, to drive it from somewhere else.
