using Core.Player.External;

namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class FallMoveState : BaseState
    {
        private readonly PlayerSharedData _sharedData;
        
        public FallMoveState(
            StateMachine currentContext,
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = (PlayerSharedData)sharedData;
        }

        public override int Key()
        {
            return States.FallMove;
        }

        protected override void OnEnterState()
        {
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
        }

        protected override void ExitState()
        {
        }

        protected override void CheckSwitchState()
        {
            if (!_sharedData.InputReader.MovementInputDetected)
            {
                SwitchState(_factory.Get(States.Idle));
            }
        }

        protected override void InitializeSubState()
        {
        }
        
        private float GetSpeed() 
        {
            return _sharedData.Config.MoveSpeed;
        }
    }
}