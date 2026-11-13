using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LastGod.Act1
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class GuardAI : MonoBehaviour
    {
        public static List<GuardAI> ActiveGuards = new List<GuardAI>();

        public enum GuardType { Standard, Elite }
        public GuardType guardType = GuardType.Standard;

        public enum AIState { Spawning, Patrol, Aiming, Shooting, Hurt, Broken, Dead }
        public AIState CurrentState { get; private set; } = AIState.Patrol;

        [Header("Stats")]
        public float maxHealth = 30f;
        public float currentHealth = 30f;
        public float moveSpeed = 2.2f;
        public float aimDuration = 0.85f;
        public float attackCooldown = 1.8f;
        public float preferredDistance = 6.0f;

        [Header("Weapons")]
        public Transform gunMuzzle;
        public GameObject bulletPrefab;
        public GameObject muzzleFlashPrefab;
        public LineRenderer laserAimLine;

        [Header("Visuals")]
        public SpriteRenderer spriteRenderer;
        public GameObject brokenMarkerPrefab;
        public GameObject absorbParticlesPrefab;

        [Header("Tactical Blue Sprites")]
        public Sprite tacticalIdleSprite;
        public Sprite tacticalAimSprite;
        public Sprite tacticalShootSprite;
        public Sprite tacticalRecoilSprite;
        public Sprite tacticalHurtSprite;
        public Sprite tacticalBrokenSprite;

        private Rigidbody2D rb;
        private Collider2D col;
        private float facingDir = -1f;
        private float stateTimer = 0f;
        private float cooldownTimer = 0f;
        private int burstShotsRemaining = 0;
        private GameObject spawnedBrokenMarker;

        private void OnEnable()
        {
            ActiveGuards.Add(this);
        }

        private void OnDisable()
        {
            ActiveGuards.Remove(this);
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (guardType == GuardType.Elite)
            {
                maxHealth = 55f;
                aimDuration = 0.55f;
                attackCooldown = 1.3f;
                moveSpeed = 2.8f;
            }
        }

        private void Start()
        {
            currentHealth = maxHealth;
            if (laserAimLine != null) laserAimLine.enabled = false;
        }

        private void Update()
        {
            if (CurrentState == AIState.Dead) return;

            stateTimer += Time.deltaTime;
            if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;

            if (AeronController.Instance == null) return;
            Transform target = AeronController.Instance.transform;
            float dist = Mathf.Abs(target.position.x - transform.position.x);

            // Turn towards Aeron
            if (CurrentState != AIState.Broken && CurrentState != AIState.Hurt)
            {
                facingDir = Mathf.Sign(target.position.x - transform.position.x);
                if (spriteRenderer != null) spriteRenderer.flipX = (facingDir > 0);
            }

            switch (CurrentState)
            {
                case AIState.Spawning:
                    if (stateTimer >= 0.5f) CurrentState = AIState.Patrol;
                    break;

                case AIState.Patrol:
                    // Maintain tactical engagement distance
                    if (dist > preferredDistance + 1.0f)
                    {
                        rb.linearVelocity = new Vector2(facingDir * moveSpeed, rb.linearVelocity.y);
                    }
                    else if (dist < preferredDistance - 1.5f)
                    {
                        rb.linearVelocity = new Vector2(-facingDir * (moveSpeed * 0.8f), rb.linearVelocity.y);
                    }
                    else
                    {
                        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    }

                    // Check firing readiness
                    if (cooldownTimer <= 0f && dist <= preferredDistance + 2.5f && AeronController.Instance.CurrentState != AeronController.AeronState.Suspended)
                    {
                        StartAiming();
                    }
                    break;

                case AIState.Aiming:
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                    UpdateLaserAim(target.position);

                    if (stateTimer >= aimDuration)
                    {
                        StartShooting();
                    }
                    break;

                case AIState.Shooting:
                    // Handled via Coroutine
                    break;

                case AIState.Broken:
                    rb.linearVelocity = Vector2.zero;
                    // Pulsing marker
                    if (stateTimer >= 4.0f)
                    {
                        // Collapse on timeout
                        Die(false);
                    }
                    break;
            }
        }

        private void StartAiming()
        {
            CurrentState = AIState.Aiming;
            stateTimer = 0f;
            if (laserAimLine != null)
            {
                laserAimLine.enabled = true;
                laserAimLine.startColor = new Color(0.15f, 0.85f, 1f, 0.9f);
                laserAimLine.endColor = new Color(0.1f, 0.65f, 1f, 0.2f);
            }
            if (spriteRenderer != null && tacticalAimSprite != null) spriteRenderer.sprite = tacticalAimSprite;
        }

        private void UpdateLaserAim(Vector3 targetPos)
        {
            if (laserAimLine == null || gunMuzzle == null) return;
            laserAimLine.SetPosition(0, gunMuzzle.position);
            Vector3 endPoint = gunMuzzle.position + Vector3.right * (facingDir * 12.0f);
            laserAimLine.SetPosition(1, endPoint);
        }

        private void StartShooting()
        {
            if (laserAimLine != null) laserAimLine.enabled = false;
            CurrentState = AIState.Shooting;
            stateTimer = 0f;
            burstShotsRemaining = (guardType == GuardType.Elite) ? 3 : 1;
            StartCoroutine(ShootBurstRoutine());
        }

        private IEnumerator ShootBurstRoutine()
        {
            if (spriteRenderer != null && tacticalShootSprite != null) spriteRenderer.sprite = tacticalShootSprite;
            while (burstShotsRemaining > 0)
            {
                FireSingleBullet();
                burstShotsRemaining--;
                if (burstShotsRemaining > 0)
                    yield return new WaitForSeconds(0.12f);
            }

            if (spriteRenderer != null && tacticalRecoilSprite != null) spriteRenderer.sprite = tacticalRecoilSprite;
            cooldownTimer = attackCooldown;
            yield return new WaitForSeconds(0.2f);
            if (CurrentState == AIState.Shooting)
            {
                if (spriteRenderer != null && tacticalIdleSprite != null) spriteRenderer.sprite = tacticalIdleSprite;
                CurrentState = AIState.Patrol;
            }
        }

        private void FireSingleBullet()
        {
            if (bulletPrefab == null) return;
            Vector3 spawnPos = gunMuzzle != null ? gunMuzzle.position : transform.position + Vector3.right * (facingDir * 0.7f);
            GameObject bObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            var bullet = bObj.GetComponent<Bullet>();
            if (bullet != null)
            {
                float spd = (guardType == GuardType.Elite) ? 10.5f : 8.5f;
                float dmg = (guardType == GuardType.Elite) ? 14f : 10f;
                bullet.Initialize(facingDir, spd, dmg);
            }

            if (muzzleFlashPrefab != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, spawnPos, Quaternion.identity);
                Destroy(flash, 0.1f);
            }
        }

        public void TakeDamage(float amount, float knockbackDir)
        {
            if (CurrentState == AIState.Dead) return;

            if (CurrentState == AIState.Broken)
            {
                // Player Cut Down the broken guard
                Die(false);
                GameManager.Instance?.RecordCutDown();
                return;
            }

            currentHealth -= amount;
            rb.linearVelocity = new Vector2(knockbackDir * 3.0f, 0.5f);

            if (currentHealth <= 0f)
            {
                EnterBrokenState();
            }
            else
            {
                StartCoroutine(HurtRoutine());
            }
        }

        public void ForceBrokenState()
        {
            EnterBrokenState();
        }

        private void EnterBrokenState()
        {
            CurrentState = AIState.Broken;
            stateTimer = 0f;
            if (laserAimLine != null) laserAimLine.enabled = false;
            rb.linearVelocity = Vector2.zero;

            if (spriteRenderer != null && tacticalBrokenSprite != null)
            {
                spriteRenderer.sprite = tacticalBrokenSprite;
            }

            // Spawn teal broken indicator
            if (brokenMarkerPrefab != null && spawnedBrokenMarker == null)
            {
                spawnedBrokenMarker = Instantiate(brokenMarkerPrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity, transform);
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.3f, 0.9f, 0.8f, 0.85f);
            }
        }

        private IEnumerator HurtRoutine()
        {
            AIState prev = CurrentState;
            CurrentState = AIState.Hurt;
            if (laserAimLine != null) laserAimLine.enabled = false;

            if (spriteRenderer != null) spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            if (spriteRenderer != null) spriteRenderer.color = Color.white;

            if (CurrentState == AIState.Hurt)
            {
                CurrentState = AIState.Patrol;
            }
        }

        public void AbsorbByAeron()
        {
            // Aeron chose to TAKE
            if (spawnedBrokenMarker != null) Destroy(spawnedBrokenMarker);

            if (absorbParticlesPrefab != null)
            {
                GameObject p = Instantiate(absorbParticlesPrefab, transform.position, Quaternion.identity);
                Destroy(p, 1.2f);
            }

            Die(true);
        }

        private void Die(bool wasAbsorbed)
        {
            CurrentState = AIState.Dead;
            if (laserAimLine != null) laserAimLine.enabled = false;
            if (spawnedBrokenMarker != null) Destroy(spawnedBrokenMarker);

            col.enabled = false;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.4f, 0.4f, 0.4f, 0.6f);
            }

            GameManager.Instance?.OnEnemyDefeated(this);
            Destroy(gameObject, 2.0f);
        }

        public static GuardAI GetNearestBrokenGuard(Vector3 pos, float maxRange)
        {
            GuardAI nearest = null;
            float minDst = maxRange;
            foreach (var g in ActiveGuards)
            {
                if (g.CurrentState == AIState.Broken)
                {
                    float dst = Vector2.Distance(pos, g.transform.position);
                    if (dst < minDst)
                    {
                        minDst = dst;
                        nearest = g;
                    }
                }
            }
            return nearest;
        }
    }
}

