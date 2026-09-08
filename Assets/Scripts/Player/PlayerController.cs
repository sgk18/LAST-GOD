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
        Dead,
        Awakening
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

        [Header("Post-Chamber Action Locking")]
        [Tooltip("When true, the character can ONLY walk and jump. Attack, Dash, Block, and Special Powers are strictly locked.")]
        [SerializeField] private bool lockToWalkAndJumpOnly = true;

        public bool LockToWalkAndJumpOnly
        {
            get => lockToWalkAndJumpOnly;
            set => lockToWalkAndJumpOnly = value;
        }

        [Header("Awakening & Chronos Aura")]
        [SerializeField] private GameObject chronosAuraVisual;
        [SerializeField] private bool hasChronosAura = false;
        [SerializeField] private AudioClip awakeningVoiceClip;
        private bool _controlsLocked = false;
        private Coroutine _awakeningCoroutine;

        [Header("Fireball Attack")]
        [SerializeField] private bool hasFireballPower = false;
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private Transform fireballSpawnPoint;
        [SerializeField] private AudioClip fireballCastSFX;

        public bool HasFireballPower
        {
            get => hasFireballPower;
            set => hasFireballPower = value;
        }

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
        private static bool _ladderTagValid = true;

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

            EnsureCharacterVisibility();
        }

        private void Start()
        {
            EnsureCharacterVisibility();
            EnsureChronosAuraVisual();
            // Start with walk & jump unlocked, combat locked until player presses to awaken
            _controlsLocked = false;
            if (chronosAuraVisual != null)
            {
                chronosAuraVisual.SetActive(false);
            }
        }

        private void EnsureCharacterVisibility()
        {
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();

            _sr.sortingLayerName = "Default";
            _sr.sortingOrder = 10;
            _sr.enabled = true;

            if (_sr.sprite == null)
            {
                Sprite spr = Resources.Load<Sprite>("aeron onside idle/01") ??
                             Resources.Load<Sprite>("01") ??
                             Resources.Load<Sprite>("Aeron_Concept") ?? 
                             Resources.Load<Sprite>("Aeron_Spritesheet");

#if UNITY_EDITOR
                if (spr == null)
                {
                    spr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/aeron onside idle/01.png") ??
                          UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/aeron outside idle/01.png") ??
                          UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Aeron_Concept.png") ??
                          UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Aeron_Spritesheet.png");

                    if (spr == null)
                    {
                        var protoAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/PrototypeCharacter/Weapon_1.png");
                        if (protoAssets != null)
                        {
                            foreach (var a in protoAssets)
                            {
                                if (a is Sprite protoSprite)
                                {
                                    spr = protoSprite;
                                    break;
                                }
                            }
                        }
                    }
                }
#endif

                if (spr == null)
                {
                    // Generate crisp GBA Hero pixel art sprite (16x28 pixels)
                    Texture2D tex = new Texture2D(16, 28, TextureFormat.RGBA32, false);
                    tex.filterMode = FilterMode.Point;
                    Color[] colors = new Color[16 * 28];

                    Color armorGold  = new Color(0.95f, 0.75f, 0.25f, 1f);
                    Color tunicCyan  = new Color(0.20f, 0.75f, 0.95f, 1f);
                    Color skinTone   = new Color(0.95f, 0.80f, 0.65f, 1f);
                    Color darkBoots  = new Color(0.15f, 0.15f, 0.25f, 1f);
                    Color eyeGlow    = new Color(1.00f, 1.00f, 1.00f, 1f);
                    Color clear      = Color.clear;

                    for (int y = 0; y < 28; y++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            Color px = clear;
                            if (y <= 4 && x >= 4 && x <= 11) px = darkBoots;
                            else if (y >= 5 && y <= 10 && x >= 4 && x <= 11) px = tunicCyan;
                            else if (y >= 11 && y <= 19 && x >= 3 && x <= 12) px = (x >= 5 && x <= 10) ? armorGold : tunicCyan;
                            else if (y >= 20 && y <= 26 && x >= 4 && x <= 11)
                            {
                                if (y == 23 && (x == 6 || x == 9)) px = eyeGlow;
                                else if (y >= 25) px = armorGold;
                                else px = skinTone;
                            }
                            colors[y * 16 + x] = px;
                        }
                    }

                    tex.SetPixels(colors);
                    tex.Apply();
                    spr = Sprite.Create(tex, new Rect(0, 0, 16, 28), new Vector2(0.5f, 0.0f), 16f);
                }

                _sr.sprite = spr;
            }

#if UNITY_EDITOR
            if (fireballPrefab == null)
            {
                fireballPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Aeron_Fireball_FX.prefab");
            }
