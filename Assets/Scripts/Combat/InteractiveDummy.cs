using System.Collections;
using UnityEngine;
using LastGod.Core;

namespace LastGod.Combat
{
    /// <summary>
    /// Training dummy that absorbs hits, shakes playfully, and resets.
    /// Provides immediate tactical feedback for player melee attacks.
    /// </summary>
    public class InteractiveDummy : MonoBehaviour, IDamageable
    {
        [Header("Feedback")]
        [SerializeField] private float wobbleAmount = 15f;
        [SerializeField] private float wobbleDuration = 0.25f;

        private Vector3 _originalRotation;
        private Coroutine _wobbleCoroutine;

        public bool IsDead => false; // Dummy never permanently dies

        private void Awake()
        {
            _originalRotation = transform.localEulerAngles;
        }

        public void TakeDamage(int amount, Vector2 knockbackDir)
        {
            if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);
            _wobbleCoroutine = StartCoroutine(WobbleRoutine(knockbackDir.x > 0 ? 1 : -1));

            if (CameraShake2D.Instance != null)
            {
                CameraShake2D.Instance.TriggerShake(0.15f, 0.08f);
            }
        }

        private IEnumerator WobbleRoutine(float direction)
        {
            float elapsed = 0f;
            while (elapsed < wobbleDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / wobbleDuration;
                float angle = Mathf.Sin(progress * Mathf.PI * 4f) * (1f - progress) * wobbleAmount * -direction;
                transform.localEulerAngles = new Vector3(_originalRotation.x, _originalRotation.y, _originalRotation.z + angle);
                yield return null;
            }
            transform.localEulerAngles = _originalRotation;
            _wobbleCoroutine = null;
        }
    }
}
