using Core.Player.External;
using UnityEngine;

namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class GroundedState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;
        
        public GroundedState(
            StateMachine currentContext,
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = (PlayerSharedData)sharedData;
        }

        public override int Key()
        {
            return States.Grounded;
        }

        protected override void OnEnterState()
        {
            _sharedData.Velocity.SetVelocity(Vector2.up * _sharedData.Config.GroundedGravity);
            
            _sharedData.InputReader.JumpPerformedEvent += OnJump;
        }

        protected override void OnUpdateState()
        {
        }

        protected override void ExitState()
        {
            _sharedData.InputReader.JumpPerformedEvent -= OnJump;
        }

        protected override void CheckSwitchState()
        {
            if (!_sharedData.Controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Fall));
            }
        }

        protected override void InitializeSubState()
        {
            if (_sharedData.InputReader.MovementInputDetected)
            {
                SetSubState(_factory.Get(States.Move));
                return;
            }
            SetSubState(_factory.Get(States.Idle));
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _sharedData.InputReader.JumpPerformedEvent -= OnJump;
        }
        
        private void OnJump()
        {
            SwitchState(_factory.Get(States.Jump));
        }
    }
}