using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using LastGod.Core;
using LastGod.Input;

namespace LastGod.Player
{
    // ─── State enum ───────────────────────────────────────────────────────────
    public enum PlayerState
    {
        Idle,
        Run,
        Jump,
        Climb,
        Attack,
        Hurt,
        Dead
    }

    // ─── PlayerController ─────────────────────────────────────────────────────
    /// <summary>
    /// Root player brain for Aeron (Hero Knight).
    /// Supports:
    ///   - Responsive 8-way movement + snappy stop
    ///   - Double Jump & Variable Jump Cut
    ///   - Dash / Dodge Roll with i-frames & SlideDust VFX
    ///   - 3-hit combo attacks (Left Click, J, Z, F, E) with buffering
    ///   - Ladder Climbing & Wall Sliding (Hero Knight WallSlide pose)
    ///   - Shield Guard / Blocking (Right Click, K, X)
    ///   - Dual-input engine: New Input System + direct UnityEngine.Input
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6.5f;

        [Header("Jump & Double Jump")]
        [SerializeField] private float jumpForce = 13.5f;
        [SerializeField] private float doubleJumpForce = 12f;
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private float fallMultiplier = 3f;
        [SerializeField] private float groundCheckDistance = 0.15f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Dash / Dodge Roll")]
        [SerializeField] private float dashSpeed = 16f;
        [SerializeField] private float dashDuration = 0.22f;
        [SerializeField] private float dashCooldown = 0.45f;
        [SerializeField] private float dashInvincibilityWindow = 0.25f;

        [Header("Climbing (Ladders & Wall Slide)")]
        [SerializeField] private float climbSpeed = 4.5f;
        [SerializeField] private float wallSlideSpeed = 2.0f;
        [SerializeField] private LayerMask ladderLayer;

        [Header("Hero Knight Visuals & VFX")]
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject slideDustPrefab;
        [SerializeField] private Transform dustSpawnPoint;

