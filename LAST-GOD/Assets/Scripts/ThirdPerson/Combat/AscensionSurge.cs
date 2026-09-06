using System;
using UnityEngine;
using UnityEngine.Events;

namespace LastGod.ThirdPerson.Combat
{
    public class AscensionSurge : MonoBehaviour
    {
        [Header("Surge Parameters")]
        [SerializeField] private float duration = 4.0f;
        [SerializeField] private float cooldown = 9.0f;
        [SerializeField] private float speedMultiplier = 1.9f;
        [SerializeField] private float damageMultiplier = 2.0f;

        [Header("Visual & FX")]
        [SerializeField] private ParticleSystem surgeAuraVFX;
        [SerializeField] private Light eyeGlowLight;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip surgeActivateSFX;
        [SerializeField] private AudioClip surgeLoopSFX;

        private float _activeTimer;
        private float _cooldownTimer;
        private bool _isUnlocked = true;

        public bool IsActive => _activeTimer > 0f;
        public bool IsOnCooldown => _cooldownTimer > 0f;
        public float CooldownRemaining => _cooldownTimer;
        public float CooldownPercent => Mathf.Clamp01(_cooldownTimer / Mathf.Max(0.1f, cooldown));
        public float SpeedMultiplier => IsActive ? speedMultiplier : 1.0f;
        public float DamageMultiplier => IsActive ? damageMultiplier : 1.0f;

        public UnityEvent OnSurgeActivated = new();
        public UnityEvent OnSurgeDeactivated = new();

        public void UnlockSurge()
        {
            _isUnlocked = true;
        }

        private void Update()
        {
            if (_activeTimer > 0f)
            {
                _activeTimer -= Time.deltaTime;
                if (_activeTimer <= 0f)
                {
                    DeactivateSurge();
                }
            }
            else if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }

        public bool TryActivate()
        {
            if (!_isUnlocked || IsActive || IsOnCooldown) return false;

            _activeTimer = duration;
            _cooldownTimer = cooldown;

            if (surgeAuraVFX != null) surgeAuraVFX.Play();
            if (eyeGlowLight != null) eyeGlowLight.enabled = true;
            if (audioSource != null && surgeActivateSFX != null)
            {
                audioSource.PlayOneShot(surgeActivateSFX);
            }

            OnSurgeActivated?.Invoke();
            return true;
        }

        private void DeactivateSurge()
        {
            if (surgeAuraVFX != null) surgeAuraVFX.Stop();
            if (eyeGlowLight != null) eyeGlowLight.enabled = false;
            OnSurgeDeactivated?.Invoke();
        }
    }
}
