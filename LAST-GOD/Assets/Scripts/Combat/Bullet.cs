using UnityEngine;
using LastGod.Core;

namespace LastGod.Combat
{
    /// <summary>
    /// Bullet projectile fired by Guards. Slows down when near Aeron as per script.
    /// Deals damage to IDamageable targets.
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float baseSpeed = 8f;
        [SerializeField] private float slowSpeed = 2f;
        [SerializeField] private float slowProximityRadius = 2.5f;

        [Header("Damage & VFX")]
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private GameObject impactVfxPrefab;
        [SerializeField] private AudioClip impactSFX;

        private Vector2 _direction = Vector2.right;
        private Transform _playerTransform;
        private bool _isInitialized;

        private void Awake()
        {
#if UNITY_EDITOR
            if (impactVfxPrefab == null)
            {
                impactVfxPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/VFX/VFX_LaserImpact_Sparks.prefab");
            }
#endif
        }

        public void Initialize(Vector2 direction, Transform playerTarget)
        {
            _direction = direction.normalized;
            _playerTransform = playerTarget;
            _isInitialized = true;

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (!_isInitialized) return;

            float currentSpeed = baseSpeed;

            // Bullet slow effect near Aeron
            if (_playerTransform != null)
            {
                float dist = Vector2.Distance(transform.position, _playerTransform.position);
                if (dist <= slowProximityRadius)
                {
                    currentSpeed = Mathf.Lerp(slowSpeed, baseSpeed, dist / slowProximityRadius);
                }
            }

            transform.Translate(_direction * (currentSpeed * Time.deltaTime), Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore other bullets, guards who fired, and enemies
            if (other.CompareTag("Enemy") || other.name.IndexOf("Guard", System.StringComparison.OrdinalIgnoreCase) >= 0 || other.GetComponent<Bullet>() != null || other.isTrigger) return;

            if (impactVfxPrefab != null)
            {
                Instantiate(impactVfxPrefab, transform.position, Quaternion.identity);
            }

            if (impactSFX != null)
            {
                AudioSource.PlayClipAtPoint(impactSFX, transform.position, 0.8f);
            }

            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage, _direction * 2f);
            }

            Destroy(gameObject);
        }
    }
}