        [Header("Hero Knight SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip slash1SFX;
        [SerializeField] private AudioClip slash2SFX;
        [SerializeField] private AudioClip slash3SFX;
        [SerializeField] private AudioClip jumpSFX;
        [SerializeField] private AudioClip dashSFX;
        [SerializeField] private AudioClip blockSFX;
        [SerializeField] private AudioClip hurtSFX;
        [SerializeField] private AudioClip deathSFX;

        [Header("Events")]
        public UnityEvent<PlayerState, PlayerState> OnStateChanged = new();

        // ─── Components ───────────────────────────────────────────────────────
        private Rigidbody2D _rb;
        private Collider2D _col;
        private Health _health;
        private SpriteRenderer _sr;

        // ─── Input ────────────────────────────────────────────────────────────
        private PlayerInputActions _input;
        private Vector2 _moveInput;
        private bool _jumpPressed;
        private bool _attackPressed;
        private bool _dashPressed;

        // ─── Movement Moments (Game Feel) ─────────────────────────────────────
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private float _attackBufferTimer;
        private const float CoyoteDuration = 0.14f;
        private const float JumpBufferDuration = 0.14f;
        private float _currentHorizontalInput;
        private float _currentVerticalInput;
        private int _jumpsRemaining;

        // ─── State machine ────────────────────────────────────────────────────
        private PlayerState _state = PlayerState.Idle;
        public PlayerState State => _state;

        // ─── Runtime flags ────────────────────────────────────────────────────
        private bool _isGrounded;
        private bool _facingRight = true;
        private bool _isBlocking;
        private bool _isNearLadder;
        private bool _isClimbing;
        private bool _isWallSliding;

        // Attack Combo
        private int _currentAttack = 0;
        private float _timeSinceAttack = 0f;
        private float _attackDurationTimer = 0f;

        // Dash
        private bool _isDashing;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private bool _isDashInvincible;
        private float _dashInvincibilityTimer;

        // Hurt
        private float _hurtLockTimer;
        private const float HurtLockDuration = 0.35f;

        // ─── Unity lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            _rb     = GetComponent<Rigidbody2D>();
            _col    = GetComponent<Collider2D>();
            _health = GetComponent<Health>();
            _sr     = GetComponent<SpriteRenderer>();

            if (animator == null) animator = GetComponent<Animator>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            // Rigidbody tuning
            _rb.gravityScale   = 3f;
            _rb.freezeRotation = true;
            _rb.interpolation  = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Frictionless physics material so Aeron never catches on walls or corners
            PhysicsMaterial2D frictionless = new PhysicsMaterial2D("HeroKnightFrictionless")
            {
                friction = 0f,
                bounciness = 0f
            };
            _col.sharedMaterial = frictionless;

            // Wire Health events
            _health.OnDamaged.AddListener(OnDamaged);
            _health.OnDeath.AddListener(OnDeath);

            // Input System setup
            try
            {
                _input = new PlayerInputActions();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PlayerController] PlayerInputActions initialization note: {ex.Message}");
            }
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.Player.Enable();
                _input.Player.Move.performed   += ctx => _moveInput   = ctx.ReadValue<Vector2>();
                _input.Player.Move.canceled    += ctx => _moveInput   = Vector2.zero;
                _input.Player.Jump.performed   += ctx => _jumpPressed  = true;
                _input.Player.Attack.performed += ctx => _attackPressed = true;
                _input.Player.Dash.performed   += ctx => _dashPressed  = true;
            }
        }

        private void OnDisable()
        {
            _input?.Player.Disable();
        }

        private void Update()
        {
            TickTimers();
            CheckGroundAndSurroundings();
            ReadInputs();
            UpdateAnimationParameters();
            ProcessInput();

            // Clear single-frame flags
            _jumpPressed   = false;
            _attackPressed = false;
            _dashPressed   = false;
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyFallMultiplier();
        }

        // ─── Input Reading (Dual Input: System + Legacy) ──────────────────────
        private void ReadInputs()
        {
            // Horizontal movement
            float h = _moveInput.x;
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h = -1f;
                else if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h = 1f;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                float rawH = UnityEngine.Input.GetAxisRaw("Horizontal");
                if (Mathf.Abs(rawH) > 0.01f) h = rawH;
            }
            catch {}
#endif
            _currentHorizontalInput = h;

            // Vertical movement (for climbing ladders)
            float v = _moveInput.y;
#if ENABLE_INPUT_SYSTEM
            if (kb != null)
            {
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v = -1f;
                else if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v = 1f;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                float rawV = UnityEngine.Input.GetAxisRaw("Vertical");
                if (Mathf.Abs(rawV) > 0.01f) v = rawV;
            }
            catch {}
#endif
            _currentVerticalInput = v;

            // Ground & Jumps Reset
            if (_isGrounded)
            {
                _coyoteTimer = CoyoteDuration;
                _jumpsRemaining = maxJumps;
                if (_isClimbing) StopClimbing();
            }
            else
            {
                _coyoteTimer -= Time.deltaTime;
            }

            // Jump input & Buffer
            bool jumpDown = _jumpPressed;
#if ENABLE_INPUT_SYSTEM
            if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame))
            {
                jumpDown = true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (UnityEngine.Input.GetButtonDown("Jump") ||
                    UnityEngine.Input.GetKeyDown(KeyCode.Space) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.W) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
                {
                    jumpDown = true;
                }
            }
            catch {}
#endif

            if (jumpDown) _jumpBufferTimer = JumpBufferDuration;
            else _jumpBufferTimer -= Time.deltaTime;

            // Variable jump height (releasing jump cuts ascent)
            bool jumpHeld = false;
#if ENABLE_INPUT_SYSTEM
            if (kb != null)
            {
                jumpHeld = kb.spaceKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                jumpHeld = UnityEngine.Input.GetButton("Jump") ||
                           UnityEngine.Input.GetKey(KeyCode.Space) ||
                           UnityEngine.Input.GetKey(KeyCode.W) ||
                           UnityEngine.Input.GetKey(KeyCode.UpArrow);
            }
            catch {}
