using Core.Player.External;

namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class MoveState : BaseState
    {
        private readonly PlayerSharedData _sharedData;

        public MoveState(
            StateMachine currentContext,
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = (PlayerSharedData)sharedData;
        }

        public override int Key()
        {
            return States.Move;
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

            _sharedData.Velocity.SetXVelocity(desiredVelocity);
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
            return (_sharedData.InputReader.SprintDetected) 
                ? _sharedData.Config.MoveSpeed * 2 
                : _sharedData.Config.MoveSpeed;
        }
    }
}