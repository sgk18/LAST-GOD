using System;
using UnityEngine;
using LastGod.Core;
using LastGod.Combat;
using LastGod.Player;

namespace LastGod.Enemies
{
    public enum BringerState
    {
        Idle,
        Walk,
        MeleeAttack,
        CastSpell,
        Hurt,
        Dead
    }

    /// <summary>
    /// AI controller for the Bringer Of Death enemy.
    /// Features hovering pursuit, devastating scythe slashes, ranged dark necrotic magic,
    /// hurt recoil, and an ash-dissolve death sequence.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Health))]
    public class BringerOfDeathAI : MonoBehaviour
    {
        [Header("Movement & Range")]
        [SerializeField] private float moveSpeed = 2.8f;
        [SerializeField] private float detectionRange = 12f;
        [SerializeField] private float meleeRange = 2.2f;
        [SerializeField] private float castRangeMax = 9f;
        [SerializeField] private float castRangeMin = 3.5f;

        [Header("Combat Settings")]
        [SerializeField] private int meleeDamage = 3;
        [SerializeField] private float attackCooldown = 2.0f;
        [SerializeField] private float spellCooldown = 4.5f;

        [Header("Projectiles & Spawn Points")]
        [SerializeField] private GameObject darkMagicPrefab;
        [SerializeField] private Transform castSpawnPoint;

        [Header("Audio (SFX)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip scytheAttackSFX;
        [SerializeField] private AudioClip spellCastSFX;
        [SerializeField] private AudioClip hurtSFX;
        [SerializeField] private AudioClip deathSFX;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private Color hurtFlashColor = new Color(1f, 0.3f, 0.3f, 1f);

        private Rigidbody2D _rb;
        private Collider2D _col;
        private Health _health;
        private Transform _playerTarget;

        private BringerState _state = BringerState.Idle;
        private float _attackCooldownTimer;
        private float _spellCooldownTimer;
        private float _actionLockTimer;
        private float _hurtFlashTimer;
        private Color _originalColor = Color.white;
        private bool _facingRight = false; // Default Bringer of Death sprite faces left

        public BringerState State => _state;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _health = GetComponent<Health>();

            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null) animator = GetComponent<Animator>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            if (spriteRenderer != null) _originalColor = spriteRenderer.color;

            _rb.gravityScale = 2f;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            _health.OnDamaged.AddListener(OnDamaged);
            _health.OnDeath.AddListener(OnDeath);
        }

        private void Start()
        {
            // Scale melee reach dynamically according to enemy size
            float s = Mathf.Abs(transform.localScale.x);
            meleeRange = 2.0f * s;

            // Auto-acquire player
            FindPlayer();
        }

        private void FindPlayer()
        {
            if (_playerTarget != null) return;
            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) _playerTarget = pc.transform;
        }

        private void Update()
        {
            if (_state == BringerState.Dead) return;

            // Flash effect timer
            if (_hurtFlashTimer > 0f)
            {
                _hurtFlashTimer -= Time.deltaTime;
                if (_hurtFlashTimer <= 0f && spriteRenderer != null)
                {
                    spriteRenderer.color = _originalColor;
                }
            }

            // Timers
            if (_attackCooldownTimer > 0f) _attackCooldownTimer -= Time.deltaTime;
            if (_spellCooldownTimer > 0f) _spellCooldownTimer -= Time.deltaTime;

            if (_actionLockTimer > 0f)
            {
                _actionLockTimer -= Time.deltaTime;
                if (_actionLockTimer <= 0f)
                {
                    SetState(BringerState.Idle);
                }
                return;
            }

            if (_playerTarget == null)
            {
                FindPlayer();
                if (_playerTarget == null) return;
            }

            float distToPlayer = Vector2.Distance(transform.position, _playerTarget.position);

            // Turn to face player
            UpdateFacing();

            if (distToPlayer > detectionRange)
            {
                SetState(BringerState.Idle);
                return;
            }

            // In combat range: decide between melee, spell, or walk
            if (distToPlayer <= meleeRange && _attackCooldownTimer <= 0f)
            {
                ExecuteMeleeAttack();
            }
            else if (distToPlayer >= castRangeMin && distToPlayer <= castRangeMax && _spellCooldownTimer <= 0f && darkMagicPrefab != null)
            {
                ExecuteSpellCast();
            }
            else if (distToPlayer > meleeRange * 0.8f)
            {
                SetState(BringerState.Walk);
            }
            else
            {
                SetState(BringerState.Idle);
            }
        }

