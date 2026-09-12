using Core.Player.External;

namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class FallIdleState: BaseState
    {
        private readonly PlayerSharedData _sharedData;

        public FallIdleState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = (PlayerSharedData)sharedData;
        }

        public override int Key()
        {
            return States.FallIdle;
        }

        protected override void OnEnterState()
        {
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
                SwitchState(_factory.Get(States.FallMove));
            }
        }

        protected override void InitializeSubState()
        {
        }
    }
}