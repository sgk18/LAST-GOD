using System;
using UnityEngine;
using LastGod.Core;
using LastGod.Weapons;

namespace LastGod.Player.Guard
{
    public enum GuardState
    {
        Idle,
        Walk,
        Run,
        Jump,
        Fall,
        Land,
        Crouch,
        Interact,
        Aim,
        Attack,
        Reload,
        Hurt,
        Dead
    }

    /// <summary>
    /// Primary actor controller for the human security Guard (Unit B-3).
    /// Implements state machine, human locomotion, flashlight, weapon coordination, and IDamageable.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class GuardController : CharacterController2D, IDamageable
    {
        [Header("Guard State")]
        [SerializeField] private GuardState currentState = GuardState.Idle;

        [Header("Subsystems")]
        [SerializeField] private Animator animator;
        [SerializeField] private FlashlightController2D flashlight;
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private Health health;

        [Header("Locomotion Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip footstepSfx;
        [SerializeField] private AudioClip jumpSfx;
        [SerializeField] private AudioClip landSfx;
        [SerializeField] private AudioClip hurtSfx;
        [SerializeField] private AudioClip deathSfx;

        private float _footstepTimer;
        private bool _isAiming;
        private bool _isCrouching;

        public GuardState CurrentState => currentState;
        public bool IsDead => currentState == GuardState.Dead;

        protected override void Awake()
        {
            base.Awake();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (flashlight == null) flashlight = GetComponentInChildren<FlashlightController2D>();
            if (weaponController == null) weaponController = GetComponentInChildren<WeaponController>();
            if (health == null) health = GetComponent<Health>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        protected override void Start()
        {
            base.Start();
            if (health != null)
            {
                health.OnDamaged.AddListener(OnHealthDamaged);
                health.OnDeath.AddListener(OnHealthDeath);
            }
        }

        protected override void Update()
        {
            if (currentState == GuardState.Dead) return;

            base.Update();
            ReadDirectInputs();
            EvaluateStateTransitions();
            UpdateAnimator();
            HandleFootstepAudio();
        }

        private void ReadDirectInputs()
        {
            // Input layer: Support standard keyboard & mouse for testing and New Input System
            float h = UnityEngine.Input.GetAxisRaw("Horizontal");
            bool run = UnityEngine.Input.GetKey(KeyCode.LeftShift);
            SetMoveInput(h, run);

            if (UnityEngine.Input.GetButtonDown("Jump") || UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                BufferJump(true);
            }
            if (UnityEngine.Input.GetButtonUp("Jump") || UnityEngine.Input.GetKeyUp(KeyCode.Space))
            {
                ReleaseJump();
            }

            // Flashlight Toggle (F key)
            if (UnityEngine.Input.GetKeyDown(KeyCode.F))
            {
                flashlight?.Toggle();
            }

            // Crouch (C or S key)
            _isCrouching = UnityEngine.Input.GetKey(KeyCode.C) || UnityEngine.Input.GetKey(KeyCode.S);

            // Aiming (Right Mouse Button or Left Alt)
            _isAiming = UnityEngine.Input.GetMouseButton(1) || UnityEngine.Input.GetKey(KeyCode.LeftAlt);

            // Mouse Aim Direction calculation
            Vector3 mousePos = Camera.main != null ? Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition) : transform.position + Vector3.right;
            Vector2 aimDir = (mousePos - transform.position).normalized;
            flashlight?.SetAimDirection(aimDir);
            weaponController?.SetAimDirection(aimDir);

            // Firing (Left Mouse Button while aiming or when drawn)
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                if (weaponController != null && weaponController.TryFire())
                {
                    animator?.SetTrigger("Fire");
                }
            }

            // Reload (R key)
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                if (weaponController != null)
                {
                    weaponController.TryReload();
                    animator?.SetTrigger("Reload");
                }
            }

            // Interact (E key)
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                animator?.SetTrigger("Interact");
            }
        }

        private void EvaluateStateTransitions()
        {
            if (currentState == GuardState.Dead || currentState == GuardState.Hurt) return;

            if (!_isGrounded)
            {
                SetState(VerticalVelocity > 0 ? GuardState.Jump : GuardState.Fall);
            }
            else if (_wasGrounded == false)
            {
                SetState(GuardState.Land);
                PlaySound(landSfx);
            }
            else if (_isCrouching)
            {
                SetState(GuardState.Crouch);
            }
            else if (_isAiming)
            {
                SetState(GuardState.Aim);
            }
            else if (Mathf.Abs(HorizontalVelocity) > 0.1f)
            {
                SetState(_isRunHeld ? GuardState.Run : GuardState.Walk);
            }
            else
            {
                SetState(GuardState.Idle);
            }
        }

        private void SetState(GuardState next)
        {
            if (currentState == next) return;
            currentState = next;
        }

        private void UpdateAnimator()
        {
            if (animator == null) return;
            animator.SetFloat("Speed", Mathf.Abs(HorizontalVelocity));
            animator.SetBool("IsGrounded", _isGrounded);
            animator.SetFloat("VerticalVelocity", VerticalVelocity);
            animator.SetBool("IsAiming", _isAiming);
            animator.SetBool("IsCrouching", _isCrouching);
            animator.SetBool("IsDead", IsDead);
        }

        protected override void OnJumpExecuted()
        {
            PlaySound(jumpSfx);
            SetState(GuardState.Jump);
        }

        private void HandleFootstepAudio()
        {
            if (_isGrounded && Mathf.Abs(HorizontalVelocity) > 0.1f)
            {
                float stepInterval = _isRunHeld ? 0.32f : 0.48f;
                _footstepTimer += Time.deltaTime;
                if (_footstepTimer >= stepInterval)
                {
                    _footstepTimer = 0f;
                    PlaySound(footstepSfx);
                }
            }
            else
            {
                _footstepTimer = 0f;
            }
        }

        public void TakeDamage(int amount, Vector2 knockbackDir)
        {
            if (IsDead) return;
            PlaySound(hurtSfx);
            animator?.SetTrigger("Hurt");
            SetState(GuardState.Hurt);
            _rb.linearVelocity = knockbackDir * 2.5f;

            if (health != null)
            {
                health.TakeDamage(amount, knockbackDir);
            }
        }

        private void OnHealthDamaged(int currentHP)
        {
            // Handled in TakeDamage
        }

        private void OnHealthDeath()
        {
            SetState(GuardState.Dead);
            PlaySound(deathSfx);
            animator?.SetBool("IsDead", true);
            _rb.linearVelocity = Vector2.zero;
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
