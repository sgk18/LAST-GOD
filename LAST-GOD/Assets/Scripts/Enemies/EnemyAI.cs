using System;
using System.Collections;
using UnityEngine;
using LastGod.Core;
using LastGod.Combat;

namespace LastGod.Enemies
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Hurt,
        Dead
    }

    /// <summary>
    /// Smart CyberGuard AI for Act 1 Platformer.
    /// Features:
    /// - Proximity & Line-of-Sight detection against Aeron
    /// - 2D Platform Pathfinding: edge/drop-off awareness, wall detection, platform patrol
    /// - Animation-synchronized combat: rifle aim -> muzzle flash particle burst -> laser bullet -> recoil/recovery
    /// - Triggers Aeron's awakening sequence on opening volley
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    [RequireComponent(typeof(Health))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Detection & Vision")]
        [SerializeField] private float detectionRange = 14f;
        [SerializeField] private float verticalSightRange = 6f;
        [SerializeField] private LayerMask visionObstacleMask;

        [Header("Movement & Platform Pathfinding")]
        [SerializeField] private float patrolSpeed = 2.2f;
        [SerializeField] private float chaseSpeed = 3.4f;
        [SerializeField] private float minAttackRange = 3.5f;
        [SerializeField] private float maxAttackRange = 9.0f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float edgeCheckForwardOffset = 0.8f;
        [SerializeField] private float edgeCheckDownDistance = 1.6f;
        [SerializeField] private float wallCheckDistance = 0.8f;

        [Header("Combat & VFX Assets")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float attackCooldown = 1.8f;
        [SerializeField] private AudioClip gunshotSFX;

        [Header("Visuals & Animation")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Sprite deadSprite;

        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private Health _health;
        private AudioSource _audioSource;

        private Transform _targetPlayer;
        private EnemyState _state = EnemyState.Patrol;
        private float _attackTimer;
        private float _hurtTimer;
        private float _patrolPauseTimer;
        private float _moveDir = 1f;
        private bool _facingRight = true;
        private bool _hasAwakenedAeron = false;
        private Coroutine _shootCoroutine;

        public EnemyState State => _state;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();
            _health = GetComponent<Health>();
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

            _health.OnDamaged.AddListener(OnDamaged);
            _health.OnDeath.AddListener(OnDeath);

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = "Default";
            spriteRenderer.sortingOrder = 9;

            if (animator == null) animator = GetComponent<Animator>();

            if (firePoint == null)
            {
                Transform fp = transform.Find("FirePoint");
                if (fp != null) firePoint = fp;
            }

            if (groundLayer.value == 0)
            {
                groundLayer = LayerMask.GetMask("Ground") | (1 << 8);
            }

            if (!gameObject.CompareTag("Enemy"))
            {
                try { gameObject.tag = "Enemy"; } catch {}
            }
            _facingRight = transform.localScale.x >= 0f;

#if UNITY_EDITOR
            if (bulletPrefab == null)
            {
                bulletPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/Guard_Laser_Bullet.prefab");
            }
            if (muzzleFlashPrefab == null)
            {
                muzzleFlashPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/VFX_MuzzleFlash_Cyber.prefab");
            }
            if (gunshotSFX == null)
            {
                gunshotSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/gunshot.wav");
            }
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Guard/Guard_Idle_0.png") ??
                                       UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Guard_Spritesheet.png");
            }
#endif
        }

        public void SetTarget(Transform target)
        {
            _targetPlayer = target;
        }

        private void Update()
        {
            if (_state == EnemyState.Dead) return;

            if (_hurtTimer > 0f)
            {
                _hurtTimer -= Time.deltaTime;
                if (_hurtTimer <= 0f && _state == EnemyState.Hurt)
                {
                    SetState(EnemyState.Patrol);
                }
                return;
            }

            if (_attackTimer > 0f)
            {
                _attackTimer -= Time.deltaTime;
            }

            // Target acquisition
            if (_targetPlayer == null)
            {
                var pc = FindAnyObjectByType<LastGod.Player.PlayerController>();
                if (pc != null) _targetPlayer = pc.transform;
            }

            // Don't steer or interrupt while actively in shooting animation
            if (_state == EnemyState.Attack)
            {
                return;
            }

            bool canSeePlayer = CanSeePlayer();

            if (canSeePlayer && _targetPlayer != null)
            {
                // ─── CHASE & ENGAGE BEHAVIOR ─────────────────────────────────
                float distToPlayer = Vector2.Distance(transform.position, _targetPlayer.position);
                float dirX = Mathf.Sign(_targetPlayer.position.x - transform.position.x);

                // Face player
                if (dirX > 0.05f && !_facingRight) Flip();
                else if (dirX < -0.05f && _facingRight) Flip();

                if (distToPlayer >= minAttackRange && distToPlayer <= maxAttackRange)
                {
                    // In prime shooting range! Halt and attack
                    _moveDir = 0f;
                    SetState(EnemyState.Idle);
                    if (animator != null) animator.SetBool("IsWalking", false);

                    if (_attackTimer <= 0f)
                    {
                        ExecuteAttack();
                    }
                }
                else if (distToPlayer > maxAttackRange)
                {
                    // Aeron is far away - approach across platform
                    bool groundAhead = IsGroundAhead(dirX);
                    bool wallAhead = IsWallAhead(dirX);

                    if (groundAhead && !wallAhead)
                    {
                        _moveDir = dirX;
                        SetState(EnemyState.Chase);
                        if (animator != null) animator.SetBool("IsWalking", true);
                    }
                    else if (!groundAhead)
                    {
                        // Check if Aeron is below us (drop down opportunity)
                        if (_targetPlayer.position.y < transform.position.y - 1.2f)
                        {
                            _moveDir = dirX;
                            SetState(EnemyState.Chase);
                            if (animator != null) animator.SetBool("IsWalking", true);
                        }
                        else
                        {
                            // Edge of platform, stop and hold position
                            _moveDir = 0f;
                            SetState(EnemyState.Idle);
                            if (animator != null) animator.SetBool("IsWalking", false);

                            // Shoot across gap if cooldown ready
                            if (_attackTimer <= 0f && distToPlayer <= maxAttackRange * 1.25f)
                            {
                                ExecuteAttack();
                            }
                        }
                    }
                    else
                    {
                        // Wall ahead
                        _moveDir = 0f;
                        SetState(EnemyState.Idle);
                        if (animator != null) animator.SetBool("IsWalking", false);
                    }
                }
                else
                {
                    // Closer than minAttackRange - shoot immediately
                    _moveDir = 0f;
                    SetState(EnemyState.Idle);
                    if (animator != null) animator.SetBool("IsWalking", false);

                    if (_attackTimer <= 0f)
                    {
                        ExecuteAttack();
                    }
                }
            }
            else
            {
                // ─── PATROL BEHAVIOR ─────────────────────────────────────────
                if (_patrolPauseTimer > 0f)
                {
                    _patrolPauseTimer -= Time.deltaTime;
                    _moveDir = 0f;
                    SetState(EnemyState.Idle);
                    if (animator != null) animator.SetBool("IsWalking", false);
                }
                else
                {
                    float currentDir = _facingRight ? 1f : -1f;
                    bool groundAhead = IsGroundAhead(currentDir);
                    bool wallAhead = IsWallAhead(currentDir);

                    if (!groundAhead || wallAhead)
                    {
                        // Reverse patrol direction at platform edge or obstacle
                        Flip();
                        _patrolPauseTimer = 0.8f;
                        _moveDir = 0f;
                        SetState(EnemyState.Idle);
                        if (animator != null) animator.SetBool("IsWalking", false);
                    }
                    else
                    {
                        _moveDir = currentDir;
                        SetState(EnemyState.Patrol);
                        if (animator != null) animator.SetBool("IsWalking", true);
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (_state == EnemyState.Dead) return;

            if (_state == EnemyState.Attack || _state == EnemyState.Hurt || _state == EnemyState.Idle)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            float currentSpeed = (_state == EnemyState.Chase) ? chaseSpeed : patrolSpeed;
            _rb.linearVelocity = new Vector2(_moveDir * currentSpeed, _rb.linearVelocity.y);
        }

        private bool CanSeePlayer()
        {
            if (_targetPlayer == null) return false;

            Vector2 eyePos = (Vector2)transform.position + Vector2.up * 1.5f;
            Vector2 playerPos = (Vector2)_targetPlayer.position + Vector2.up * 1.0f;
            Vector2 toPlayer = playerPos - eyePos;
            float dist = toPlayer.magnitude;

            if (dist > detectionRange) return false;
            if (Mathf.Abs(playerPos.y - eyePos.y) > verticalSightRange) return false;

            LayerMask obstacleMask = visionObstacleMask.value != 0
                ? visionObstacleMask
                : (groundLayer.value != 0 ? groundLayer : (LayerMask.GetMask("Ground") | (1 << 8)));

            RaycastHit2D hit = Physics2D.Raycast(eyePos, toPlayer.normalized, dist, obstacleMask);
            if (hit.collider != null && hit.transform != _targetPlayer && !hit.transform.IsChildOf(_targetPlayer))
            {
                // Sightline blocked by platform or terrain
                return false;
            }

            return true;
        }

        private bool IsGroundAhead(float dir)
        {
            Vector2 origin = (Vector2)transform.position + new Vector2(dir * edgeCheckForwardOffset, -0.4f);
            LayerMask mask = groundLayer.value != 0 ? groundLayer : (LayerMask.GetMask("Ground") | (1 << 8));
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, edgeCheckDownDistance, mask);
            return hit.collider != null;
        }

        private bool IsWallAhead(float dir)
        {
            Vector2 origin = (Vector2)transform.position + Vector2.up * 1.0f;
            LayerMask mask = groundLayer.value != 0 ? groundLayer : (LayerMask.GetMask("Ground") | (1 << 8));
            RaycastHit2D hit = Physics2D.Raycast(origin, new Vector2(dir, 0f), wallCheckDistance, mask);
            return hit.collider != null && !hit.collider.isTrigger;
        }

        private void ExecuteAttack()
        {
            SetState(EnemyState.Attack);
            _attackTimer = attackCooldown;
            _moveDir = 0f;
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);

            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
                animator.SetTrigger("Shoot");
            }

            if (_shootCoroutine != null)
            {
                StopCoroutine(_shootCoroutine);
            }
            _shootCoroutine = StartCoroutine(PerformShootRoutine());
        }

        private IEnumerator PerformShootRoutine()
        {
            // Frame 0: Aiming rifle (0.00s to 0.125s)
            yield return new WaitForSeconds(0.125f);

            // Frame 1: Rifle fires at 0.125s
            Vector3 spawnPos = firePoint != null
                ? firePoint.position
                : transform.position + new Vector3(_facingRight ? 1.0f : -1.0f, 2.2f, 0f);

            // 1. Muzzle Flash Particle VFX Asset
            if (muzzleFlashPrefab != null)
            {
                Instantiate(muzzleFlashPrefab, spawnPos, firePoint != null ? firePoint.rotation : Quaternion.identity);
            }

            // 2. Audio SFX
            if (gunshotSFX != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(gunshotSFX, 1.0f);
            }

            // 3. Laser Bullet Projectile Prefab Asset
            if (_targetPlayer != null)
            {
                Vector3 targetAim = _targetPlayer.position + Vector3.up * 0.8f;
                Vector2 shootDir = (targetAim - spawnPos).normalized;

                GameObject bObj = bulletPrefab != null
                    ? Instantiate(bulletPrefab, spawnPos, Quaternion.identity)
                    : CreateLaserBullet(spawnPos);

                if (bObj != null && bObj.TryGetComponent<Bullet>(out var bullet))
                {
                    bullet.Initialize(shootDir, _targetPlayer);
                }

                // Awaken Aeron on the opening shot
                if (!_hasAwakenedAeron)
                {
                    _hasAwakenedAeron = true;
                    if (_targetPlayer.TryGetComponent<LastGod.Player.PlayerController>(out var pc))
                    {
                        pc.AwakenByGuardShot();
                    }
                }
            }

            // Frame 2 & 3: Recoil and recovery (0.375s)
            yield return new WaitForSeconds(0.375f);

            if (_state == EnemyState.Attack)
            {
                SetState(EnemyState.Idle);
            }
            _shootCoroutine = null;
        }

        private GameObject CreateLaserBullet(Vector3 pos)
        {
            GameObject bObj = new GameObject("LaserBullet");
            bObj.transform.position = pos;

            var sr = bObj.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 14;

#if UNITY_EDITOR
            Sprite laserSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Guard/Guard_Bullet_0.png") ??
                                 UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Laser_Bullet_FX.png");
            if (laserSprite != null) sr.sprite = laserSprite;
#endif
            bObj.transform.localScale = Vector3.one;

            var col = bObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;

            var rb = bObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            bObj.AddComponent<Bullet>();
            return bObj;
        }

        private void SetState(EnemyState next)
        {
            if (_state == next) return;
            _state = next;
        }

        private void OnDamaged(int remainingHP)
        {
            if (_state == EnemyState.Dead) return;
            SetState(EnemyState.Hurt);
            _hurtTimer = 0.25f;
            _moveDir = 0f;
            _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        }

        private void OnDeath()
        {
            SetState(EnemyState.Dead);
            if (_shootCoroutine != null) StopCoroutine(_shootCoroutine);
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false; // Stop physics, bodies remain resting on ground
            if (_col != null) _col.enabled = false;

            if (animator != null) animator.enabled = false;

            if (spriteRenderer != null)
            {
                if (deadSprite != null)
                {
                    spriteRenderer.sprite = deadSprite;
                }
                else
                {
                    spriteRenderer.color = new Color(0.4f, 0.4f, 0.45f, 0.85f);
                    transform.rotation = Quaternion.Euler(0f, 0f, _facingRight ? -90f : 90f);
                }
            }
        }

        private void Flip()
        {
            _facingRight = !_facingRight;
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (_facingRight ? 1f : -1f);
            transform.localScale = s;
        }

        private void OnDrawGizmosSelected()
        {
            // Detection sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 1.5f, detectionRange);

            // Combat ranges
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxAttackRange);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, minAttackRange);

            // Edge raycast
            float dir = _facingRight ? 1f : -1f;
            Gizmos.color = Color.green;
            Vector3 edgeStart = transform.position + new Vector3(dir * edgeCheckForwardOffset, -0.4f, 0f);
            Gizmos.DrawLine(edgeStart, edgeStart + Vector3.down * edgeCheckDownDistance);

            // Wall raycast
            Gizmos.color = Color.cyan;
            Vector3 wallStart = transform.position + Vector3.up * 1.0f;
            Gizmos.DrawLine(wallStart, wallStart + new Vector3(dir * wallCheckDistance, 0f, 0f));
        }
    }
}
