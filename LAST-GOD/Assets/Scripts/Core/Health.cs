using UnityEngine;
using UnityEngine.Events;

namespace LastGod.Core
{
    /// <summary>
    /// Reusable HP component. Drop this onto any actor that needs a health pool.
    /// Implements <see cref="IDamageable"/> so attack scripts stay decoupled.
    ///
    /// Wire-up in Inspector or from code:
    ///   OnDamaged  -- broadcast to HUD, flash shader, etc.
    ///   OnDeath    -- trigger death animation, spawn pickups, etc.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        // ─── Inspector ────────────────────────────────────────────────────────
        [Header("Stats")]
        [SerializeField, Min(1)] private int maxHP = 6;

        [Header("Knockback")]
        [Tooltip("Force applied to Rigidbody2D on hit. Tune per-actor in Inspector.")]
        [SerializeField] private float knockbackForce = 8f;

        [Header("Events")]
        /// <summary>Fired every time damage is taken. Arg = remaining HP.</summary>
        public UnityEvent<int> OnDamaged = new UnityEvent<int>();

        /// <summary>Fired once when HP reaches zero.</summary>
        public UnityEvent OnDeath = new UnityEvent();

        // ─── State ────────────────────────────────────────────────────────────
        private int _currentHP;
        private Rigidbody2D _rb;

        /// <inheritdoc/>
        public bool IsDead => _currentHP <= 0;

        /// <summary>Current HP (read-only from outside).</summary>
        public int CurrentHP => _currentHP;

        /// <summary>Max HP configured in Inspector.</summary>
        public int MaxHP => maxHP;

        // ─── Unity lifecycle ──────────────────────────────────────────────────
        private void Awake()
        {
            _currentHP = maxHP;
            // Optional: Rigidbody2D is used for knockback — non-mandatory.
            _rb = GetComponent<Rigidbody2D>();
        }

        // ─── IDamageable ──────────────────────────────────────────────────────
        /// <inheritdoc/>
        public void TakeDamage(int amount, Vector2 knockbackDir)
        {
            if (IsDead) return;  // Don't damage a dead entity.

            _currentHP = Mathf.Max(0, _currentHP - amount);

            // Apply knockback impulse if a Rigidbody2D is present.
            if (_rb != null && knockbackDir != Vector2.zero)
                _rb.AddForce(knockbackDir.normalized * knockbackForce, ForceMode2D.Impulse);

            OnDamaged.Invoke(_currentHP);

            if (IsDead)
                HandleDeath();
        }

        // ─── Internal helpers ─────────────────────────────────────────────────
        private void HandleDeath()
        {
            OnDeath.Invoke();
            // Concrete death behaviour (animation, ragdoll, destroy) is handled
            // by listeners — keeps Health.cs generic.
        }

        /// <summary>
        /// Restore HP to <paramref name="amount"/> clamped to maxHP.
        /// Useful for pickups / checkpoints later.
        /// </summary>
        public void Heal(int amount)
        {
            if (IsDead) return;
            _currentHP = Mathf.Min(maxHP, _currentHP + amount);
        }

        /// <summary>Instantly kill this entity (e.g. kill-plane / void damage).</summary>
        public void Kill() => TakeDamage(_currentHP, Vector2.zero);
    }
}
