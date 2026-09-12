using Core.Player.External;
using UnityEngine;

namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class JumpState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;
        
        public JumpState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = (PlayerSharedData)sharedData;
        }

        public override int Key()
        {
            return States.Jump;
        }

        protected override void OnEnterState()
        {
            _sharedData.PreviousYVelocity = _sharedData.InitialJumpVelocity;
            _sharedData.Velocity.SetYVelocity(_sharedData.InitialJumpVelocity);
            _sharedData.Controller.HeadCollisionObject += OnHead;
        }

        private void OnHead(GameObject obj)
        {
            _sharedData.Controller.HeadCollisionObject -= OnHead;
            _sharedData.PreviousYVelocity = 0;
            _sharedData.Velocity.SetYVelocity(0);
        }

        protected override void OnUpdateState()
        {
            float moveInput = _sharedData.InputReader.MoveComposite.x;
            
            bool hasInput = moveInput != 0;
            
            float desiredVelocity = 0;
            
            if (hasInput)
            {
                desiredVelocity = moveInput * GetSpeed();
            }

            _sharedData.Velocity.SetXVelocity(desiredVelocity / 2f);

            //if (_sharedData.Velocity.GetVelocity().y <= 0)
            //{
            //    SwitchState(_factory.Get(States.Fall));
            //    return;
            //}
            
            HandleGravity();
        }
        
        private void HandleGravity()
        {
            float previousYVelocity = _sharedData.PreviousYVelocity;

            _sharedData.PreviousYVelocity += _sharedData.Config.JumpGravity * Time.deltaTime;
            float appliedY = Mathf.Max((previousYVelocity + _sharedData.PreviousYVelocity) * .5f,
                _sharedData.Config.MaxFallGravity);
            
            _sharedData.Velocity.SetYVelocity(appliedY);
        }
        
        protected override void ExitState()
        {
            _sharedData.Controller.HeadCollisionObject -= OnHead;
        }

        protected override void CheckSwitchState()
        {
            if (_sharedData.Velocity.GetVelocity().y <= 0 && _sharedData.Controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Grounded));
            }
        }

        protected override void InitializeSubState()
        {
        }
        
        private float GetSpeed()
        {
            return _sharedData.Config.MoveSpeed;
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _sharedData.Controller.HeadCollisionObject -= OnHead;
        }
    }
}