namespace Common.UnitStateMachine.Runtime.PlayerStates
{
    public class GroundedState : BaseState, IRootState
    {
        private readonly ISharedData _sharedData;
        
        public GroundedState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            ISharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Grounded;
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
            //if (/*!_sharedData.Controller.IsGrounded*/)
            //{
            //    SwitchState(_factory.Get(States.Air));
            //}
        }

        protected override void InitializeSubState()
        {
            //if (/*_sharedData.InputReader._movementInputDetected*/)
            //{
            //    SetSubState(_factory.Get(States.Move));
            //    return;
            //}
            //SetSubState(_factory.Get(States.Idle));
        }

        protected override void OnDispose()
        {
            base.OnDispose();
        }
    }
}