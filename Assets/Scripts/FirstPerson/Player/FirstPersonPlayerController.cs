using System;
using UnityEngine;

namespace LastGod.FirstPerson.Player
{
    /// <summary>
    /// Core locomotion controller for Aeron in the Full 3D First-Person architecture.
    /// Handles grounded movement, sprinting, crouching, slope navigation, and smooth acceleration.
    /// Zero combat mechanics or abilities are present in this foundation pass.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonPlayerController : MonoBehaviour
    {
        [Header("Locomotion Speeds")]
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float sprintSpeed = 6.5f;
        [SerializeField] private float crouchSpeed = 2.0f;
        [SerializeField] private float acceleration = 12f;
        [SerializeField] private float deceleration = 14f;

        [Header("Physics & Grounding")]
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.25f;
        [SerializeField] private LayerMask groundMask;

        [Header("References")]
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Animator bodyAnimator;
        [SerializeField] private Animator armsAnimator;

        // Runtime state
        private CharacterController _controller;
        private Vector3 _moveVelocity;
        private float _verticalVelocity;
        private bool _isGrounded;
        private bool _isCrouching;
        private float _originalHeight;
        private Vector3 _originalCenter;

        public bool IsGrounded => _isGrounded;
        public bool IsMoving => _moveVelocity.sqrMagnitude > 0.05f;
        public float CurrentSpeed => _moveVelocity.magnitude;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _originalHeight = _controller.height;
            _originalCenter = _controller.center;

            if (groundMask == 0)
            {
                // Default ground mask to Layer 8 ("Ground") and Default
                groundMask = (1 << 0) | (1 << 8);
            }
        }

        private void Update()
        {
            CheckGrounded();
            HandleMovement();
            HandleCrouch();
            ApplyGravity();
            UpdateAnimators();
        }

        private void CheckGrounded()
        {
            if (groundCheck != null)
            {
                _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            }
            else
            {
                _isGrounded = _controller.isGrounded;
            }

            if (_isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = -2f; // Slight downward force to stay glued to slopes/stairs
            }
        }

        private void HandleMovement()
        {
            float horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            float vertical = UnityEngine.Input.GetAxisRaw("Vertical");

            Vector3 inputDirection = (transform.right * horizontal + transform.forward * vertical).normalized;

            float targetSpeed = walkSpeed;
            if (_isCrouching)
            {
                targetSpeed = crouchSpeed;
            }
            else if (UnityEngine.Input.GetKey(KeyCode.LeftShift) && vertical > 0.1f)
            {
                targetSpeed = sprintSpeed;
            }

            Vector3 targetVelocity = inputDirection * targetSpeed;
            float accelRate = (inputDirection.sqrMagnitude > 0.01f) ? acceleration : deceleration;
            _moveVelocity = Vector3.MoveTowards(_moveVelocity, targetVelocity, accelRate * Time.deltaTime);

            Vector3 finalMovement = _moveVelocity;
            finalMovement.y = _verticalVelocity;

            _controller.Move(finalMovement * Time.deltaTime);
        }

        private void HandleCrouch()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftControl) || UnityEngine.Input.GetKeyDown(KeyCode.C))
            {
                _isCrouching = !_isCrouching;
                _controller.height = _isCrouching ? _originalHeight * 0.6f : _originalHeight;
                _controller.center = _isCrouching ? _originalCenter * 0.6f : _originalCenter;

                if (cameraHolder != null)
                {
                    float targetY = _isCrouching ? 1.05f : 1.70f;
                    cameraHolder.localPosition = new Vector3(0, targetY, 0);
                }
            }
        }

        private void ApplyGravity()
        {
            if (UnityEngine.Input.GetButtonDown("Jump") && _isGrounded && !_isCrouching)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
            }

            _verticalVelocity -= gravity * Time.deltaTime;
        }

        private void UpdateAnimators()
        {
            float normalizedSpeed = _moveVelocity.magnitude / sprintSpeed;

            if (bodyAnimator != null)
            {
                bodyAnimator.SetFloat("Speed", normalizedSpeed);
                bodyAnimator.SetBool("IsGrounded", _isGrounded);
            }

            if (armsAnimator != null)
            {
                armsAnimator.SetFloat("Speed", normalizedSpeed);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = _isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }
        }
    }
}
