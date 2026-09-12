using Common.UnitStateMachine.Runtime;
using Common.UnitVelocity;
using Core.InputSystem;
using UnityEngine;

namespace Core.Player.External
{
    public class PlayerSharedData : ISharedData
    {
        public float InitialJumpVelocity { get; private set; }
        public float PreviousYVelocity { get; set; }

        public readonly CharacterController2D Controller;
        public readonly UnitVelocity Velocity;
        public readonly PlayerConfig Config;
        public readonly InputReader InputReader;
        
        public PlayerSharedData( 
            CharacterController2D controller,
            PlayerConfig config,
            UnitVelocity unitVelocity,
            InputReader reader)
        {
            Controller = controller;
            Config = config;
            Velocity = unitVelocity;
            InputReader = reader;
            
            SetupVariables();
        }
        
        private void SetupVariables()
        {
            float timeToApex = Config.MaxJumpTime / 2;
            Config.JumpGravity = (-2 * Config.MaxJumpHeight) / Mathf.Pow(timeToApex, 2);
            InitialJumpVelocity = (2 * Config.MaxJumpHeight) / timeToApex;
        }
    }
}