using UnityEngine;

namespace LastGod.Environment2D
{
    /// <summary>
    /// Controls multi-plane parallax depth scrolling for 2D platformer environments.
    /// Supports pixel-perfect snapping to prevent sub-pixel shimmering.
    /// </summary>
    [ExecuteAlways]
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [Tooltip("Parallax movement multiplier relative to camera movement (0 = static, 1 = moves with gameplay plane).")]
        [Range(-0.5f, 2.0f)]
        public float parallaxFactorX = 0.5f;

        [Tooltip("Vertical parallax movement multiplier (usually smaller than horizontal).")]
        [Range(0f, 1.0f)]
        public float parallaxFactorY = 0.1f;

        [Header("Pixel Snapping")]
        public bool pixelSnap = true;
        public float pixelsPerUnit = 32f;

        [Header("References")]
        [SerializeField] private Camera targetCamera;

        private Vector3 startPosition;
        private Vector3 startCameraPosition;

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            startPosition = transform.position;
            if (targetCamera != null)
            {
                startCameraPosition = targetCamera.transform.position;
            }
        }

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            startPosition = transform.position;
            if (targetCamera != null)
            {
                startCameraPosition = targetCamera.transform.position;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null) return;
                startCameraPosition = targetCamera.transform.position;
            }

            Vector3 cameraDelta = targetCamera.transform.position - startCameraPosition;

            float newX = startPosition.x + (cameraDelta.x * parallaxFactorX);
            float newY = startPosition.y + (cameraDelta.y * parallaxFactorY);

            if (pixelSnap && pixelsPerUnit > 0)
            {
                newX = Mathf.Round(newX * pixelsPerUnit) / pixelsPerUnit;
                newY = Mathf.Round(newY * pixelsPerUnit) / pixelsPerUnit;
            }

            transform.position = new Vector3(newX, newY, startPosition.z);
        }

        public void ResetOrigin()
        {
            startPosition = transform.position;
            if (targetCamera != null)
            {
                startCameraPosition = targetCamera.transform.position;
            }
        }
    }
}
