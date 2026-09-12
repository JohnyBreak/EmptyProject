using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.InputSystem
{
    public class InputReader : Controls.IPlayerActions
    {
        public Vector2 _mouseDelta;
        public Vector2 MoveComposite;

        public float _movementInputDuration;
        public bool MovementInputDetected;

        public bool SprintDetected;

        private Controls _controls;

        public event Action JumpPerformedEvent;
        public event Action SprintActivatedEvent;
        public event Action SprintDeactivatedEvent;

        public void Enable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }

            _controls.Player.Enable();
        }

        public void Disable()
        {
            _controls.Player.Disable();
        }

        /// <summary>
        ///     Defines the action to perform when the OnLook callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnLook(InputAction.CallbackContext context)
        {
            _mouseDelta = context.ReadValue<Vector2>();
        }

        /// <summary>
        ///     Defines the action to perform when the OnMove callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnMove(InputAction.CallbackContext context)
        {
            MoveComposite = context.ReadValue<Vector2>();
            MovementInputDetected = MoveComposite.magnitude > 0;
        }

        /// <summary>
        ///     Defines the action to perform when the OnJump callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            JumpPerformedEvent?.Invoke();
        }

        /// <summary>
        ///     Defines the action to perform when the OnSprint callback is called.
        /// </summary>
        /// <param name="context">The context of the callback.</param>
        public void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                SprintDetected = true;
                SprintActivatedEvent?.Invoke();
            }
            else if (context.canceled)
            {
                SprintDetected = false;
                SprintDeactivatedEvent?.Invoke();
            }
        }
    }
}