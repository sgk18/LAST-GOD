using UnityEngine;
using LastGod.Core;

namespace LastGod.Combat
{
    /// <summary>
    /// Cyan/Orange Fireball projectile launched by Aeron post-awakening.
    /// Traverses horizontally and detonates upon impacting enemies or geometry.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class FireballProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 14f;
        [SerializeField] private int damage = 4;
        [SerializeField] private float lifeTime = 3.5f;
        [SerializeField] private AudioClip impactSFX;

        private Vector2 _direction = Vector2.right;
        private Rigidbody2D _rb;
        private CircleCollider2D _col;
        private bool _hasHit = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CircleCollider2D>();

            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _col.isTrigger = true;
            _col.radius = 0.45f;

#if UNITY_EDITOR
            if (impactSFX == null)
            {
                impactSFX = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/AudioClips/dark_magic_hit.wav");
            }
#endif
        }

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        public void Initialize(Vector2 direction, int projectileDamage = 4)
        {
            _direction = direction.normalized;
            damage = projectileDamage;

            // Orient sprite towards direction
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (_direction.x < 0)
            {
                // Invert Y if needed to keep sprite upright when facing left
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), -Mathf.Abs(transform.localScale.y), 1f);
            }
            else
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), 1f);
            }
        }

        private void Update()
        {
            if (_hasHit) return;
            transform.position += (Vector3)(_direction * (speed * Time.deltaTime));
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_hasHit) return;

            // Ignore player and triggers
            if (collision.CompareTag("Player") || collision.isTrigger) return;

            _hasHit = true;

            if (collision.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage, _direction * 4f);
            }
            else if (collision.attachedRigidbody != null && collision.attachedRigidbody.TryGetComponent<IDamageable>(out var rbDamageable))
            {
                rbDamageable.TakeDamage(damage, _direction * 4f);
            }

            if (impactSFX != null)
            {
                AudioSource.PlayClipAtPoint(impactSFX, transform.position, 1.0f);
            }

            Destroy(gameObject);
        }
    }
}
