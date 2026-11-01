using UnityEngine;

namespace LastGod.Act1
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        [Header("Properties")]
        public float baseSpeed = 9.0f;
        public float damage = 12.0f;
        public float lifetime = 8.0f;
        public LayerMask hitLayers;

        [Header("Visuals")]
        public Sprite normalSprite;
        public Sprite slowedSprite;
        public GameObject sparkVfxPrefab;

        public bool IsReflected { get; private set; } = false;
        public float Direction { get; private set; } = -1f;

        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private float lifeTimer = 0f;
        private bool hasDodgedThrough = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
        }

        public void Initialize(float dir, float spd, float dmg)
        {
            Direction = Mathf.Sign(dir);
            baseSpeed = spd;
            damage = dmg;
            if (sr != null)
            {
                sr.flipX = (Direction < 0);
                sr.color = new Color(0.25f, 0.85f, 1f, 1f); // Electric Blue Plasma Bolt
            }
        }

        public void DisintegrateByLaser()
        {
            SpawnSparks();
            Destroy(gameObject);
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Check proximity to Aeron's time-dilation field
            float currentSpeed = baseSpeed;
            if (AeronController.Instance != null && !IsReflected)
            {
                float dist = Vector2.Distance(transform.position, AeronController.Instance.transform.position);
                float radius = AeronController.Instance.bulletSlowRadius + (100f - AeronController.Instance.humanity) * 0.02f;

                if (dist < radius)
                {
                    // Slow down progressively as it gets closer
                    float t = Mathf.Clamp01(dist / radius);
                    float slowFactor = Mathf.Lerp(AeronController.Instance.bulletSlowFactor, 0.85f, t);
                    currentSpeed *= slowFactor;

                    if (sr != null && slowedSprite != null) sr.sprite = slowedSprite;

                    // Check for Echo dodge when Aeron dashes through bullet
                    if (AeronController.Instance.CurrentState == AeronController.AeronState.Dashing && !hasDodgedThrough)
                    {
                        if (dist < 1.2f)
                        {
                            hasDodgedThrough = true;
                            AeronController.Instance.TriggerEcho();
                        }
                    }
                }
                else
                {
                    if (sr != null && normalSprite != null) sr.sprite = normalSprite;
                }
            }

            rb.linearVelocity = new Vector2(Direction * currentSpeed, 0f);
        }

        public void Reflect(float newDir)
        {
            IsReflected = true;
            Direction = Mathf.Sign(newDir);
            baseSpeed *= 1.45f;
            damage *= 2.2f;

            if (sr != null)
            {
                sr.flipX = (Direction < 0);
                sr.color = Color.cyan;
            }

            if (sparkVfxPrefab != null)
            {
                GameObject spark = Instantiate(sparkVfxPrefab, transform.position, Quaternion.identity);
                Destroy(spark, 0.5f);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // If hitting Aeron
            if (!IsReflected && collision.CompareTag("Player"))
            {
                var aeron = collision.GetComponent<AeronController>();
                if (aeron != null && !aeron.IsInvulnerable)
                {
                    aeron.TakeDamage(damage, Direction);
                    SpawnSparks();
                    Destroy(gameObject);
                }
            }
            // If reflected and hitting a Guard
            else if (IsReflected && collision.CompareTag("Enemy"))
            {
                var guard = collision.GetComponent<GuardAI>();
                if (guard != null)
                {
                    guard.TakeDamage(damage, Direction);
                    SpawnSparks();
                    Destroy(gameObject);
                }
            }
            // Hitting scenery/walls
            else if (collision.CompareTag("Environment"))
            {
                SpawnSparks();
                Destroy(gameObject);
            }
        }

        private void SpawnSparks()
        {
            if (sparkVfxPrefab != null)
            {
                GameObject spark = Instantiate(sparkVfxPrefab, transform.position, Quaternion.identity);
                Destroy(spark, 0.4f);
            }
        }
    }
}
