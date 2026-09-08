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
        [SerializeField] private bool shootOnly = true;
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float attackRange = 12f;
        [SerializeField] private float stopDistance = 2f;

        [Header("Attack")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float attackCooldown = 1.8f;
        [SerializeField] private AudioClip gunshotSFX;

        [Header("Sprite / Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;
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
        private bool _hasAwakenedAeron = false;

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

#if UNITY_EDITOR
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Guard/Guard_Idle_0.png") ??
                                       UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Guard_Spritesheet.png");
            }
            if (gunshotSFX == null)
            {
                gunshotSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/gunshot.wav");
            }
#endif
        }

        public void SetTarget(Transform target)
        {
            _targetPlayer = target;
            SetState(shootOnly ? EnemyState.Idle : EnemyState.Approach);
        }

        private void Update()
        {
            if (_state == EnemyState.Dead) return;

            if (_hurtTimer > 0f)
            {
                _hurtTimer -= Time.deltaTime;
                if (_hurtTimer <= 0f && _state == EnemyState.Hurt)
                {
                    SetState(shootOnly ? EnemyState.Idle : EnemyState.Approach);
                }
                return;
            }

            if (_attackTimer > 0f)
            {
                _attackTimer -= Time.deltaTime;
            }

            if (_targetPlayer == null)
            {
                var pc = FindAnyObjectByType<LastGod.Player.PlayerController>();
                if (pc != null) _targetPlayer = pc.transform;
                if (_targetPlayer == null) return;
            }

            float distToPlayer = Vector2.Distance(transform.position, _targetPlayer.position);

            // Face player
            float dirX = _targetPlayer.position.x - transform.position.x;
            if (dirX > 0.1f && !_facingRight) Flip();
            else if (dirX < -0.1f && _facingRight) Flip();

            if (shootOnly)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                if (distToPlayer <= attackRange)
                {
                    if (_attackTimer <= 0f)
                    {
                        ExecuteAttack();
                    }
                    else
                    {
                        SetState(EnemyState.Idle);
                        if (animator != null) animator.SetBool("IsWalking", false);
                    }
                }
                return;
            }

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
                    if (animator != null) animator.SetBool("IsWalking", false);
                }
            }
            else
            {
                SetState(EnemyState.Approach);
                if (animator != null) animator.SetBool("IsWalking", true);
            }
        }

        private void FixedUpdate()
        {
            if (shootOnly)
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

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

            if (animator != null)
            {
                animator.SetTrigger("Shoot");
            }

            if (gunshotSFX != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(gunshotSFX);
            }

            if (_targetPlayer != null)
            {
                Vector3 spawnPos = firePoint != null ? firePoint.position : (transform.position + new Vector3(_facingRight ? 0.8f : -0.8f, 0.5f, 0f));
                GameObject bObj = bulletPrefab != null ? Instantiate(bulletPrefab, spawnPos, Quaternion.identity) : CreateLaserBullet(spawnPos);

                if (bObj != null && bObj.TryGetComponent<Bullet>(out var bullet))
                {
                    Vector2 shootDir = (_targetPlayer.position - spawnPos).normalized;
                    bullet.Initialize(shootDir, _targetPlayer);
                }
            }
        }

        private GameObject CreateLaserBullet(Vector3 pos)
        {
            GameObject bObj = new GameObject("LaserBullet");
            bObj.transform.position = pos;

            var sr = bObj.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 12;

#if UNITY_EDITOR
            Sprite laserSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Laser_Bullet_FX.png") ??
                                 UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/Bullet.png");
            if (laserSprite != null) sr.sprite = laserSprite;
#endif
            bObj.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            var col = bObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.25f;

            var rb = bObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

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
            _hurtTimer = 0.3f;
        }

        private void OnDeath()
        {
            SetState(EnemyState.Dead);
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false; // Stop physics, bodies stay on ground
            if (_col != null) _col.enabled = false;

            if (spriteRenderer != null)
            {
                if (deadSprite != null)
                {
                    spriteRenderer.sprite = deadSprite;
                }
                else
                {
                    spriteRenderer.color = new Color(0.45f, 0.4f, 0.45f, 0.85f);
                    transform.rotation = Quaternion.Euler(0f, 0f, _facingRight ? -90f : 90f);
                }
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
