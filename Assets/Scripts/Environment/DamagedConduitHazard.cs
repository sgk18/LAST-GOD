using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using LastGod.Core;

namespace LastGod.Environment
{
    /// <summary>
    /// Damaged electrical conduit in Act 1 Laboratory. Sparks hazardously until struck by bullet.
    /// Acts as the first organic weapon tutorial without combat enemies.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DamagedConduitHazard : MonoBehaviour, IDamageable
    {
        [Header("State")]
        [SerializeField] private bool isStabilized = false;

        [Header("Visual Elements")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem sparkParticles;
        [SerializeField] private Light2D hazardLight;
        [SerializeField] private Light2D secondaryAreaLight;

        [Header("Linked Environment Reaction")]
        [SerializeField] private GameObject linkedDoorToOpen;
        [SerializeField] private MonoBehaviour linkedTerminalToActivate;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip sparkingSfx;
        [SerializeField] private AudioClip dischargeSfx;

        public bool IsDead => isStabilized;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (sparkParticles == null) sparkParticles = GetComponentInChildren<ParticleSystem>();
            if (hazardLight == null) hazardLight = GetComponentInChildren<Light2D>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        public void TakeDamage(int amount, Vector2 knockbackDir)
        {
            if (isStabilized) return;
            StabilizeConduit();
        }

        public void StabilizeConduit()
        {
            isStabilized = true;
            StartCoroutine(ConduitReactionSequence());
        }

        private IEnumerator ConduitReactionSequence()
        {
            // 1. Electrical Discharge Reaction
            if (dischargeSfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(dischargeSfx);
            }

            // Burst sparks & flicker light
            if (sparkParticles != null)
            {
                sparkParticles.Emit(25);
            }

            if (hazardLight != null)
            {
                hazardLight.intensity = 3.5f;
                hazardLight.color = new Color(0.8f, 0.95f, 1.0f);
            }

            yield return new WaitForSeconds(0.15f);

            if (hazardLight != null)
            {
                hazardLight.intensity = 0.5f;
            }

            yield return new WaitForSeconds(0.1f);

            // 2. Machinery / Door response
            if (linkedDoorToOpen != null)
            {
                linkedDoorToOpen.SetActive(false); // Opens passage
            }

            if (secondaryAreaLight != null)
            {
                secondaryAreaLight.enabled = true;
                secondaryAreaLight.intensity = 1.0f;
            }

            // Halts violent sparking
            if (sparkParticles != null)
            {
                var emission = sparkParticles.emission;
                emission.rateOverTime = 0;
            }

            Debug.Log("[DamagedConduitHazard] Conduit stabilized by pistol shot! Auxiliary power restored.");
        }
    }
}