#endif

            if (!jumpHeld && _rb.linearVelocity.y > 0f && !_isClimbing)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * 0.55f);
            }

            // Attack input (Buffering & Multi-key: Left Click, J, Z, F, E)
            bool attackDown = _attackPressed;
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame) attackDown = true;
            if (kb != null && (kb.jKey.wasPressedThisFrame || kb.zKey.wasPressedThisFrame || kb.fKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)) attackDown = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (UnityEngine.Input.GetMouseButtonDown(0) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.J) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.Z) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.F) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.E))
                {
                    attackDown = true;
                }
            }
            catch {}
#endif

            if (attackDown) _attackBufferTimer = 0.2f;
            else _attackBufferTimer -= Time.deltaTime;

            // Dash / Dodge Roll input (Left Shift, L, C, Right Shift)
#if ENABLE_INPUT_SYSTEM
            if (kb != null && (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame || kb.lKey.wasPressedThisFrame || kb.cKey.wasPressedThisFrame))
            {
                _dashPressed = true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.LeftShift) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.RightShift) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.L) ||
                    UnityEngine.Input.GetKeyDown(KeyCode.C))
                {
                    _dashPressed = true;
                }
            }
            catch {}
#endif

            // Guard / Block input (Right Click, K, X)
            bool blockHeld = false;
#if ENABLE_INPUT_SYSTEM
            if (mouse != null && mouse.rightButton.isPressed) blockHeld = true;
            if (kb != null && (kb.kKey.isPressed || kb.xKey.isPressed)) blockHeld = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                blockHeld = UnityEngine.Input.GetMouseButton(1) ||
                            UnityEngine.Input.GetKey(KeyCode.K) ||
                            UnityEngine.Input.GetKey(KeyCode.X);
            }
            catch {}
