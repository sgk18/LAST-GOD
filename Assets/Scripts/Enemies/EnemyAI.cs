using System;
using UnityEngine;
using LastGod.Core;
using LastGod.Combat;

namespace LastGod.Enemies
{
    public enum EnemyState
    {
        Idle,
        Approach,
        Attack,
        Hurt,
        Dead
    }

    /// <summary>
    /// Minimal Guard AI for Act 1 Scene 1.
    /// Reuses state machine pattern & IDamageable / Health foundation.
    /// Approaches Aeron, fires bullets, stays on ground upon death.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    [RequireComponent(typeof(Health))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float stopDistance = 2f;

        [Header("Attack")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private AudioClip gunshotSFX;

        [Header("Sprite / Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite deadSprite;

        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        private Health _health;
        private AudioSource _audioSource;

        private Transform _targetPlayer;
        private EnemyState _state = EnemyState.Idle;
        private float _attackTimer;
        private float _hurtTimer;
        private bool _facingRight = true;

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
        }

        public void SetTarget(Transform target)
        {
            _targetPlayer = target;
            SetState(EnemyState.Approach);
        }

        private void Update()
        {
            if (_state == EnemyState.Dead) return;

            if (_hurtTimer > 0f)
            {
                _hurtTimer -= Time.deltaTime;
                if (_hurtTimer <= 0f && _state == EnemyState.Hurt)
                {
                    SetState(EnemyState.Approach);
                }
                return;
            }

            if (_attackTimer > 0f)
            {
                _attackTimer -= Time.deltaTime;
            }

            if (_targetPlayer == null) return;

            float distToPlayer = Vector2.Distance(transform.position, _targetPlayer.position);

            // Face player
            float dirX = _targetPlayer.position.x - transform.position.x;
            if (dirX > 0.1f && !_facingRight) Flip();
            else if (dirX < -0.1f && _facingRight) Flip();

            if (distToPlayer <= attackRange)
            {
                if (_attackTimer <= 0f)
                {
                    ExecuteAttack();
                }
                else if (distToPlayer <= stopDistance)
                {
                    SetState(EnemyState.Idle);
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                }
            }
            else
            {
                SetState(EnemyState.Approach);
            }
        }

        private void FixedUpdate()
        {
            if (_state == EnemyState.Approach && _targetPlayer != null)
            {
                float dir = (_targetPlayer.position.x > transform.position.x) ? 1f : -1f;
                _rb.linearVelocity = new Vector2(dir * moveSpeed, _rb.linearVelocity.y);
            }
            else if (_state != EnemyState.Approach)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
            }
        }

        private void ExecuteAttack()
        {
            SetState(EnemyState.Attack);
            _attackTimer = attackCooldown;

            if (gunshotSFX != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(gunshotSFX);
            }

            if (bulletPrefab != null && _targetPlayer != null)
            {
                Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
                GameObject bObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
                if (bObj.TryGetComponent<Bullet>(out var bullet))
                {
                    Vector2 shootDir = (_targetPlayer.position - spawnPos).normalized;
                    bullet.Initialize(shootDir, _targetPlayer);
                }
            }
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
            _hurtTimer = 0.3f;
        }

        private void OnDeath()
        {
            SetState(EnemyState.Dead);
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false; // Stop physics, bodies stay on ground
            if (_col != null) _col.enabled = false;

            if (spriteRenderer != null && deadSprite != null)
            {
                spriteRenderer.sprite = deadSprite;
            }
        }

        private void Flip()
        {
            _facingRight = !_facingRight;
            Vector3 s = transform.localScale;
            s.x = -s.x;
            transform.localScale = s;
        }
    }
}
