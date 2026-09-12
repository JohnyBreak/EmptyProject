namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class IdleState : BaseState
    {
        private readonly ISharedData _sharedData;

        public IdleState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Idle;
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