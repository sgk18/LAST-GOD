using System.Collections;
using UnityEngine;

namespace LastGod.ThirdPerson.Environment
{
    public class StasisChamber : MonoBehaviour
    {
        [Header("Chamber Parts")]
        [SerializeField] private GameObject glassCylinder;
        [SerializeField] private GameObject liquidSurface;
        [SerializeField] private Light chamberInternalLight;

        [Header("VFX & SFX")]
        [SerializeField] private ParticleSystem bubblesVFX;
        [SerializeField] private ParticleSystem shatterGlassVFX;
        [SerializeField] private ParticleSystem liquidBurstVFX;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip glassCrackSFX;
        [SerializeField] private AudioClip glassShatterSFX;
        [SerializeField] private AudioClip liquidDrainSFX;

        private bool _isCracked;
        private bool _isShattered;

        public bool IsShattered => _isShattered;

        public void StartDrainLiquid(float duration = 3.0f)
        {
            if (audioSource != null && liquidDrainSFX != null)
            {
                audioSource.PlayOneShot(liquidDrainSFX);
            }
            StartCoroutine(DrainRoutine(duration));
        }

        public void CrackGlass()
        {
            if (_isCracked) return;
            _isCracked = true;

            if (audioSource != null && glassCrackSFX != null)
            {
                audioSource.PlayOneShot(glassCrackSFX);
            }

            // Pulse chamber light
            if (chamberInternalLight != null)
            {
                chamberInternalLight.color = new Color(1.0f, 0.2f, 0.2f);
                chamberInternalLight.intensity = 4.0f;
            }
        }

        public void ShatterChamber()
        {
            if (_isShattered) return;
            _isShattered = true;

            if (glassCylinder != null) glassCylinder.SetActive(false);
            if (liquidSurface != null) liquidSurface.SetActive(false);
            if (bubblesVFX != null) bubblesVFX.Stop();

            if (shatterGlassVFX != null) shatterGlassVFX.Play();
            if (liquidBurstVFX != null) liquidBurstVFX.Play();

            if (audioSource != null && glassShatterSFX != null)
            {
                audioSource.PlayOneShot(glassShatterSFX);
            }

            if (chamberInternalLight != null)
            {
                chamberInternalLight.enabled = false;
            }
        }

        private IEnumerator DrainRoutine(float duration)
        {
            if (liquidSurface == null) yield break;

            Vector3 startPos = liquidSurface.transform.localPosition;
            Vector3 targetPos = startPos - Vector3.up * 2.2f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                liquidSurface.transform.localPosition = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                yield return null;
            }
        }
    }
}
