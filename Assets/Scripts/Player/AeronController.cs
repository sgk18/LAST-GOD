using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LastGod.Act1
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class AeronController : MonoBehaviour
    {
        public static AeronController Instance { get; private set; }

        [Header("Movement")]
        public float moveSpeed = 5.0f;
        public float dashSpeed = 16.0f;
        public float dashDuration = 0.22f;
        public float dashCooldown = 0.45f;

        [Header("Combat Stats")]
        public float maxLife = 100f;
        public float currentLife = 100f;
        public float maxFocus = 100f;
        public float currentFocus = 100f;
        public float humanity = 100f; // 0 - 100%
        public float baseAttackDamage = 18f;

        [Header("Time Manipulation")]
        public float bulletSlowRadius = 3.2f;
        public float bulletSlowFactor = 0.25f;
        public float timeDilationScale = 0.25f;

        [Header("Visual & Effects")]
        public SpriteRenderer spriteRenderer;
        public SpriteRenderer eyeGlowRenderer;
        public Animator animator;
        public Transform attackPoint;
        public float attackRadius = 1.4f;
        public LayerMask enemyLayer;
        public LayerMask bulletLayer;
        public GameObject slashEffectPrefab;
        public GameObject timeRipplePrefab;
        public GameObject absorbVfxPrefab;

        // Runtime states
        public enum AeronState { Suspended, Awakening, Collapsed, Idle, Walking, Attacking, Dashing, Absorbing, Hurt, Dead }
        public AeronState CurrentState { get; private set; } = AeronState.Suspended;

        public bool IsInvulnerable => isInvulnerable;
        public bool IsDilatingTime => isDilatingTime;
        public float FacingDirection { get; private set; } = 1f;

        private Rigidbody2D rb;
        private Collider2D col;
        private bool isInvulnerable = false;
        private bool isDilatingTime = false;
        private float dashTimer = 0f;
        private float dashCooldownTimer = 0f;
        private float attackTimer = 0f;
        private float absorbTimer = 0f;

        private void Awake()
        {
            Instance = this;
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null) animator = GetComponent<Animator>();
        }

        private void Start()
        {
            currentLife = maxLife;
            currentFocus = maxFocus;
            humanity = 100f;
        }

        private void Update()
        {
            // Cooldowns
            if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.unscaledDeltaTime;

            if (CurrentState == AeronState.Suspended || CurrentState == AeronState.Awakening || CurrentState == AeronState.Dead)
                return;

            HandleInput();
            HandleTimeDilation();
            UpdateVisuals();
        }

        private void HandleInput()
        {
            if (CurrentState == AeronState.Dashing || CurrentState == AeronState.Absorbing || CurrentState == AeronState.Hurt)
                return;

            // Movement Inputs (A/D or Arrows)
            float h = Input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(h) > 0.1f)
            {
                FacingDirection = Mathf.Sign(h);
                if (spriteRenderer != null) spriteRenderer.flipX = (FacingDirection < 0);
                if (attackPoint != null)
                {
                    Vector3 localPos = attackPoint.localPosition;
                    localPos.x = Mathf.Abs(localPos.x) * FacingDirection;
                    attackPoint.localPosition = localPos;
                }
            }

            // State updates for movement
            if (CurrentState != AeronState.Attacking)
            {
                if (Mathf.Abs(h) > 0.1f)
                {
                    rb.linearVelocity = new Vector2(h * moveSpeed, rb.linearVelocity.y);
                    CurrentState = AeronState.Walking;
                }
                else
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    CurrentState = AeronState.Idle;
                }
            }

            // Attack (J / Left Click / Z)
            if ((Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0)) && CurrentState != AeronState.Attacking)
            {
                PerformAttack();
            }

            // Dash (K / Right Shift / X / Space while moving)
            if ((Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift)) && dashCooldownTimer <= 0f)
            {
                StartCoroutine(DashRoutine());
            }

            // Take / Absorb Broken Enemy (L / E / C)
            if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.C))
            {
                TryTakeBrokenEnemy();
            }
        }

        private void HandleTimeDilation()
        {
            // Hold Space or RMB to Dilate Time
            bool wantDilate = (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(1)) && currentFocus > 0f && CurrentState != AeronState.Suspended;
            
            if (wantDilate)
            {
                isDilatingTime = true;
                currentFocus -= 35f * Time.unscaledDeltaTime;
                Time.timeScale = timeDilationScale;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
            }
            else
            {
                isDilatingTime = false;
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = 0.02f;

                // Passive focus recovery (faster recovery as Humanity decreases!)
                float focusRegen = 8f + (100f - humanity) * 0.12f;
                currentFocus = Mathf.Min(maxFocus, currentFocus + focusRegen * Time.deltaTime);
            }
        }

        private void PerformAttack()
        {
            CurrentState = AeronState.Attacking;
            rb.linearVelocity = new Vector2(FacingDirection * 2.5f, rb.linearVelocity.y); // slight lunge

            // Spawn visual slash
            if (slashEffectPrefab != null && attackPoint != null)
            {
                GameObject slash = Instantiate(slashEffectPrefab, attackPoint.position, Quaternion.identity);
                if (FacingDirection < 0)
                {
                    Vector3 s = slash.transform.localScale;
                    s.x = -Mathf.Abs(s.x);
                    slash.transform.localScale = s;
                }
                Destroy(slash, 0.25f);
            }

            // Damage multiplier scales as Humanity is sacrificed!
            float damageMultiplier = 1.0f + ((100f - humanity) / 100f) * 0.65f;
            float totalDamage = baseAttackDamage * damageMultiplier;

            // Hit detection: Enemies
            Vector2 checkPos = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position + Vector2.right * (FacingDirection * 1.0f);
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(checkPos, attackRadius, enemyLayer);
            foreach (var hit in hitEnemies)
            {
                var guard = hit.GetComponent<GuardAI>();
                if (guard != null)
                {
                    guard.TakeDamage(totalDamage, FacingDirection);
                    currentFocus = Mathf.Min(maxFocus, currentFocus + 8f);
                }
            }

            // Bullet Reflection Mechanic!
            Collider2D[] hitBullets = Physics2D.OverlapCircleAll(checkPos, attackRadius * 1.2f, bulletLayer);
            foreach (var bCol in hitBullets)
            {
                var bullet = bCol.GetComponent<Bullet>();
                if (bullet != null && !bullet.IsReflected)
                {
                    bullet.Reflect(FacingDirection);
                    currentFocus = Mathf.Min(maxFocus, currentFocus + 15f);
                    GameManager.Instance?.RecordReflect();
                }
            }

            StartCoroutine(AttackCooldownRoutine());
        }

        private IEnumerator AttackCooldownRoutine()
        {
            yield return new WaitForSeconds(0.28f);
            if (CurrentState == AeronState.Attacking)
                CurrentState = AeronState.Idle;
        }

        private IEnumerator DashRoutine()
        {
            CurrentState = AeronState.Dashing;
            isInvulnerable = true;
            dashCooldownTimer = dashCooldown;

            float dashDir = FacingDirection;
            float elapsed = 0f;

            while (elapsed < dashDuration)
            {
                rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            isInvulnerable = false;
            CurrentState = AeronState.Idle;
        }

        private void TryTakeBrokenEnemy()
        {
            // Find nearest broken guard
            GuardAI targetGuard = GuardAI.GetNearestBrokenGuard(transform.position, 2.5f);
            if (targetGuard != null)
            {
                StartCoroutine(AbsorbRoutine(targetGuard));
            }
        }

        private IEnumerator AbsorbRoutine(GuardAI guard)
        {
            CurrentState = AeronState.Absorbing;
            rb.linearVelocity = Vector2.zero;

            // Heal and recharge
            currentLife = Mathf.Min(maxLife, currentLife + 28f);
            currentFocus = maxFocus;

            // Sacrifice Humanity!
            humanity = Mathf.Max(0f, humanity - 12f);

            // Notify Guard to disintegrate into energy
            guard.AbsorbByAeron();
            GameManager.Instance?.RecordTake(humanity);

            if (absorbVfxPrefab != null)
            {
                GameObject vfx = Instantiate(absorbVfxPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 1.0f);
            }

            yield return new WaitForSeconds(0.65f);
            CurrentState = AeronState.Idle;
        }

        public void TakeDamage(float amount, float knockbackDir)
        {
            if (isInvulnerable || CurrentState == AeronState.Dead) return;

            currentLife -= amount;
            GameManager.Instance?.RecordHitTaken();

            if (currentLife <= 0f)
            {
                currentLife = 0f;
                CurrentState = AeronState.Dead;
                rb.linearVelocity = Vector2.zero;
                GameManager.Instance?.OnPlayerDied();
            }
            else
            {
                StartCoroutine(HurtRoutine(knockbackDir));
            }
        }

        private IEnumerator HurtRoutine(float knockbackDir)
        {
            CurrentState = AeronState.Hurt;
            isInvulnerable = true;
            rb.linearVelocity = new Vector2(knockbackDir * 3.5f, 1.5f);

            // Screen flash & shake
            CameraFollow2D.Instance?.Shake(0.2f, 0.25f);

            // Sprite flicker
            for (int i = 0; i < 4; i++)
            {
                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.4f, 0.4f, 0.5f);
                yield return new WaitForSeconds(0.06f);
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.06f);
            }

            isInvulnerable = false;
            CurrentState = AeronState.Idle;
        }

        public void TriggerEcho()
        {
            // Dashing through bullet
            currentFocus = Mathf.Min(maxFocus, currentFocus + 25f);
            GameManager.Instance?.RecordEcho();
            CameraFollow2D.Instance?.Shake(0.12f, 0.15f);
        }

        private void UpdateVisuals()
        {
            // Eye glow becomes more piercing as humanity drops
            if (eyeGlowRenderer != null)
            {
                float alpha = Mathf.Lerp(0.2f, 1.0f, (100f - humanity) / 100f);
                eyeGlowRenderer.color = new Color(0.5f, 0.95f, 0.85f, alpha);
            }

            // Drive animator movement parameters
            if (animator != null && rb != null)
            {
                float speed = Mathf.Abs(rb.linearVelocity.x);
                animator.SetFloat("Speed", speed);
            }
        }

        public void SetState(AeronState state)
        {
            CurrentState = state;
            if (state == AeronState.Collapsed)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 checkPos = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position + Vector2.right * (FacingDirection * 1.0f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(checkPos, attackRadius);

            // Bullet slow radius
            Gizmos.color = new Color(0f, 1f, 0.8f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, bulletSlowRadius + (100f - humanity) * 0.02f);
        }
    }
}