#endif
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

            // Check for player press to awaken Aeron and activate Chronos Chakra
            if (!hasFireballPower && _awakeningCoroutine == null)
            {
                bool awakenPressed = false;
#if ENABLE_INPUT_SYSTEM
                var kb = Keyboard.current;
                var mouse = Mouse.current;
                if (kb != null)
                {
                    if (kb.jKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame || kb.qKey.wasPressedThisFrame ||
                        kb.enterKey.wasPressedThisFrame || kb.fKey.wasPressedThisFrame || kb.kKey.wasPressedThisFrame ||
                        kb.digit1Key.wasPressedThisFrame || kb.digit2Key.wasPressedThisFrame || kb.digit3Key.wasPressedThisFrame)
                    {
                        awakenPressed = true;
                    }
                }
                if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame))
                {
                    awakenPressed = true;
                }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
                try
                {
                    if (UnityEngine.Input.GetKeyDown(KeyCode.J) || UnityEngine.Input.GetKeyDown(KeyCode.E) ||
                        UnityEngine.Input.GetKeyDown(KeyCode.Q) || UnityEngine.Input.GetKeyDown(KeyCode.F) ||
                        UnityEngine.Input.GetKeyDown(KeyCode.Return) || UnityEngine.Input.GetMouseButtonDown(0) ||
                        UnityEngine.Input.GetMouseButtonDown(1))
                    {
                        awakenPressed = true;
                    }
                }
                catch { }
#endif
                if (awakenPressed)
                {
                    TriggerAwakening(2.4f);
                }
            }

            if (chronosAuraVisual != null && chronosAuraVisual.activeSelf)
            {
                chronosAuraVisual.transform.Rotate(0f, 0f, -60f * Time.deltaTime);
            }

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
            if (_controlsLocked || _state == PlayerState.Awakening || _state == PlayerState.Dead)
            {
                _currentHorizontalInput = 0f;
                _currentVerticalInput = 0f;
                return;
            }

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
                if (Mathf.Abs(rawH) > 0.15f) h = rawH;
            }
            catch {}
