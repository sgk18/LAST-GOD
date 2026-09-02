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

        [Header("Damage")]
        [SerializeField] private int damage = 1;
        [SerializeField] private float lifetime = 4f;

        private Vector2 _direction = Vector2.right;
        private Transform _playerTransform;
        private bool _isInitialized;

        public void Initialize(Vector2 direction, Transform playerTarget)
        {
            _direction = direction.normalized;
            _playerTransform = playerTarget;
            _isInitialized = true;
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
            // Ignore other bullets or guards who fired
            if (other.CompareTag("Enemy") || other.GetComponent<Bullet>() != null) return;

            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage, _direction);
                Destroy(gameObject);
            }
            else if (!other.isTrigger)
            {
                // Hit wall / obstacle
                Destroy(gameObject);
            }
        }
    }
}
