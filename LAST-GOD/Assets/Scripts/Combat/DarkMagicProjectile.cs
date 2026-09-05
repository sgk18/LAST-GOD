using UnityEngine;
using LastGod.Core;

namespace LastGod.Combat
{
    /// <summary>
    /// Necrotic projectile fired by Bringer Of Death enemies.
    /// Deals damage on impact with the Player, or dissipates on Ground contact.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class DarkMagicProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 7.5f;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private int damage = 2;
        [SerializeField] private float homingStrength = 1.2f;

        [Header("Audio & VFX")]
        [SerializeField] private AudioClip hitSFX;
        [SerializeField] private GameObject hitEffectPrefab;

        private Vector2 _direction = Vector2.left;
        private Transform _target;
        private Rigidbody2D _rb;
        private CircleCollider2D _col;
        private bool _hasHit;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CircleCollider2D>();

            _rb.gravityScale = 0f;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _col.isTrigger = true;
        }

        public void Initialize(Vector2 direction, Transform target, int projectileDamage = 2, AudioClip impactSFX = null)
        {
            _direction = direction.normalized;
            _target = target;
            damage = projectileDamage;
            if (impactSFX != null) hitSFX = impactSFX;

            Destroy(gameObject, lifetime);
        }

        private void FixedUpdate()
        {
            if (_hasHit) return;

            // Gentle homing toward player if target exists
            if (_target != null)
            {
                Vector2 toTarget = ((Vector2)_target.position - (Vector2)transform.position).normalized;
                _direction = Vector2.Lerp(_direction, toTarget, homingStrength * Time.fixedDeltaTime).normalized;
            }

            _rb.linearVelocity = _direction * speed;

            // Rotate in flight direction
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_hasHit) return;

            // Ignore enemy colliders
            if (other.CompareTag("Enemy")) return;

            if (other.CompareTag("Player") || other.GetComponent<Health>() != null)
            {
                if (other.TryGetComponent<IDamageable>(out var dmg))
                {
                    dmg.TakeDamage(damage, _direction * 3f + Vector2.up * 1.5f);
                }

                Explode();
            }
            else if (other.gameObject.layer == 8 || other.CompareTag("Ground")) // Ground
            {
                Explode();
            }
        }

        private void Explode()
        {
            _hasHit = true;
            _rb.linearVelocity = Vector2.zero;

            if (hitSFX != null)
            {
                AudioSource.PlayClipAtPoint(hitSFX, transform.position, 1f);
            }

            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
