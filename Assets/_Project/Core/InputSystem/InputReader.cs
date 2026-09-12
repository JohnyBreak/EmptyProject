using System;
using UnityEngine;

namespace Core.InputSystem
{
    public class InputReader
    {
        public bool MovementInputDetected;
        public Vector2 MoveComposite;
        public event Action JumpPerformedEvent;
        public bool SprintDetected { get; set; }

        public void Tick()
        {
            MoveComposite = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            MovementInputDetected = MoveComposite != Vector2.zero;
            SprintDetected = Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                JumpPerformedEvent?.Invoke();
            }
        }
    }
}