using Common.UnitStateMachine.Runtime;
using Common.UnitStateMachine.Runtime.PlayerStates;
using Common.UnitVelocity;
using Core.InputSystem;
using UnityEngine;

namespace Core.Player.External
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private CharacterController2D _controller;
        private InputReader _inputReader;
        
        private StateMachine _stateMachine;
        private StateFactory _stateFactory;
        private PlayerSharedData _sharedData;
        private UnitVelocity _velocity = new UnitVelocity();
        private PlayerConfig _config = new PlayerConfig();
        
        private void Start()
        {
            _stateMachine = new StateMachine();
            _stateFactory = new StateFactory();
            _inputReader = new InputReader();
            
            _inputReader.Enable();
            
            _sharedData = new PlayerSharedData(
                _controller,
                _config,
                _velocity,
                _inputReader);
            
            GroundedState grounded = new GroundedState(
                _stateMachine,
                _stateFactory,
                _sharedData);
            
            MoveState move = new MoveState(
                _stateMachine,
                _stateFactory,
                _sharedData);
        
            IdleState idle = new IdleState(
                _stateMachine,
                _stateFactory,
                _sharedData);
            
            FallState fall = new FallState(
                _stateMachine,
                _stateFactory,
                _sharedData);
            
            FallMoveState fallMove = new FallMoveState(
                _stateMachine,
                _stateFactory,
                _sharedData);
        
            FallIdleState fallIdle = new FallIdleState(
                _stateMachine, 
                _stateFactory, 
                _sharedData);
            
            JumpState jump = new JumpState(
                _stateMachine,
                _stateFactory,
                _sharedData);
            
            _stateMachine.SetState(grounded);
            _stateMachine.Start();
        }

        private void Update()
        {
            _stateMachine.Tick();
            _controller.Move(_velocity.GetVelocity());
            //Debug.Log(_velocity.GetVelocity());
        }
        
        private void OnDestroy()
        {
            _inputReader?.Disable();
            _stateFactory?.Dispose();
        }
    }

}