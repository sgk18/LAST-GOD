using UnityEngine;

namespace LastGod.Core
{
    /// <summary>
    /// Implemented by any GameObject that can receive damage and die.
    /// Combat hitbox scripts call TakeDamage; HP-bar UI reads IsDead.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Apply <paramref name="amount"/> points of damage.
        /// <paramref name="knockbackDir"/> is a normalised world-space direction
        /// the receiver should be pushed toward.
        /// </summary>
        void TakeDamage(int amount, Vector2 knockbackDir);

        /// <summary>Returns true once HP has reached zero.</summary>
        bool IsDead { get; }
    }
}
