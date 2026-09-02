using System.Collections;
using UnityEngine;

namespace LastGod.Player
{
    /// <summary>
    /// Lightweight 2D Pixel-Perfect Screen Shake driver.
    /// Used during chamber shatter and alarm beats.
    /// </summary>
    public class CameraShake2D : MonoBehaviour
    {
        public static CameraShake2D Instance { get; private set; }

        private Vector3 _originalPos;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            _originalPos = transform.localPosition;
        }

        public void TriggerShake(float duration = 0.4f, float magnitude = 0.15f)
        {
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = _originalPos + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = _originalPos;
            _shakeCoroutine = null;
        }
    }
}
