namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class FallState : BaseState, IRootState
    {
        private readonly ISharedData _sharedData;
        
        public FallState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Air;
        }

        protected override void OnEnterState()
        {
        }

        protected override void OnUpdateState()
        {
            HandleGravity();
        }

        private void HandleGravity()
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
        
        protected override void OnDispose()
        {
            base.OnDispose();
        }
    }
}