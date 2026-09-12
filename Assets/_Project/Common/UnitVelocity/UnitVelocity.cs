using UnityEngine;

namespace Common.UnitVelocity
{
    public class UnitVelocity
    {
        private Vector2 _velocity;
    
        public Vector2 GetVelocity()
        {
            return _velocity;
        }
    
        public void AddVelocity(Vector2 additionalVelocity)
        {
            _velocity += additionalVelocity;
        }

        public void SetVelocity(Vector2 newVelocity)
        {
            _velocity = newVelocity;
        }

        public void SetXVelocity(float newXVelocity)
        {
            _velocity = new Vector3(newXVelocity, _velocity.y);
        }
    
        public void ZeroXVelocity()
        {
            _velocity = new Vector3(0, _velocity.y);
        }
    
        public void ZeroVelocity()
        {
            _velocity = Vector3.zero;
        }
    
        public void ZeroYVelocity()
        {
            _velocity = new Vector3(_velocity.x, 0);
        }

        public void SetYVelocity(float appliedY)
        {
            _velocity = new Vector3(_velocity.x, appliedY);
        }
    }
}