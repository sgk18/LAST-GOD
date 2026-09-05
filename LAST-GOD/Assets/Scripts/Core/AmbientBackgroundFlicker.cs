using System.Collections;
using UnityEngine;

namespace LastGod.Core
{
    /// <summary>
    /// Swaps background sprite frames at random intervals using a coroutine to simulate flickering lab electronics.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class AmbientBackgroundFlicker : MonoBehaviour
    {
        [Header("Flicker Frames")]
        [Tooltip("Array of background frame sprites to cycle through randomly.")]
        [SerializeField] private Sprite[] frameSprites;

        [Header("Flicker Timing")]
        [Tooltip("Minimum interval in seconds between frame swaps.")]
        [SerializeField] private float minFlickerInterval = 0.15f;

        [Tooltip("Maximum interval in seconds between frame swaps.")]
        [SerializeField] private float maxFlickerInterval = 0.60f;

        private SpriteRenderer _spriteRenderer;
        private Coroutine _flickerCoroutine;
        private int _currentFrameIndex = 0;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
                    // Pick a different frame index to guarantee visual flicker change
                    int nextIndex = Random.Range(0, frameSprites.Length);
                    if (nextIndex == _currentFrameIndex)
                    {
                        nextIndex = (_currentFrameIndex + 1) % frameSprites.Length;
                    }
                    _currentFrameIndex = nextIndex;

                    if (frameSprites[_currentFrameIndex] != null)
                    {
                        _spriteRenderer.sprite = frameSprites[_currentFrameIndex];
                    }
                }
            }
        }
    }
}
