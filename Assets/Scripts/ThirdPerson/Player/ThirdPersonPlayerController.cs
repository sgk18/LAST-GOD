using System.Collections;
using UnityEngine;
using LastGod.ThirdPerson.Combat;

namespace LastGod.ThirdPerson.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(ThirdPersonPlayerInput))]
    [RequireComponent(typeof(Health3D))]
    public class ThirdPersonPlayerController : MonoBehaviour
    {
        [Header("Locomotion Speeds")]
        [SerializeField] private float walkSpeed = 3.6f;
        [SerializeField] private float runSpeed = 6.8f;
        [SerializeField] private float sprintSpeed = 10.5f;
        [SerializeField] private float crouchSpeed = 2.2f;
        [SerializeField] private float acceleration = 14f;
        [SerializeField] private float deceleration = 16f;
        [SerializeField] private float rotationSpeed = 12f;

        [Header("Jump & Physics")]
        [SerializeField] private float jumpHeight = 1.6f;
        [SerializeField] private float gravity = 22f;
        [SerializeField] private float slopeLimit = 45f;

        [Header("Dodge / Evade")]
        [SerializeField] private float dodgeSpeed = 14f;
        [SerializeField] private float dodgeDuration = 0.32f;
        [SerializeField] private float dodgeCooldown = 0.5f;
        [SerializeField] private float dodgeInvulnerability = 0.28f;

        [Header("Combat Strike Tuning")]
        [SerializeField] private float attackRange = 2.2f;
        [SerializeField] private float attackRadius = 1.0f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private float comboResetTime = 0.75f;

        [Header("Audio & SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip punchSFX;
        [SerializeField] private AudioClip heavyStrikeSFX;
        [SerializeField] private AudioClip whooshSFX;
        [SerializeField] private AudioClip dodgeSFX;

        // Components
        private CharacterController _cc;
        private ThirdPersonPlayerInput _input;
        private ThirdPersonPlayerAnimator _animator;
        private Health3D _health;
        private AscensionSurge _surge;
        private Transform _cameraTransform;

        // Locomotion state
        private Vector3 _velocity;
        private float _currentSpeed;
        private float _verticalVelocity;
        private bool _isCrouching;
        private bool _isDodging;
        private float _dodgeTimer;
        private Vector3 _dodgeDirection;

        // Combat state
        private int _comboIndex = 0;
        private float _lastAttackTime;
        private bool _isAttacking;

        public bool IsDodging => _isDodging;
        public bool IsCrouching => _isCrouching;
        public float CurrentSpeed => _currentSpeed;
        public Health3D Health => _health;
        public AscensionSurge Surge => _surge;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _input = GetComponent<ThirdPersonPlayerInput>();
            _animator = GetComponentInChildren<ThirdPersonPlayerAnimator>();
            _health = GetComponent<Health3D>();
            _surge = GetComponent<AscensionSurge>();

            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }

            _cc.slopeLimit = slopeLimit;
        }

        private void Start()
        {
            // Register player
            if (gameObject.tag != "Player") gameObject.tag = "Player";
        }

        public void SetInputLocked(bool locked)
        {
            if (_input != null) _input.InputLocked = locked;
            if (locked)
            {
                _currentSpeed = 0f;
                _velocity = Vector3.zero;
            }
        }

        private void Update()
        {
            if (_health != null && _health.IsDead) return;

            if (_dodgeTimer > 0f) _dodgeTimer -= Time.deltaTime;

            HandleCombatInput();
            HandleLocomotion();
            UpdateAnimator();
        }

        private void HandleLocomotion()
        {
            if (_isDodging)
            {
                _cc.Move(_dodgeDirection * (dodgeSpeed * Time.deltaTime) + Vector3.down * (gravity * Time.deltaTime));
                return;
            }

            bool grounded = _cc.isGrounded;
            if (grounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            // Stance & Speed
            _isCrouching = _input.CrouchPressed && grounded;
            float targetSpeed = walkSpeed;

            if (_input.MoveInput.magnitude > 0.05f)
            {
                if (_isCrouching)
                {
                    targetSpeed = crouchSpeed;
                }
                else if (_input.SprintPressed)
                {
                    targetSpeed = sprintSpeed;
                }
                else
                {
                    targetSpeed = runSpeed;
                }
            }
            else
            {
                targetSpeed = 0f;
            }

            // Apply Ascension Surge boost if active
            if (_surge != null && _surge.IsActive)
            {
                targetSpeed *= _surge.SpeedMultiplier;
            }

            // Smooth speed interpolation
            float rate = targetSpeed > _currentSpeed ? acceleration : deceleration;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, rate * Time.deltaTime);

            // Compute camera-relative movement direction
            Vector3 camForward = _cameraTransform != null ? _cameraTransform.forward : transform.forward;
            Vector3 camRight = _cameraTransform != null ? _cameraTransform.right : transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * _input.MoveInput.y + camRight * _input.MoveInput.x).normalized;

            // Rotation
            var cm = CombatManager.Instance;
            if (cm != null && cm.HasLockedTarget)
            {
                Vector3 toTarget = cm.CurrentLockedTarget.position - transform.position;
                toTarget.y = 0f;
                if (toTarget.sqrMagnitude > 0.01f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget), rotationSpeed * 1.5f * Time.deltaTime);
                }
            }
            else if (moveDir.sqrMagnitude > 0.001f && !_isAttacking)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            // Jump
            if (_input.JumpTriggered && grounded && !_isCrouching && !_isAttacking)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
            }

            // Gravity
            _verticalVelocity -= gravity * Time.deltaTime;

            // Apply Movement
            Vector3 motion = (moveDir * _currentSpeed) + (Vector3.up * _verticalVelocity);
            _cc.Move(motion * Time.deltaTime);
        }

        private void HandleCombatInput()
        {
            // Dodge
            if (_input.DodgeTriggered && _dodgeTimer <= 0f && !_isDodging)
            {
                StartCoroutine(PerformDodge());
                return;
            }

            // Ascension Surge
            if (_input.SurgeTriggered && _surge != null)
            {
                _surge.TryActivate();
            }

            // Lock-on toggle
            if (_input.LockOnTriggered)
            {
                var cm = CombatManager.Instance;
                if (cm != null && _cameraTransform != null)
                {
                    cm.ToggleLockOn(transform.position, _cameraTransform.forward);
                }
            }

            // Attacks
            if (_input.LightAttackTriggered && !_isAttacking && !_isDodging)
            {
                PerformLightAttack();
            }
            else if (_input.HeavyAttackTriggered && !_isAttacking && !_isDodging)
            {
                PerformHeavyAttack();
            }
        }

        private void PerformLightAttack()
        {
            _isAttacking = true;
            if (Time.time - _lastAttackTime > comboResetTime)
            {
                _comboIndex = 0;
            }

            _comboIndex = (_comboIndex % 3) + 1;
            _lastAttackTime = Time.time;

            if (_animator != null) _animator.PlayLightAttack(_comboIndex);
            if (audioSource != null && whooshSFX != null) audioSource.PlayOneShot(whooshSFX);

            float baseDamage = _comboIndex switch
            {
                1 => 14f,
                2 => 18f,
                _ => 28f
            };

            if (_surge != null && _surge.IsActive) baseDamage *= _surge.DamageMultiplier;

            // Hit detection slightly in front
            Vector3 hitCenter = transform.position + Vector3.up * 1.1f;
            var cm = CombatManager.Instance;
            if (cm != null)
            {
                int hits = cm.PerformMeleeHit(
                    hitCenter,
                    transform.forward,
                    attackRadius,
                    attackRange,
                    new DamageInfo(baseDamage, gameObject, default, default, transform.forward, _comboIndex == 3 ? 9f : 3f, _comboIndex == 3),
                    enemyLayer
                );

                if (hits > 0 && audioSource != null && punchSFX != null)
                {
                    audioSource.PlayOneShot(punchSFX);
                }
            }

            StartCoroutine(ResetAttackAfter(0.25f));
        }

        private void PerformHeavyAttack()
        {
            _isAttacking = true;
            _comboIndex = 0;
            _lastAttackTime = Time.time;

            if (_animator != null) _animator.PlayHeavyAttack();
            if (audioSource != null && heavyStrikeSFX != null) audioSource.PlayOneShot(heavyStrikeSFX);

            float damage = 42f;
            if (_surge != null && _surge.IsActive) damage *= _surge.DamageMultiplier;

            Vector3 hitCenter = transform.position + Vector3.up * 1.1f;
            var cm = CombatManager.Instance;
            if (cm != null)
            {
                int hits = cm.PerformMeleeHit(
                    hitCenter,
                    transform.forward,
                    attackRadius * 1.3f,
                    attackRange * 1.2f,
                    new DamageInfo(damage, gameObject, default, default, transform.forward, 15f, true),
                    enemyLayer
                );

                if (hits > 0 && audioSource != null && punchSFX != null)
                {
                    audioSource.PlayOneShot(punchSFX);
                }
            }

            StartCoroutine(ResetAttackAfter(0.45f));
        }

        private IEnumerator ResetAttackAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            _isAttacking = false;
        }

        private IEnumerator PerformDodge()
        {
            _isDodging = true;
            _dodgeTimer = dodgeCooldown;

            Vector3 inputDir = (_cameraTransform.forward * _input.MoveInput.y + _cameraTransform.right * _input.MoveInput.x);
            inputDir.y = 0f;
            _dodgeDirection = inputDir.sqrMagnitude > 0.05f ? inputDir.normalized : -transform.forward;

            transform.rotation = Quaternion.LookRotation(_dodgeDirection);

            if (_animator != null) _animator.PlayDodge();
            if (audioSource != null && dodgeSFX != null) audioSource.PlayOneShot(dodgeSFX);
            if (_health != null) _health.SetInvulnerable(dodgeInvulnerability);

            yield return new WaitForSeconds(dodgeDuration);
            _isDodging = false;
        }

        private void UpdateAnimator()
        {
            if (_animator != null)
            {
                float forward = Vector3.Dot(transform.forward, _cc.velocity);
                float lateral = Vector3.Dot(transform.right, _cc.velocity);
                _animator.SetMovementState(forward, lateral, _cc.isGrounded, _isCrouching, _input.SprintPressed);
            }
        }
    }
}
