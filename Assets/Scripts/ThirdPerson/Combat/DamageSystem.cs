using System;
using UnityEngine;
using UnityEngine.Events;

namespace LastGod.ThirdPerson.Combat
{
    public class DamageSystem : MonoBehaviour
    {
    }

    public struct DamageInfo
    {
        public float Amount;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public Vector3 KnockbackDirection;
        public float KnockbackForce;
        public GameObject Attacker;
        public bool IsHeavy;
        public bool IsExecution;

        public DamageInfo(float amount, GameObject attacker = null, Vector3 hitPoint = default, Vector3 hitNormal = default, Vector3 knockbackDir = default, float knockbackForce = 0f, bool isHeavy = false, bool isExecution = false)
        {
            Amount = amount;
            Attacker = attacker;
            HitPoint = hitPoint;
            HitNormal = hitNormal;
            KnockbackDirection = knockbackDir;
            KnockbackForce = knockbackForce;
            IsHeavy = isHeavy;
            IsExecution = isExecution;
        }
    }

    public interface IDamageReceiver
    {
        void TakeDamage(DamageInfo damage);
        bool IsDead { get; }
        Transform transform { get; }
    }

    public class DamageReceiver : MonoBehaviour, IDamageReceiver
    {
        [SerializeField] private Health3D health;
        [SerializeField] private HitReaction hitReaction;

        public bool IsDead => health != null && health.IsDead;

        private void Awake()
        {
            if (health == null) health = GetComponent<Health3D>() ?? GetComponentInParent<Health3D>();
            if (hitReaction == null) hitReaction = GetComponent<HitReaction>() ?? GetComponentInParent<HitReaction>();
        }

        public void TakeDamage(DamageInfo damage)
        {
            if (health != null && !health.IsDead)
            {
                health.ApplyDamage(damage);
            }

            if (hitReaction != null)
            {
                hitReaction.ReactToHit(damage);
            }
        }
    }

    public class Health3D : MonoBehaviour
    {
        [Header("Vital Attributes")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float invulnerabilityDuration = 0.35f;

        private float _invulnTimer;
        private bool _isDead;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => Mathf.Clamp01(currentHealth / Mathf.Max(1f, maxHealth));
        public bool IsDead => _isDead;
        public bool IsInvulnerable => _invulnTimer > 0f;

        public UnityEvent<float, float> OnHealthChanged = new();
        public UnityEvent<DamageInfo> OnDamaged = new();
        public UnityEvent<GameObject> OnDeath = new();

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (_invulnTimer > 0f)
            {
                _invulnTimer -= Time.deltaTime;
            }
        }

        public void SetInvulnerable(float duration)
        {
            _invulnTimer = Mathf.Max(_invulnTimer, duration);
        }

        public void ApplyDamage(DamageInfo damage)
        {
            if (_isDead || _invulnTimer > 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - damage.Amount);
            _invulnTimer = invulnerabilityDuration;

            OnDamaged?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f && !_isDead)
            {
                _isDead = true;
                OnDeath?.Invoke(damage.Attacker);
            }
        }

        public void Heal(float amount)
        {
            if (_isDead) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ResetHealth()
        {
            _isDead = false;
            currentHealth = maxHealth;
            _invulnTimer = 0f;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    public class HitReaction : MonoBehaviour
    {
        [SerializeField] private ParticleSystem bloodVFX;
        [SerializeField] private ParticleSystem sparksVFX;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hitSound;

        public void ReactToHit(DamageInfo damage)
        {
            if (bloodVFX != null)
            {
                Vector3 pos = damage.HitPoint != Vector3.zero ? damage.HitPoint : transform.position + Vector3.up * 1f;
                bloodVFX.transform.position = pos;
                bloodVFX.Play();
            }

            if (sparksVFX != null && damage.IsHeavy)
            {
                sparksVFX.transform.position = damage.HitPoint != Vector3.zero ? damage.HitPoint : transform.position + Vector3.up * 1f;
                sparksVFX.Play();
            }

            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }
    }
}
