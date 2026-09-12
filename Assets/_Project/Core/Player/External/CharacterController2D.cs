using System;
using UnityEngine;

namespace Core.Player.External
{
    public class CharacterController2D : MonoBehaviour
    {
        public event Action<GameObject> HeadCollisionObject;

        [SerializeField] private Collider2D _playerCollider;

        [SerializeField] private LayerMask _obstacleLayerMask;

        [SerializeField] private Vector2 _collisionCheckSize = Vector2.one;
        [SerializeField] private Vector3 _collisionCheckOffset;

        [SerializeField] private Vector2 _groundCheckSize = Vector2.one;
        [SerializeField] private Vector3 _groundCheckOffset;

        [SerializeField] private Vector2 _headCheckSize = Vector2.one;
        [SerializeField] private Vector3 _headCheckOffset;

        private Collider2D[] _headResults = new Collider2D[1];
        private bool _isGrounded;

        //[SerializeField] 
        private float _distanceToFloor = 0.51f;
        private Transform _transform;
        private Collider2D[] _groundResults = new Collider2D[1];
        private Vector2 _surfacePosition;

        private Vector3 _velocity;
        [SerializeField] private Vector3 _move;

        private GameObject _currentGroundObject;
        public GameObject CurrentGroundObject => _currentGroundObject;

        public Vector2 SurfacePosition => _surfacePosition;
        public bool IsGrounded => _isGrounded;

        private void Start()
        {
            _transform = transform;
        }
        
        public void Move(Vector3 moveVector)
        {
            _move = moveVector;

            _velocity += _move;
        }
        
        private void FinalMove()
        {
            _transform.position += _velocity * Time.deltaTime;
            Physics2D.SyncTransforms();
            _velocity = Vector3.zero;
        }

        private void Update()
        {
            CheckCollision();
            CheckGround();
        
            FinalMove();
            CheckHead();
            
            CheckCollision();
            CheckGround();
        }
        
        private void CheckCollision()
        {
            Vector2 point = _transform.position + _collisionCheckOffset;
            Vector2 size = new Vector2(_collisionCheckSize.x, _collisionCheckSize.y);

            var _collisionResults = Physics2D.OverlapBoxAll(point, size, 0, _obstacleLayerMask);

            foreach (Collider2D hit in _collisionResults)
            {
                if (hit == _playerCollider)
                    continue;
                ColliderDistance2D colliderDistance = hit.Distance(_playerCollider);

                if (colliderDistance.isOverlapped)
                {
                    Vector2 penetraitDirection = colliderDistance.pointA - colliderDistance.pointB;
                    _transform.position += (Vector3)penetraitDirection;
                    Physics2D.SyncTransforms();
                }
            }
        }

        private void CheckGround() 
        {
            Vector2 point = _transform.position + _groundCheckOffset;
            Vector2 size = new Vector2(_groundCheckSize.x, _groundCheckSize.y);

            if (Physics2D.OverlapBoxNonAlloc(point, size, 0, _groundResults, _obstacleLayerMask) > 0)
            {
                _isGrounded = true;
                _currentGroundObject = _groundResults[0].gameObject;
                _surfacePosition = Physics2D.ClosestPoint(_transform.position, _groundResults[0]);
            }
            else
            {
                _currentGroundObject = null;
                _isGrounded = false;
            }

            if (_velocity.y > 0)
            {
                _isGrounded = false;
            }

            if (_isGrounded && _velocity.y < 0) 
            {
                _transform.position = new Vector3(_transform.position.x, _surfacePosition.y + _distanceToFloor, _transform.position.z);
                Physics2D.SyncTransforms();
            }
        }

        private bool CheckHead()
        {
            Vector2 point = transform.position + _headCheckOffset;
            Vector2 size = new Vector2(_headCheckSize.x, _headCheckSize.y);

            if (Physics2D.OverlapBoxNonAlloc(point, size, 0, _headResults, _obstacleLayerMask) > 0)
            {
                HeadCollisionObject?.Invoke(_headResults[0].gameObject);
                return true;
            }

            return false;

        }

        public void IgnoreLayer(int layer, bool ignore = true) 
        {
            if (ignore)
            {
                _obstacleLayerMask -= layer;
            }
            else 
            {
                _obstacleLayerMask += layer;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position + _groundCheckOffset, _groundCheckSize);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + _collisionCheckOffset, _collisionCheckSize);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + _headCheckOffset, _headCheckSize);
            Debug.DrawRay(transform.position, Vector3.down * _distanceToFloor, Color.black);
        }
    }
}