#endif
            if (Mathf.Abs(h) < 0.15f) h = 0f;
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

            // Intercept non-walk/jump actions when lockToWalkAndJumpOnly is active
            if (lockToWalkAndJumpOnly)
            {
                if (attackDown || _dashPressed || blockHeld)
                {
                    SendMessage("ShowNotification", "🔒 ACTION LOCKED: Post-Chamber Stasis State — ONLY Walk & Jump Enabled!", SendMessageOptions.DontRequireReceiver);
                }

                attackDown = false;
                _attackBufferTimer = 0f;
                _dashPressed = false;
                blockHeld = false;
            }

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
                animator.SetFloat("Speed", Mathf.Abs(_currentHorizontalInput));
            }
            else
            {
                animator.SetFloat("Speed", 0f);
            }

            animator.SetBool("Grounded", _isGrounded);
            animator.SetBool("IsGrounded", _isGrounded);
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
                bool isLadder = col.name.IndexOf("Ladder", StringComparison.OrdinalIgnoreCase) >= 0
                    || (col.transform.parent != null && col.transform.parent.name.IndexOf("Ladder", StringComparison.OrdinalIgnoreCase) >= 0);
                if (!isLadder && _ladderTagValid)
                {
                    try
                    {
                        if (col.CompareTag("Ladder")) isLadder = true;
                    }
                    catch
                    {
                        // Tag "Ladder" not defined in project tags; suppress future checks this session
                        _ladderTagValid = false;
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
            if (_state == PlayerState.Dead || _state == PlayerState.Awakening || _controlsLocked) return;

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
                // One-way platform drop-through: S or Down Arrow + Jump
                if (_isGrounded && _currentVerticalInput < -0.3f)
                {
                    StartCoroutine(DropThroughPlatformRoutine());
                    _jumpBufferTimer = 0f;
                    return;
                }

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

        private System.Collections.IEnumerator DropThroughPlatformRoutine()
        {
            if (_col == null) yield break;

            Bounds b = _col.bounds;
            Vector2 origin = new(b.center.x, b.min.y);
            Vector2 size = new(b.size.x * 0.9f, groundCheckDistance + 0.3f);
            Collider2D[] hits = Physics2D.OverlapBoxAll(origin, size, 0f);
            var ignored = new System.Collections.Generic.List<Collider2D>();

            foreach (var hit in hits)
            {
                if (hit != _col && (hit.usedByEffector || hit.name.IndexOf("OneWay", System.StringComparison.OrdinalIgnoreCase) >= 0 || hit.GetComponent<PlatformEffector2D>() != null))
                {
                    Physics2D.IgnoreCollision(_col, hit, true);
                    ignored.Add(hit);
                }
            }

            if (ignored.Count > 0)
            {
                yield return new WaitForSeconds(0.35f);
                foreach (var hit in ignored)
                {
                    if (hit != null && _col != null)
                    {
                        Physics2D.IgnoreCollision(_col, hit, false);
                    }
                }
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
            if (_state == PlayerState.Dead || _state == PlayerState.Awakening || _controlsLocked)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

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
            float xVel = Mathf.Abs(_currentHorizontalInput) > 0.05f ? _currentHorizontalInput * moveSpeed : 0f;
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
                animator.SetTrigger("Attack");
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

            if (hasFireballPower)
            {
                CastFireball();
            }
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
        public bool HasChronosAura => hasChronosAura;
        public bool ControlsLocked => _controlsLocked;

        /// <summary>
        /// Locks or unlocks player control inputs while maintaining current state / animation cleanly.
        /// </summary>
        public void LockControls(bool locked)
        {
            _controlsLocked = locked;
            if (locked)
            {
                _currentHorizontalInput = 0f;
                _currentVerticalInput = 0f;
                _moveInput = Vector2.zero;
                _jumpPressed = false;
                _attackPressed = false;
                _dashPressed = false;
                _attackBufferTimer = 0f;
                _jumpBufferTimer = 0f;
                if (_isGrounded && _state != PlayerState.Dead && _state != PlayerState.Awakening)
                {
                    SetState(PlayerState.Idle);
                }
            }
        }

        /// <summary>
        /// Unlocks full melee attack combat for Aeron.
        /// </summary>
        public void UnlockCombat()
        {
            lockToWalkAndJumpOnly = false;
        }

        /// <summary>
        /// Triggers Aeron's cinematic awakening sequence post-stasis chamber rupture.
        /// Enters PlayerState.Awakening, triggers cyan surge, activates Chronos Aura, and unlocks combat.
        /// </summary>
        public void TriggerAwakening(float duration = 2.5f, Action onComplete = null)
        {
            if (_awakeningCoroutine != null) StopCoroutine(_awakeningCoroutine);
            _awakeningCoroutine = StartCoroutine(AwakeningRoutine(duration, onComplete));
        }

        private System.Collections.IEnumerator AwakeningRoutine(float duration, Action onComplete)
        {
            SetState(PlayerState.Awakening);
            _controlsLocked = true;
            _rb.linearVelocity = Vector2.zero;

            EnsureChronosAuraVisual();

            SpriteRenderer auraSr = chronosAuraVisual != null ? chronosAuraVisual.GetComponent<SpriteRenderer>() : null;
            if (auraSr != null && auraSr.sprite == null)
            {
#if UNITY_EDITOR
                auraSr.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Chronos_Aura_FX.png");
#endif
            }

            // Calculate exact target scale so halo diameter is ~2.4 world units around Aeron's chest
            float targetScale = 0.6f;
            if (auraSr != null && auraSr.sprite != null && auraSr.sprite.rect.width > 0)
            {
                float spriteWorldWidth = auraSr.sprite.rect.width / auraSr.sprite.pixelsPerUnit;
                targetScale = 2.4f / Mathf.Max(0.1f, spriteWorldWidth);
            }

            if (chronosAuraVisual != null)
            {
                chronosAuraVisual.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                chronosAuraVisual.transform.localScale = Vector3.zero;
                chronosAuraVisual.SetActive(true);
            }

#if UNITY_EDITOR
            if (awakeningVoiceClip == null)
            {
                awakeningVoiceClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/aeron_wakeup_voice.wav");
            }
#endif
            if (audioSource == null) audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            if (awakeningVoiceClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(awakeningVoiceClip);
            }

            if (animator != null)
            {
                animator.SetTrigger("Hurt"); // Visual surge pulse
            }

            // Animate Chronos Chakra surge:
            // Phase 1: Expand & flare up (0.0 to 0.4s)
            // Phase 2: Glow & rotate while voice plays (0.4s to duration - 0.6s)
            // Phase 3: Contract, fade out, and DISAPPEAR COMPLETELY (last 0.6s)
            float elapsed = 0f;
            float fadeOutStart = Mathf.Max(0.5f, duration - 0.6f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float currentScale = targetScale;
                float currentAlpha = 0.85f;

                if (elapsed < 0.4f)
                {
                    // Expanding burst
                    float t = elapsed / 0.4f;
                    currentScale = Mathf.Lerp(0.05f, targetScale * 1.15f, Mathf.SmoothStep(0f, 1f, t));
                    currentAlpha = Mathf.Lerp(0f, 0.85f, t);
                }
                else if (elapsed < fadeOutStart)
                {
                    // Rotating and gentle breathing pulse
                    float pulse = 1f + 0.06f * Mathf.Sin((elapsed - 0.4f) * 10f);
                    currentScale = targetScale * pulse;
                    currentAlpha = 0.85f;
                }
                else
                {
                    // Contracting and fading out smoothly
                    float t = (elapsed - fadeOutStart) / (duration - fadeOutStart);
                    currentScale = Mathf.Lerp(targetScale, 0f, Mathf.SmoothStep(0f, 1f, t));
                    currentAlpha = Mathf.Lerp(0.85f, 0f, t);
                }

                if (chronosAuraVisual != null)
                {
                    chronosAuraVisual.transform.localScale = new Vector3(currentScale, currentScale, 1f);
                    chronosAuraVisual.transform.Rotate(0f, 0f, -160f * Time.deltaTime);
                    if (auraSr != null)
                    {
                        auraSr.color = new Color(0f, 0.95f, 1.0f, currentAlpha);
                    }
                }

                yield return null;
            }

            // AT THE END OF THE SURGE: THE CIRCLE CHAKRA MUST GO (DEACTIVATE COMPLETELY)!
            if (chronosAuraVisual != null)
            {
                chronosAuraVisual.transform.localScale = Vector3.zero;
                chronosAuraVisual.SetActive(false);
            }

            hasChronosAura = true;
            hasFireballPower = true;
            lockToWalkAndJumpOnly = false; // Melee & Fireball combat unlocked!
            _controlsLocked = false;
            SetState(PlayerState.Idle);

            if (TryGetComponent<PrototypePowerController>(out var powerCtrl))
            {
                powerCtrl.UnlockPower(PrototypePowerType.TimeSlow);
            }

            _awakeningCoroutine = null;
            onComplete?.Invoke();
        }

        private void EnsureChronosAuraVisual()
        {
            if (chronosAuraVisual == null)
            {
                Transform auraT = transform.Find("ChronosAura");
                if (auraT != null)
                {
                    chronosAuraVisual = auraT.gameObject;
                }
                else
                {
                    GameObject auraObj = new GameObject("ChronosAura");
                    auraObj.transform.SetParent(transform, false);
                    auraObj.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                    auraObj.transform.localScale = Vector3.zero;

                    SpriteRenderer sr = auraObj.AddComponent<SpriteRenderer>();
                    sr.sortingLayerName = "Default";
                    sr.sortingOrder = 8; // Render BEHIND Aeron (order 10)
                    sr.color = new Color(0f, 0.9f, 1.0f, 0.8f);

#if UNITY_EDITOR
                    Sprite auraSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Chronos_Aura_FX.png");
                    if (auraSprite != null) sr.sprite = auraSprite;
#endif
                    chronosAuraVisual = auraObj;
                }
            }

            if (chronosAuraVisual != null)
            {
                chronosAuraVisual.SetActive(false); // MUST be inactive by default
            }
        }

        public void AwakenByGuardShot()
        {
            if (hasFireballPower || _state == PlayerState.Awakening) return;
            TriggerAwakening(2.5f, () => {
                Debug.Log("[PlayerController] Aeron's cyan/orange fireball power awakened!");
            });
        }

        public void CastFireball()
        {
            StartCoroutine(PerformCastRoutine());
        }

        private System.Collections.IEnumerator PerformCastRoutine()
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Frame 1 is aiming hand up (0.0 - 0.12s), Frame 2 releases the fire bullet (0.12s+)
            yield return new WaitForSeconds(0.12f);

            Vector3 spawnPos = fireballSpawnPoint != null 
                ? fireballSpawnPoint.position 
                : transform.position + new Vector3(_facingRight ? 1.3f : -1.3f, 1.4f, 0f);

            GameObject fb = null;
            if (fireballPrefab != null)
            {
                fb = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                fb = CreateFireballFallback(spawnPos);
            }

            if (fb != null && fb.TryGetComponent<LastGod.Combat.FireballProjectile>(out var proj))
            {
                proj.Initialize(_facingRight ? Vector2.right : Vector2.left, 4);
            }

            if (fireballCastSFX != null && audioSource != null)
            {
                audioSource.PlayOneShot(fireballCastSFX);
            }
        }

        private GameObject CreateFireballFallback(Vector3 pos)
        {
            GameObject fb = new GameObject("Fireball_Fallback");
            fb.transform.position = pos;
            var sr = fb.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 15;
#if UNITY_EDITOR
            sr.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Combat/Aeron_Fireball_FX.png");
#endif
            var col = fb.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;
            var rb = fb.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            fb.AddComponent<LastGod.Combat.FireballProjectile>();
            return fb;
        }

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
