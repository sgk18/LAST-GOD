using System.Collections;
using UnityEngine;

namespace LastGod.Environment
{
    /// <summary>
    /// Flickers ambient lighting or sprite colors for 2D props (torches, street lanterns, laboratory monitors, fireflies).
    /// Supports SpriteRenderer color/alpha modulation and Universal Render Pipeline Light2D components.
    /// </summary>
    public class LightFlicker2D : MonoBehaviour
    {
        [Header("Intensity Range")]
        [Tooltip("Minimum color intensity / brightness multiplier.")]
        [SerializeField] private float minIntensity = 0.6f;

        [Tooltip("Maximum color intensity / brightness multiplier.")]
        [SerializeField] private float maxIntensity = 1.1f;

        [Header("Timing")]
        [Tooltip("Frequency of flicker updates in seconds.")]
        [SerializeField] private float flickerSpeed = 0.08f;

        [Tooltip("If true, uses Perlin noise for smooth torch/lantern flickering instead of sharp step flickering.")]
        [SerializeField] private bool smoothFlicker = true;

        [Header("Target Components")]
        [SerializeField] private SpriteRenderer targetSprite;

        private Color _baseColor;
        private float _noiseSeed;

        private void Start()
        {
            if (targetSprite == null)
            {
                targetSprite = GetComponent<SpriteRenderer>();
            }

            if (targetSprite != null)
            {
                _baseColor = targetSprite.color;
            }

            _noiseSeed = Random.Range(0f, 1000f);

            if (!smoothFlicker)
            {
                StartCoroutine(FlickerRoutine());
            }
        }

        private void Update()
        {
            if (smoothFlicker && targetSprite != null)
            {
                float noise = Mathf.PerlinNoise(_noiseSeed, Time.time * (1f / flickerSpeed));
                float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

                Color targetColor = _baseColor * currentIntensity;
                targetColor.a = _baseColor.a; // preserve original alpha
                targetSprite.color = targetColor;
            }
        }

        private IEnumerator FlickerRoutine()
        {
            while (true)
            {
                if (targetSprite != null)
                {
                    float currentIntensity = Random.Range(minIntensity, maxIntensity);
                    Color targetColor = _baseColor * currentIntensity;
                    targetColor.a = _baseColor.a;
                    targetSprite.color = targetColor;
                }

                yield return new WaitForSeconds(flickerSpeed + Random.Range(-0.02f, 0.02f));
            }
        }
    }
}
