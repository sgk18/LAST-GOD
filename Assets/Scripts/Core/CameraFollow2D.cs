using UnityEngine;

namespace LastGod.Act1
{
    public class CameraFollow2D : MonoBehaviour
    {
        public static CameraFollow2D Instance { get; private set; }

        [Header("Target Tracking")]
        public Transform target;
        public float smoothTime = 0.2f;
        public Vector2 offset = new Vector2(0f, 0.8f);
        public Vector2 minBounds = new Vector2(-4f, -2f);
        public Vector2 maxBounds = new Vector2(24f, 6f);

        [Header("Parallax Layers")]
        public Transform farBackground;
        public float farParallaxFactor = 0.2f;
        public Transform midBackground;
        public float midParallaxFactor = 0.5f;

        private Vector3 velocity = Vector3.zero;
        private float shakeTimeRemaining = 0f;
        private float currentShakeMagnitude = 0f;
        private Vector3 startPos;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            startPos = transform.position;
            if (target == null && AeronController.Instance != null)
            {
                target = AeronController.Instance.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                if (AeronController.Instance != null) target = AeronController.Instance.transform;
                else return;
            }

            Vector3 targetPos = new Vector3(
                Mathf.Clamp(target.position.x + offset.x, minBounds.x, maxBounds.x),
                Mathf.Clamp(target.position.y + offset.y, minBounds.y, maxBounds.y),
                transform.position.z
            );

            Vector3 smoothPos = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

            // Screen shake
            if (shakeTimeRemaining > 0f)
            {
                shakeTimeRemaining -= Time.unscaledDeltaTime;
                Vector3 shakeOffset = Random.insideUnitSphere * currentShakeMagnitude;
                shakeOffset.z = 0f;
                smoothPos += shakeOffset;
            }

            transform.position = smoothPos;

            // Update Parallax
            float camDeltaX = transform.position.x - startPos.x;
            if (farBackground != null)
            {
                farBackground.position = new Vector3(startPos.x + camDeltaX * farParallaxFactor, farBackground.position.y, farBackground.position.z);
            }
            if (midBackground != null)
            {
                midBackground.position = new Vector3(startPos.x + camDeltaX * midParallaxFactor, midBackground.position.y, midBackground.position.z);
            }
        }

        public void Shake(float duration, float magnitude)
        {
            shakeTimeRemaining = duration;
            currentShakeMagnitude = magnitude;
        }
    }
}

