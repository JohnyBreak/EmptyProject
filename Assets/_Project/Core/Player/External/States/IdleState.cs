using Core.Player.External;

namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class IdleState : BaseState
    {
        private readonly PlayerSharedData _sharedData;

        public IdleState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = (PlayerSharedData)sharedData;
        }

        public override int Key()
        {
            return States.Idle;
        }

        protected override void OnEnterState()
        {
            _sharedData.Velocity.ZeroXVelocity();
        }

        protected override void OnUpdateState()
        {
        }

        protected override void ExitState()
        {
        }

        protected override void CheckSwitchState()
        {
            if (_sharedData.InputReader.MovementInputDetected)
            {
                SwitchState(_factory.Get(States.Move));
            }
        }

        protected override void InitializeSubState()
        {
        }
    }
}