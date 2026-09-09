using System.Collections;
using UnityEngine;

namespace LastGod.Core
{
    /// <summary>
    /// Swaps and crossfades background sprite frames at random intervals using a coroutine 
    /// to simulate organic laboratory electronics flicker and ambient lighting shifts.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class AmbientBackgroundFlicker : MonoBehaviour
    {
        [Header("Flicker Frames")]
        [Tooltip("Array of background frame sprites to cycle through (e.g. 1.png and 2.png).")]
        [SerializeField] private Sprite[] frameSprites;

        [Header("Flicker Timing")]
        [Tooltip("Minimum interval in seconds between frame swaps.")]
        [SerializeField] private float minFlickerInterval = 0.15f;

        [Tooltip("Maximum interval in seconds between frame swaps.")]
        [SerializeField] private float maxFlickerInterval = 0.60f;

        [Header("Crossfade & Glitch Effects")]
        [Tooltip("Enable smooth alpha crossfade blending between frames.")]
        [SerializeField] private bool useCrossfade = true;

        [Tooltip("Duration in seconds for smooth crossfade transition.")]
        [SerializeField] private float crossfadeDuration = 0.12f;

        [Tooltip("Chance (0 to 1) for a rapid electronic micro-glitch pulse.")]
        [SerializeField] private float glitchChance = 0.35f;

        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _secondaryRenderer;
        private Coroutine _flickerCoroutine;
        private int _currentFrameIndex = 0;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            // Setup secondary SpriteRenderer child for smooth dual-layer crossfading
            Transform secondaryChild = transform.Find("SecondaryFlickerLayer");
            if (secondaryChild == null)
            {
                GameObject childObj = new GameObject("SecondaryFlickerLayer");
                childObj.transform.SetParent(transform, false);
                _secondaryRenderer = childObj.AddComponent<SpriteRenderer>();
            }
            else
            {
                _secondaryRenderer = secondaryChild.GetComponent<SpriteRenderer>();
            }

            if (_secondaryRenderer != null)
            {
                _secondaryRenderer.sortingLayerID = _spriteRenderer.sortingLayerID;
                _secondaryRenderer.sortingLayerName = _spriteRenderer.sortingLayerName;
                _secondaryRenderer.sortingOrder = _spriteRenderer.sortingOrder + 1;
                _secondaryRenderer.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        private void OnEnable()
        {
            if (_flickerCoroutine != null)
            {
                StopCoroutine(_flickerCoroutine);
            }
            _flickerCoroutine = StartCoroutine(FlickerRoutine());
        }

        private void OnDisable()
        {
            if (_flickerCoroutine != null)
            {
                StopCoroutine(_flickerCoroutine);
                _flickerCoroutine = null;
            }
        }

        private IEnumerator FlickerRoutine()
        {
            while (true)
            {
                float waitTime = Random.Range(minFlickerInterval, maxFlickerInterval);
                yield return new WaitForSeconds(waitTime);

                if (_spriteRenderer != null && frameSprites != null && frameSprites.Length > 1)
                {
                    int nextIndex = Random.Range(0, frameSprites.Length);
                    if (nextIndex == _currentFrameIndex)
                    {
                        nextIndex = (_currentFrameIndex + 1) % frameSprites.Length;
                    }

                    Sprite targetSprite = frameSprites[nextIndex];
                    if (targetSprite != null)
                    {
                        bool isGlitchPulse = Random.value < glitchChance;

                        if (useCrossfade && _secondaryRenderer != null && !isGlitchPulse)
                        {
                            // Smooth alpha crossfade transition
                            _secondaryRenderer.sprite = targetSprite;
                            _secondaryRenderer.color = new Color(1f, 1f, 1f, 0f);

                            float elapsed = 0f;
                            while (elapsed < crossfadeDuration)
                            {
                                elapsed += Time.deltaTime;
                                float t = Mathf.Clamp01(elapsed / crossfadeDuration);
                                _secondaryRenderer.color = new Color(1f, 1f, 1f, t);
                                yield return null;
                            }

                            _spriteRenderer.sprite = targetSprite;
                            _secondaryRenderer.color = new Color(1f, 1f, 1f, 0f);
                        }
                        else
                        {
                            // Instant electronic flicker / glitch swap
                            _spriteRenderer.sprite = targetSprite;

                            if (isGlitchPulse)
                            {
                                // Rapid double flicker burst
                                yield return new WaitForSeconds(0.04f);
                                _spriteRenderer.sprite = frameSprites[_currentFrameIndex];
                                yield return new WaitForSeconds(0.03f);
                                _spriteRenderer.sprite = targetSprite;
                            }
                        }

                        _currentFrameIndex = nextIndex;
                    }
                }
            }
        }
    }
}
