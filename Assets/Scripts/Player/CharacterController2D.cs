using System;
using UnityEngine;

namespace LastGod.Player
{
    /// <summary>
    /// Reusable 2D physics character controller providing grounded movement, variable jump height, coyote time, and buffering.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public abstract class CharacterController2D : MonoBehaviour
    {
        [Header("Locomotion Settings")]
        [SerializeField] protected float walkSpeed = 3.5f;
        [SerializeField] protected float runSpeed = 5.5f;
        [SerializeField] protected float acceleration = 45f;
        [SerializeField] protected float deceleration = 60f;

        [Header("Jump Settings")]
        [SerializeField] protected float jumpHeight = 2.0f;
        [SerializeField] protected float timeToJumpApex = 0.35f;
        [SerializeField] protected float coyoteTimeDuration = 0.12f;
        [SerializeField] protected float jumpBufferDuration = 0.12f;
        [SerializeField] protected float lowJumpMultiplier = 2.5f;
        [SerializeField] protected float fallGravityMultiplier = 1.8f;

        [Header("Collision and Grounding")]
        [SerializeField] protected LayerMask groundLayer;
        [SerializeField] protected Transform groundCheckPoint;
        [SerializeField] protected Vector2 groundCheckSize = new Vector2(0.45f, 0.1f);

        // Core Components
        protected Rigidbody2D _rb;
        protected Collider2D _col;

        // Locomotion State
        protected float _moveInputX;
        protected bool _isRunHeld;
        protected bool _isGrounded;
        protected bool _wasGrounded;
        protected int _facingDirection = 1; // 1 = Right, -1 = Left

        // Jump Dynamics
        protected float _gravity;
        protected float _jumpVelocity;
        protected float _coyoteTimeCounter;
        protected float _jumpBufferCounter;
        protected bool _isJumpHeld;

        public bool IsGrounded => _isGrounded;
        public int FacingDirection => _facingDirection;
        public float HorizontalVelocity => _rb != null ? _rb.linearVelocity.x : 0f;
        public float VerticalVelocity => _rb != null ? _rb.linearVelocity.y : 0f;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            CalculateGravityAndJump();
        }

        protected virtual void Start()
        {
            if (groundLayer == 0)
            {
                groundLayer = LayerMask.GetMask("Ground", "Default");
            }
        }

        protected virtual void CalculateGravityAndJump()
        {
            _gravity = -(2f * jumpHeight) / (timeToJumpApex * timeToJumpApex);
            _jumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
            if (_rb != null)
            {
                _rb.gravityScale = _gravity / Physics2D.gravity.y;
            }
        }

        protected virtual void Update()
        {
            CheckGroundStatus();
            HandleTimers();
        }

        protected virtual void FixedUpdate()
        {
            ApplyHorizontalMovement();
            ApplyVerticalPhysics();
        }

        protected virtual void CheckGroundStatus()
        {
            _wasGrounded = _isGrounded;
            Vector2 checkPos = groundCheckPoint != null 
                ? (Vector2)groundCheckPoint.position 
                : (Vector2)transform.position + Vector2.down * 0.05f;

            Collider2D hit = Physics2D.OverlapBox(checkPos, groundCheckSize, 0f, groundLayer);
            _isGrounded = hit != null;

            if (_isGrounded)
            {
                _coyoteTimeCounter = coyoteTimeDuration;
            }
            else
            {
                _coyoteTimeCounter -= Time.deltaTime;
            }
        }

        protected virtual void HandleTimers()
        {
            if (_jumpBufferCounter > 0)
            {
                _jumpBufferCounter -= Time.deltaTime;
            }
        }

        public virtual void SetMoveInput(float inputX, bool runHeld = false)
        {
            _moveInputX = Mathf.Clamp(inputX, -1f, 1f);
            _isRunHeld = runHeld;

            if (_moveInputX > 0.05f && _facingDirection != 1)
            {
                FlipFacing(1);
            }
            else if (_moveInputX < -0.05f && _facingDirection != -1)
            {
                FlipFacing(-1);
            }
        }

        public virtual void BufferJump(bool isHeld)
        {
            _jumpBufferCounter = jumpBufferDuration;
            _isJumpHeld = isHeld;
        }

        public virtual void ReleaseJump()
        {
            _isJumpHeld = false;
        }

        protected virtual void ApplyHorizontalMovement()
        {
            float targetSpeed = _moveInputX * (_isRunHeld ? runSpeed : walkSpeed);
            float currentSpeedX = _rb.linearVelocity.x;
            float rate = Mathf.Abs(_moveInputX) > 0.05f ? acceleration : deceleration;

            float newSpeedX = Mathf.MoveTowards(currentSpeedX, targetSpeed, rate * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(newSpeedX, _rb.linearVelocity.y);
        }

        protected virtual void ApplyVerticalPhysics()
        {
            if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
            {
                ExecuteJump();
            }

            if (_rb.linearVelocity.y > 0 && !_isJumpHeld)
            {
                _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime);
            }
            else if (_rb.linearVelocity.y < 0)
            {
                _rb.linearVelocity += Vector2.up * (Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.fixedDeltaTime);
            }
        }

        protected virtual void ExecuteJump()
        {
            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpVelocity);
            OnJumpExecuted();
        }

        protected virtual void OnJumpExecuted() { }

        protected virtual void FlipFacing(int direction)
        {
            _facingDirection = direction;
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * direction;
            transform.localScale = s;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Vector2 checkPos = groundCheckPoint != null 
                ? (Vector2)groundCheckPoint.position 
                : (Vector2)transform.position + Vector2.down * 0.05f;
            Gizmos.DrawWireCube(checkPos, groundCheckSize);
        }
    }
}
