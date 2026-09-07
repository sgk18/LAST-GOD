using UnityEngine;

namespace LastGod.Core
{
    /// <summary>
    /// Drives pixel-snapped camera-delta parallax scrolling for an individual background or foreground layer.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [Tooltip("Parallax movement factor relative to camera. 0 = static, 1 = moves with camera (midground), <1 = slower (far bg), >1 = faster (foreground).")]
        [SerializeField] private float parallaxFactor = 1.0f;

        [Tooltip("Target camera driving parallax motion. Defaults to Camera.main if unassigned.")]
        [SerializeField] private Camera targetCamera;

        [Header("Pixel Alignment")]
        [Tooltip("Enable pixel snapping to prevent sub-pixel jitter or blur matching the 16 PPU setup.")]
        [SerializeField] private bool snapToPixelGrid = true;

        [Tooltip("Pixels per unit used for grid snapping.")]
        [SerializeField] private float pixelsPerUnit = 16f;

        private Vector3 _previousCameraPosition;
        private Vector3 _accumulatedPosition;

        /// <summary>
        /// Public accessor for the parallax factor.
        /// </summary>
        public float ParallaxFactor
        {
            get => parallaxFactor;
            set => parallaxFactor = value;
        }

        private void Awake()
        {
            _accumulatedPosition = transform.position;
        }

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                _previousCameraPosition = targetCamera.transform.position;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null) return;
                _previousCameraPosition = targetCamera.transform.position;
            }

            Vector3 currentCamPos = targetCamera.transform.position;
            Vector3 camDelta = currentCamPos - _previousCameraPosition;

            // Offset layer based on camera movement delta scaled by parallaxFactor
            _accumulatedPosition.x += camDelta.x * parallaxFactor;
            _accumulatedPosition.y += camDelta.y * parallaxFactor;

            if (snapToPixelGrid && pixelsPerUnit > 0f)
            {
                float snappedX = Mathf.Round(_accumulatedPosition.x * pixelsPerUnit) / pixelsPerUnit;
                float snappedY = Mathf.Round(_accumulatedPosition.y * pixelsPerUnit) / pixelsPerUnit;
                transform.position = new Vector3(snappedX, snappedY, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(_accumulatedPosition.x, _accumulatedPosition.y, transform.position.z);
            }

            _previousCameraPosition = currentCamPos;
        }
    }
}