#endif

            if (blockHeld && _state != PlayerState.Hurt && _state != PlayerState.Dead && !_isDashing && !_isClimbing)
            {
                if (!_isBlocking)
                {
                    _isBlocking = true;
                    if (animator != null)
                    {
                        animator.SetTrigger("Block");
                        animator.SetBool("IdleBlock", true);
                    }
                    PlaySFX(blockSFX);
                }
            }
            else if (_isBlocking)
            {
                _isBlocking = false;
                if (animator != null)
                {
                    animator.SetBool("IdleBlock", false);
                }
            }

            // Ladder Grab Check
            if (_isNearLadder && Mathf.Abs(_currentVerticalInput) > 0.1f && !_isDashing)
            {
                if (!_isClimbing) StartClimbing();
            }
        }

        private void UpdateAnimationParameters()
        {
            if (animator == null) return;

            bool isSlidingOrClimbing = _isClimbing || _isWallSliding;
            animator.SetBool("WallSlide", isSlidingOrClimbing);

            if (_state != PlayerState.Attack && _state != PlayerState.Hurt && _state != PlayerState.Dead && !_isBlocking)
            {
                int animState = (_isGrounded && Mathf.Abs(_currentHorizontalInput) > 0.05f) ? 1 : 0;
                animator.SetInteger("AnimState", animState);
            }

            animator.SetBool("Grounded", _isGrounded);
            animator.SetFloat("AirSpeedY", _rb.linearVelocity.y);
        }

        // ─── Ground & Surroundings Detection ──────────────────────────────────
        private void CheckGroundAndSurroundings()
        {
            if (_col == null) return;

            Bounds b = _col.bounds;
            Vector2 origin = new(b.center.x, b.min.y);
            Vector2 size   = new(b.size.x * 0.88f, groundCheckDistance);

            // Layer mask includes Ground (8) and Default (0)
            LayerMask mask = groundLayer.value != 0 ? groundLayer : (LayerMask.GetMask("Ground") | (1 << 0) | (1 << 8));
            _isGrounded = Physics2D.OverlapBox(origin, size, 0f, mask);

            // Check if near ladder or wall
            Collider2D[] overlaps = Physics2D.OverlapBoxAll(b.center, b.size, 0f);
            _isNearLadder = false;
            foreach (var col in overlaps)
            {
                if (col.gameObject == gameObject) continue;
                bool isLadder = col.name.IndexOf("Ladder", StringComparison.OrdinalIgnoreCase) >= 0;
                if (!isLadder)
                {
                    try
                    {
                        if (col.CompareTag("Ladder")) isLadder = true;
                    }
                    catch
                    {
                        // Tag "Ladder" not defined in project tags
                    }
                }
                if (isLadder)
                {
                    _isNearLadder = true;
                    break;
                }
            }

            // Wall slide check: when airborne, falling, and pressing into a wall
            _isWallSliding = false;
            if (!_isGrounded && !_isClimbing && _rb.linearVelocity.y < 0.1f && Mathf.Abs(_currentHorizontalInput) > 0.1f)
            {
                float wallCheckDir = _facingRight ? 1f : -1f;
                Vector2 wallOrigin = new(b.center.x + wallCheckDir * (b.extents.x + 0.05f), b.center.y);
                Collider2D wallHit = Physics2D.OverlapCircle(wallOrigin, 0.1f, mask);
                if (wallHit != null && !wallHit.isTrigger)
                {
                    _isWallSliding = true;
                    _jumpsRemaining = Mathf.Max(1, _jumpsRemaining); // replenish a jump on wall slide
                }
            }
        }

        // ─── State machine ────────────────────────────────────────────────────
        private void SetState(PlayerState next)
        {
            if (_state == next) return;

            PlayerState prev = _state;
            _state = next;
            OnStateChanged.Invoke(prev, next);
        }

        // ─── Input → State Transitions ────────────────────────────────────────
        private void ProcessInput()
        {
            if (_state == PlayerState.Dead) return;

            if (_state == PlayerState.Hurt)
            {
                if (_hurtLockTimer <= 0f)
                    SetState(PlayerState.Idle);
                return;
            }

            if (_isDashing)
            {
                if (_dashTimer <= 0f)
                    EndDash();
                return;
            }

            // --- Dash (Roll) ---
            if (_dashPressed && _dashCooldownTimer <= 0f && !_isBlocking && !_isClimbing)
            {
                StartDash();
                return;
            }

            // --- Jump & Double Jump ---
            if (_jumpBufferTimer > 0f && !_isBlocking)
            {
                // First Jump (from ground or coyote time)
                if (_isGrounded || _coyoteTimer > 0f)
                {
                    ExecuteJump(jumpForce);
                    _jumpsRemaining = maxJumps - 1;
                    return;
                }
                // Wall Jump
                else if (_isWallSliding)
                {
                    _isWallSliding = false;
                    float kickDir = _facingRight ? -1f : 1f;
                    _rb.linearVelocity = new Vector2(kickDir * moveSpeed, jumpForce);
                    Flip();
                    if (animator != null)
                    {
                        animator.SetTrigger("Jump");
                        animator.SetBool("Grounded", false);
                    }
                    PlaySFX(jumpSFX);
                    _jumpBufferTimer = 0f;
                    _jumpsRemaining = maxJumps - 1;
                    return;
                }
                // Ladder Jump
                else if (_isClimbing)
                {
                    StopClimbing();
                    ExecuteJump(jumpForce * 0.95f);
                    _jumpsRemaining = maxJumps - 1;
                    return;
                }
                // Double Jump!
                else if (_jumpsRemaining > 0)
                {
                    ExecuteDoubleJump();
                    _jumpsRemaining--;
                    return;
                }
            }

            // --- Attack (3-hit Combo) ---
            if (_attackBufferTimer > 0f && !_isBlocking && !_isClimbing)
            {
                _attackBufferTimer = 0f;
                ExecuteAttackCombo();
                return;
            }

            // --- Climb / Run / Idle State Resolution ---
            if (_isClimbing)
            {
                SetState(PlayerState.Climb);
            }
            else if (_isGrounded)
            {
                if (_isBlocking)
                {
                    SetState(PlayerState.Idle);
                }
                else if (_state != PlayerState.Attack)
                {
                    SetState(Mathf.Abs(_currentHorizontalInput) > 0.01f ? PlayerState.Run : PlayerState.Idle);
                }
            }
            else
            {
                if (_state != PlayerState.Jump && _state != PlayerState.Attack)
                    SetState(PlayerState.Jump);
            }
        }

        // ─── Jump Methods ─────────────────────────────────────────────────────
        private void ExecuteJump(float force)
        {
            _jumpBufferTimer = 0f;
            _coyoteTimer = 0f;
            SetState(PlayerState.Jump);
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, force);

            if (animator != null)
            {
                animator.SetTrigger("Jump");
                animator.SetBool("Grounded", false);
            }
            PlaySFX(jumpSFX);
        }

        private void ExecuteDoubleJump()
        {
            _jumpBufferTimer = 0f;
            SetState(PlayerState.Jump);
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, doubleJumpForce);

            if (animator != null)
            {
                animator.SetTrigger("Jump");
                animator.SetBool("Grounded", false);
            }
            PlaySFX(jumpSFX);
            SpawnSlideDust(); // Little dust puff on double jump
        }

        // ─── Movement ─────────────────────────────────────────────────────────
        private void ApplyMovement()
        {
            if (_state == PlayerState.Dead) return;

            // Dash override
            if (_isDashing)
            {
                float dir = _facingRight ? 1f : -1f;
                _rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);
                return;
            }

            if (_state == PlayerState.Hurt) return;

            // Shield block locks horizontal movement
            if (_isBlocking)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            // Ladder Climbing
            if (_isClimbing)
            {
                float climbVy = _currentVerticalInput * climbSpeed;
                float climbVx = _currentHorizontalInput * (moveSpeed * 0.5f);
                _rb.linearVelocity = new Vector2(climbVx, climbVy);
                return;
            }

            // Wall slide fall speed clamp
            if (_isWallSliding)
            {
                float clampedVy = Mathf.Max(_rb.linearVelocity.y, -wallSlideSpeed);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, clampedVy);
                return;
            }

            // Normal Run / Air horizontal movement
            float xVel = _currentHorizontalInput * moveSpeed;
            _rb.linearVelocity = new Vector2(xVel, _rb.linearVelocity.y);

            // Sprite facing flip
            if (_currentHorizontalInput > 0.01f && !_facingRight) Flip();
            if (_currentHorizontalInput < -0.01f && _facingRight) Flip();
        }

        private void ApplyFallMultiplier()
        {
            if (_isDashing || _isClimbing || _isWallSliding) return;

            if (_rb.linearVelocity.y < 0f)
            {
                _rb.linearVelocity += Vector2.up * Physics2D.gravity.y
                    * (fallMultiplier - 1f) * Time.fixedDeltaTime;
            }
        }

        // ─── Climbing Helpers ─────────────────────────────────────────────────
        private void StartClimbing()
        {
            _isClimbing = true;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = Vector2.zero;
            _jumpsRemaining = maxJumps;
            SetState(PlayerState.Climb);
        }

        private void StopClimbing()
        {
            if (!_isClimbing) return;
            _isClimbing = false;
            _rb.gravityScale = 3f;
        }

        // ─── Dash ─────────────────────────────────────────────────────────────
        private void StartDash()
        {
            _isDashing            = true;
            _dashTimer            = dashDuration;
            _dashCooldownTimer    = dashCooldown;
            _isDashInvincible     = true;
            _dashInvincibilityTimer = dashInvincibilityWindow;

            _rb.linearVelocity = Vector2.zero;
            _rb.gravityScale   = 0f;

            if (animator != null)
            {
                animator.SetTrigger("Roll");
            }
            PlaySFX(dashSFX);
            SpawnSlideDust();
        }

        private void EndDash()
        {
            _isDashing       = false;
            _rb.gravityScale = 3f;
        }

        private void SpawnSlideDust()
        {
            if (slideDustPrefab == null) return;

            Vector3 spawnPos = dustSpawnPoint != null ? dustSpawnPoint.position : transform.position;
            GameObject dust = Instantiate(slideDustPrefab, spawnPos, Quaternion.identity);
            float dir = _facingRight ? 1f : -1f;
            dust.transform.localScale = new Vector3(dir, 1f, 1f);
        }

        // ─── Combat ───────────────────────────────────────────────────────────
        private void ExecuteAttackCombo()
        {
            if (_timeSinceAttack > 1.0f)
                _currentAttack = 0;

            _currentAttack++;
            if (_currentAttack > 3)
                _currentAttack = 1;

            _timeSinceAttack = 0f;
            _attackDurationTimer = 0.28f;
            SetState(PlayerState.Attack);

            if (animator != null)
            {
                animator.SetTrigger("Attack" + _currentAttack);
            }

            AudioClip slashClip = _currentAttack switch
            {
                1 => slash1SFX,
                2 => slash2SFX,
                _ => slash3SFX
            };
            PlaySFX(slashClip);

            PerformMeleeHit(_currentAttack);
        }

        private void PerformMeleeHit(int comboStep)
        {
            float dir = _facingRight ? 1f : -1f;
            Vector2 attackCenter = (Vector2)transform.position + new Vector2(dir * 1.0f, 0.4f);
            float radius = 1.1f;

            int damage = comboStep switch
            {
                1 => 2,
                2 => 3,
                _ => 5 // Finisher bonus damage
            };

            Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, radius);
            foreach (var h in hits)
            {
                if (h.gameObject != gameObject && h.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(damage, new Vector2(dir * 3f, 1.5f));
                }
            }
        }

        // ─── Timers ───────────────────────────────────────────────────────────
        private void TickTimers()
        {
            float dt = Time.deltaTime;

            _timeSinceAttack += dt;

            if (_attackDurationTimer > 0f)
            {
                _attackDurationTimer -= dt;
                if (_attackDurationTimer <= 0f && _state == PlayerState.Attack)
                {
                    SetState(_isGrounded ? PlayerState.Idle : PlayerState.Jump);
                }
            }

            if (_dashTimer            > 0f) _dashTimer            -= dt;
            if (_dashCooldownTimer    > 0f) _dashCooldownTimer    -= dt;
            if (_hurtLockTimer        > 0f) _hurtLockTimer        -= dt;

            if (_dashInvincibilityTimer > 0f)
            {
                _dashInvincibilityTimer -= dt;
                if (_dashInvincibilityTimer <= 0f)
                    _isDashInvincible = false;
            }
        }

        // ─── Hurt / Death callbacks ───────────────────────────────────────────
        private void OnDamaged(int remainingHP)
        {
            if (_isDashInvincible) return;

            // Shield block negates damage and deflects
            if (_isBlocking)
            {
                PlaySFX(blockSFX);
                return;
            }

            StopClimbing();
            SetState(PlayerState.Hurt);
            _hurtLockTimer = HurtLockDuration;

            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }
            PlaySFX(hurtSFX);
        }

        private void OnDeath()
        {
            StopClimbing();
            SetState(PlayerState.Dead);
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated      = false;

            if (animator != null)
            {
                animator.SetTrigger("Death");
            }
            PlaySFX(deathSFX);
            Debug.Log("[Player] Aeron died.");
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────────────
        private void Flip()
        {
            _facingRight = !_facingRight;
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (_facingRight ? 1f : -1f);
            transform.localScale = s;
        }

        public bool IsInvincible => _isDashInvincible;
        public bool IsBlocking => _isBlocking;
        public bool IsClimbing => _isClimbing;

        // ─── Gizmos ───────────────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            if (_col == null) _col = GetComponent<Collider2D>();
            if (_col == null) return;

            Bounds b = _col.bounds;
            Vector3 origin = new(b.center.x, b.min.y, 0f);
            Vector3 size   = new(b.size.x * 0.88f, groundCheckDistance, 0f);

            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireCube(origin, size);

            float dir = _facingRight ? 1f : -1f;
            Vector3 attackCenter = transform.position + new Vector3(dir * 1.0f, 0.4f, 0f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackCenter, 1.1f);
        }
    }
}