        private void FixedUpdate()
        {
            if (_state == BringerState.Dead || _actionLockTimer > 0f)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            if (_state == BringerState.Walk && _playerTarget != null)
            {
                float dirX = (_playerTarget.position.x > transform.position.x) ? 1f : -1f;
                _rb.linearVelocity = new Vector2(dirX * moveSpeed, _rb.linearVelocity.y);
            }
            else
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            }
        }

        private void UpdateFacing()
        {
            if (_playerTarget == null) return;

            bool playerIsRight = (_playerTarget.position.x > transform.position.x);
            // Default sprite faces left (false). When player is to the right, flipX = true.
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = playerIsRight;
            }
            _facingRight = playerIsRight;
        }

        private void SetState(BringerState next)
        {
            if (_state == next) return;
            _state = next;

            if (animator == null) return;

            switch (_state)
            {
                case BringerState.Idle:
                    animator.Play("Idle");
                    break;
                case BringerState.Walk:
                    animator.Play("Walk");
                    break;
                case BringerState.MeleeAttack:
                    animator.Play("Attack");
                    break;
                case BringerState.CastSpell:
                    animator.Play("Spell");
                    break;
                case BringerState.Hurt:
                    animator.Play("Hurt");
                    break;
                case BringerState.Dead:
                    animator.Play("Death");
                    break;
            }
        }

        private void ExecuteMeleeAttack()
        {
            SetState(BringerState.MeleeAttack);
            _attackCooldownTimer = attackCooldown;
            _actionLockTimer = 0.85f;

            PlaySFX(scytheAttackSFX);

            // Delay melee hitbox check to match the scythe swing frame
            Invoke(nameof(PerformScytheImpact), 0.35f);
        }

        private void PerformScytheImpact()
        {
            if (_state != BringerState.MeleeAttack) return;

            float dirX = _facingRight ? 1f : -1f;
            float s = Mathf.Abs(transform.localScale.x);
            Vector2 hitCenter = (Vector2)transform.position + new Vector2(dirX * 1.3f * s, 0.4f * s);
            float radius = 1.3f * s;

            Collider2D[] hits = Physics2D.OverlapCircleAll(hitCenter, radius);
            foreach (var h in hits)
            {
                if (h.CompareTag("Player") || h.GetComponent<PlayerController>() != null)
                {
                    var pc = h.GetComponent<PlayerController>();
                    if (pc != null && pc.IsInvincible) continue; // Player dashed through

                    if (h.TryGetComponent<IDamageable>(out var dmg))
                    {
                        dmg.TakeDamage(meleeDamage, new Vector2(dirX * 4f, 2f));
                    }
                }
            }
        }

        private void ExecuteSpellCast()
        {
            SetState(BringerState.CastSpell);
            _spellCooldownTimer = spellCooldown;
            _actionLockTimer = 1.0f;

            PlaySFX(spellCastSFX);

            Invoke(nameof(SpawnSpellProjectile), 0.45f);
        }

        private void SpawnSpellProjectile()
        {
            if (_state != BringerState.CastSpell || darkMagicPrefab == null) return;

            Vector3 spawnPos = castSpawnPoint != null ? castSpawnPoint.position : transform.position + new Vector3(_facingRight ? 1f : -1f, 0.5f, 0f);
            GameObject projObj = Instantiate(darkMagicPrefab, spawnPos, Quaternion.identity);

            if (projObj.TryGetComponent<DarkMagicProjectile>(out var proj))
            {
                Vector2 shootDir = (_playerTarget != null) 
                    ? ((Vector2)(_playerTarget.position + Vector3.up * 0.5f) - (Vector2)spawnPos).normalized 
                    : (_facingRight ? Vector2.right : Vector2.left);

                proj.Initialize(shootDir, _playerTarget, 2);
            }
        }

        private void OnDamaged(int remainingHP)
        {
            if (_state == BringerState.Dead) return;

            SetState(BringerState.Hurt);
            _actionLockTimer = 0.35f;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = hurtFlashColor;
                _hurtFlashTimer = 0.18f;
            }

            PlaySFX(hurtSFX);
        }

        private void OnDeath()
        {
            SetState(BringerState.Dead);
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false; // Disable physics
            if (_col != null) _col.enabled = false; // Disable collisions

            if (spriteRenderer != null)
            {
                spriteRenderer.color = _originalColor;
            }

            PlaySFX(deathSFX);
            Debug.Log("[BringerOfDeath] Slain by Aeron.");

            // Destroy game object after death animation completes
            Destroy(gameObject, 1.6f);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw melee arc
            float dirX = _facingRight ? 1f : -1f;
            Vector3 hitCenter = transform.position + new Vector3(dirX * 1.5f, 0.4f, 0f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(hitCenter, 1.4f);

            // Draw detection range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
