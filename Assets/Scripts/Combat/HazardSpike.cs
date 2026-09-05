using UnityEngine;
using LastGod.Core;

namespace LastGod.Combat
{
    /// <summary>
    /// Environmental hazard component placed on spike traps.
    /// Inflicts damage and upward knockback to any IDamageable on contact.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HazardSpike : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private int damageAmount = 1;
        [SerializeField] private Vector2 knockbackDirection = new Vector2(0f, 1f);
        [SerializeField] private float damageCooldown = 0.6f;

        private float _lastDamageTime = -999f;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            ProcessHit(other.gameObject);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            ProcessHit(collision.gameObject);
        }

        private void ProcessHit(GameObject target)
        {
            if (Time.time - _lastDamageTime < damageCooldown) return;

            if (target.TryGetComponent<IDamageable>(out var damageable))
            {
                if (!damageable.IsDead)
                {
                    _lastDamageTime = Time.time;
                    Vector2 dir = knockbackDirection.normalized;
                    damageable.TakeDamage(damageAmount, dir);

                    if (CameraShake2D.Instance != null)
                    {
                        CameraShake2D.Instance.TriggerShake(0.2f, 0.15f);
                    }
                }
            }
        }
    }
}
