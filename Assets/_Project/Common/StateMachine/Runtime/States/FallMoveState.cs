namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class FallMoveState : BaseState
    {
        private readonly ISharedData _sharedData;
        
        public FallMoveState(
            StateMachine currentContext,
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
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