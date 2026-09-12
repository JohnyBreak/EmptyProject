using Core.Player.External;
using UnityEngine;

namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class FallState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;
        
        public FallState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = (PlayerSharedData)sharedData;
        }

        public override int Key()
        {
            return States.Fall;
        }

        protected override void OnEnterState()
        {
        }

        protected override void OnUpdateState()
        {
            HandleGravity();
        }
        
        protected override void ExitState()
        {
        }

        protected override void CheckSwitchState()
        {
            if (_sharedData.Controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Grounded));
            }
        }
        
        protected override void InitializeSubState()
        {
            if (_sharedData.InputReader.MovementInputDetected)
            {
                SetSubState(_factory.Get(States.FallMove));
                return;
            }
            SetSubState(_factory.Get(States.FallIdle));
        }
        
        private void HandleGravity()
        {
            float previousYVelocity = _sharedData.PreviousYVelocity;

            _sharedData.PreviousYVelocity += _sharedData.Config.Gravity * Time.deltaTime;
            float appliedY = Mathf.Max((previousYVelocity + _sharedData.PreviousYVelocity) * .5f,
                _sharedData.Config.MaxFallGravity);
            
            _sharedData.Velocity.SetYVelocity(appliedY);
        }
    }
}