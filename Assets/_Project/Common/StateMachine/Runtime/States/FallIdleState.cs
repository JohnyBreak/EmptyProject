namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class FallIdleState: BaseState
    {
        private readonly ISharedData _sharedData;

        public FallIdleState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
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
           
        }

        protected override void InitializeSubState()
        {
        }
    }
}