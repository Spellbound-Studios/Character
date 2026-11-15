// Copyright (c) Spellbound Studios. All rights reserved.

using Spellbound.Character.Body;
using Spellbound.Character.StateMachine;
using UnityEngine;

namespace Spellbound.Character.Samples.Locomotion {
    public abstract class LocomotionState : BodyState {
        [SerializeField, Min(0f), Tooltip("Seconds the animator takes to cross-fade into this state's animation.")]
        private float animationFade = 0.15f;

        private int _animatorStateHash;

        protected LocomotionContext Locomotion { get; private set; }

        protected abstract string AnimatorState { get; }

        protected Vector3 Up => Body.Transform.up;

        protected float PlanarSpeed {
            get {
                var rigidbody = Body.Rigidbody;
                var relative = rigidbody.linearVelocity - Body.ReferenceFrame.WorldVelocityAt(rigidbody.position);

                return Vector3.ProjectOnPlane(relative, Up).magnitude;
            }
        }

        protected float VerticalSpeed => VelocityMath.GetVerticalSpeed(Body.Rigidbody, Up);

        protected override void OnContextInitialized() => Locomotion = (LocomotionContext)Context;

        protected override void OnStateInitialize() => _animatorStateHash = Animator.StringToHash(AnimatorState);

        protected override void EnterStateLogic() =>
                Locomotion.Animation?.CrossFade(_animatorStateHash, 0, animationFade);

        protected override void UpdateStateLogic() { }

        protected override void ExitStateLogic() { }

        protected void MoveTowards(Vector3 direction, float speed, float acceleration) {
            var rigidbody = Body.Rigidbody;
            var up = Up;
            var planarVelocity = Vector3.ProjectOnPlane(rigidbody.linearVelocity, up);
            var worldVelocity = Body.ReferenceFrame.WorldVelocity(direction * speed, rigidbody.position);
            var targetVelocity = Vector3.ProjectOnPlane(worldVelocity, up);
            var nextVelocity = Vector3.MoveTowards(planarVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);

            rigidbody.AddForce(nextVelocity - planarVelocity, ForceMode.VelocityChange);
        }

        protected void TurnTowards(Vector3 direction, float turnSpeed) {
            var rigidbody = Body.Rigidbody;
            var up = Up;
            var carried = Body.ReferenceFrame.Turn(rigidbody.rotation, Time.fixedDeltaTime);
            var rotation = RotationMath.RotateTowardsDirection(
                carried, Vector3.ProjectOnPlane(direction, up), up, turnSpeed, Time.fixedDeltaTime);

            rigidbody.MoveRotation(rotation);
        }

        protected void HoldRideHeight() {
            var floatData = Body.Capsule.CapsuleFloatData;
            var force = FloatSpringMath.SolveFloatSpring(
                -Up, Body.Support.Distance, floatData.RideHeight, Body.Rigidbody.linearVelocity,
                floatData.SpringStrength, floatData.SpringDamper);

            Body.Rigidbody.AddForce(force, ForceMode.Acceleration);
        }
    }
}
