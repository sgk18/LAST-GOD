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

        [Header("Movement & Agility")]
        public float moveSpeed = 5.0f;
        public float dashSpeed = 16.0f;
        public float dashDuration = 0.22f;
        public float dashCooldown = 0.45f;
        public float jumpForce = 13.5f;
        public float fallGravityMultiplier = 1.35f;
        public float pogoBounceForce = 12.0f;
        public LayerMask groundLayer;
        public Transform groundCheck;
        public float groundCheckRadius = 0.35f;

        [Header("Combat Stats")]
        public float maxLife = 100f;
        public float currentLife = 100f;
        public float maxFocus = 100f;
        public float currentFocus = 100f;
        public float humanity = 100f; // 0 - 100%
        public float baseAttackDamage = 18f;

        [Header("Divine Right-Arm Laser Beam")]
        public GameObject armLaserBeamPrefab;
        public float laserFocusCost = 25f;
        public float laserChargeDuration = 0.2f;
        public float laserFireDuration = 0.45f;
        public bool isFiringLaser = false;

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
        public enum AeronState { Suspended, Awakening, Collapsed, Idle, Walking, Jumping, Falling, Attacking, Dashing, Absorbing, Hurt, LaserBeam, Dead }
        public AeronState CurrentState { get; private set; } = AeronState.Suspended;

        public bool IsInvulnerable => isInvulnerable;
        public bool IsDilatingTime => isDilatingTime;
        public float FacingDirection { get; private set; } = 1f;

        public bool IsGrounded => isGrounded;
        private bool isGrounded = false;
        private bool canAirDash = true;
        private float coyoteTimer = 0f;
        private float jumpBufferTimer = 0f;

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
            if (rb != null) rb.gravityScale = 3.0f;
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

            CheckGroundStatus();
            HandleInput();
            HandleTimeDilation();
            UpdateVisuals();
        }

        private void CheckGroundStatus()
        {
            Vector2 gPos = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down * 0.1f;
            int gMask = groundLayer.value != 0 ? groundLayer.value : LayerMask.GetMask("Default", "Environment", "Ground");
            isGrounded = Physics2D.OverlapCircle(gPos, groundCheckRadius, gMask);

            if (isGrounded)
            {
                canAirDash = true;
                coyoteTimer = 0.12f;
            }
            else
            {
                coyoteTimer -= Time.deltaTime;
            }

            // Snappy falling gravity multiplier (Hollow Knight feel)
            if (rb != null)
            {
                if (rb.linearVelocity.y < -0.1f)
                {
                    rb.gravityScale = 3.0f * fallGravityMultiplier;
                }
                else
                {
                    rb.gravityScale = 3.0f;
                }
            }
        }

        private void HandleInput()
        {
            if (CurrentState == AeronState.Dashing || CurrentState == AeronState.Absorbing || CurrentState == AeronState.Hurt || CurrentState == AeronState.LaserBeam)
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
                    if (isGrounded) CurrentState = AeronState.Walking;
                }
                else
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    if (isGrounded) CurrentState = AeronState.Idle;
                }

                if (!isGrounded)
                {
                    CurrentState = (rb.linearVelocity.y > 0.1f) ? AeronState.Jumping : AeronState.Falling;
                }
            }

            // Jump Inputs (W, Space, Up Arrow)
            jumpBufferTimer -= Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                jumpBufferTimer = 0.14f;
            }

            if (jumpBufferTimer > 0f && coyoteTimer > 0f && CurrentState != AeronState.Attacking)
            {
                ExecuteJump();
            }

            // Variable jump height cut
            if ((Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow)) && rb.linearVelocity.y > 0.5f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
            }

            // Divine Right-Arm Laser Beam (U / I / Right Click)
            if ((Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.I) || Input.GetMouseButtonDown(1)) && currentFocus >= laserFocusCost && !isFiringLaser)
            {
                StartCoroutine(FireLaserBeamRoutine());
            }

            // Attack (J / Left Click / Z)
            if ((Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Z) || Input.GetMouseButtonDown(0)) && CurrentState != AeronState.Attacking)
            {
                PerformAttack();
            }

            // Dash (K / Right Shift / X / Left Shift)
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

        private void ExecuteJump()
        {
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            CurrentState = AeronState.Jumping;
            if (animator != null) animator.SetTrigger("JumpTrigger");
        }

        private void PerformAttack()
        {
            float v = Input.GetAxisRaw("Vertical");
            // If airborne and pressing downward: Perform Downward Pogo Slash!
            if (!isGrounded && (v < -0.2f || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)))
            {
                PerformPogoSlash();
                return;
            }

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

        private void PerformPogoSlash()
        {
            CurrentState = AeronState.Attacking;

            // Angled downward slash
            Vector2 pogoCheckPos = (Vector2)transform.position + Vector2.down * 0.6f;
            if (slashEffectPrefab != null)
            {
                GameObject slash = Instantiate(slashEffectPrefab, pogoCheckPos, Quaternion.Euler(0f, 0f, -90f));
                Destroy(slash, 0.25f);
            }

            float damageMultiplier = 1.0f + ((100f - humanity) / 100f) * 0.65f;
            float totalDamage = baseAttackDamage * 1.35f * damageMultiplier;

            bool hitTarget = false;

            // Hit detection: Guards below
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(pogoCheckPos, attackRadius, enemyLayer);
            foreach (var hit in hitEnemies)
            {
                var guard = hit.GetComponent<GuardAI>();
                if (guard != null)
                {
                    guard.TakeDamage(totalDamage, FacingDirection);
                    currentFocus = Mathf.Min(maxFocus, currentFocus + 14f);
                    hitTarget = true;
                }
            }

            // Hit detection: Bullets below
            Collider2D[] hitBullets = Physics2D.OverlapCircleAll(pogoCheckPos, attackRadius * 1.3f, bulletLayer);
            foreach (var bCol in hitBullets)
            {
                var bullet = bCol.GetComponent<Bullet>();
                if (bullet != null && !bullet.IsReflected)
                {
                    bullet.Reflect(FacingDirection);
                    currentFocus = Mathf.Min(maxFocus, currentFocus + 20f);
                    GameManager.Instance?.RecordReflect();
                    hitTarget = true;
                }
            }

            // If struck enemy or bullet: POGO BOUNCE!
            if (hitTarget)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoBounceForce);
                canAirDash = true;
                CameraFollow2D.Instance?.Shake(0.12f, 0.22f);
            }

            StartCoroutine(AttackCooldownRoutine());
        }

        private IEnumerator FireLaserBeamRoutine()
        {
            CurrentState = AeronState.LaserBeam;
            isFiringLaser = true;
            currentFocus -= laserFocusCost;

            // Halt momentum during firing
            rb.linearVelocity = new Vector2(0f, isGrounded ? 0f : rb.linearVelocity.y * 0.1f);

            // Charging windup
            yield return new WaitForSeconds(laserChargeDuration);

            // Spawn laser beam
            Vector2 spawnPos = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position + Vector2.right * (FacingDirection * 0.8f);
            LastGod.Combat.ArmLaserBeam.Spawn(armLaserBeamPrefab, this, spawnPos, FacingDirection, laserFireDuration);

            // Recoil
            rb.linearVelocity = new Vector2(-FacingDirection * 3.5f, rb.linearVelocity.y);

            yield return new WaitForSeconds(laserFireDuration);
            isFiringLaser = false;
            CurrentState = isGrounded ? AeronState.Idle : AeronState.Falling;
        }

        private IEnumerator AttackCooldownRoutine()
        {
            yield return new WaitForSeconds(0.28f);
            if (CurrentState == AeronState.Attacking)
                CurrentState = isGrounded ? AeronState.Idle : AeronState.Falling;
        }

        private IEnumerator DashRoutine()
        {
            if (!isGrounded)
            {
                if (!canAirDash) yield break;
                canAirDash = false;
            }

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
            CurrentState = isGrounded ? AeronState.Idle : AeronState.Falling;
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

            // Drive animator movement & aerial parameters
            if (animator != null && rb != null)
            {
                float speed = Mathf.Abs(rb.linearVelocity.x);
                animator.SetFloat("Speed", speed);
                animator.SetBool("IsGrounded", isGrounded);
                animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
                animator.SetBool("IsFiringBeam", isFiringLaser);
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

