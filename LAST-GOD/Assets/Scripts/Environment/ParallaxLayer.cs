using UnityEngine;

namespace LastGod.Environment
{
    /// <summary>
    /// Controls 2D Parallax scrolling for a background, midground, or foreground layer.
    /// Supports distance-based parallax multipliers, infinite looping, and optional continuous auto-scroll.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [Header("Parallax Settings")]
        [Tooltip("0 = fixed to background (moves with camera 0%), 1 = fixed to foreground (moves with camera 100%). Values < 1 sit behind camera focus.")]
        [Range(0f, 1.5f)]
        [SerializeField] private float parallaxFactorX = 0.5f;

        [Range(0f, 1.5f)]
        [SerializeField] private float parallaxFactorY = 0.2f;

        [Header("Infinite Scrolling Settings")]
        [Tooltip("If true, automatically tiles/repositions this layer horizontally as the camera moves past bounds.")]
        [SerializeField] private bool infiniteHorizontal = true;

        [Tooltip("The horizontal width of the background sprite/texture in world units. Auto-calculated if 0.")]
        [SerializeField] private float textureSizeX = 0f;

        [Header("Auto-Scroll Settings")]
        [Tooltip("Constant horizontal movement speed independent of camera (e.g. drifting clouds, moving fog).")]
        [SerializeField] private float autoScrollSpeedX = 0f;

        [Tooltip("Constant vertical movement speed.")]
        [SerializeField] private float autoScrollSpeedY = 0f;

        private Transform _cameraTransform;
        private Vector3 _lastCameraPosition;
        private float _autoScrollOffsetAccumulatorX;
        private float _autoScrollOffsetAccumulatorY;

        public float ParallaxFactorX { get => parallaxFactorX; set => parallaxFactorX = value; }
        public float ParallaxFactorY { get => parallaxFactorY; set => parallaxFactorY = value; }

        private void Start()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
                _lastCameraPosition = _cameraTransform.position;
            }

            // Auto calculate texture size if not set manually
            if (textureSizeX <= 0f)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    textureSizeX = sr.bounds.size.x;
                }
            }
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null)
            {
                if (Camera.main != null)
                {
                    _cameraTransform = Camera.main.transform;
                    _lastCameraPosition = _cameraTransform.position;
                }
                return;
            }

            Vector3 cameraDelta = _cameraTransform.position - _lastCameraPosition;

            // Compute Parallax Displacement
            float moveX = cameraDelta.x * parallaxFactorX;
            float moveY = cameraDelta.y * parallaxFactorY;

            // Add constant auto-scrolling
            if (autoScrollSpeedX != 0f)
            {
                _autoScrollOffsetAccumulatorX += autoScrollSpeedX * Time.deltaTime;
                moveX += autoScrollSpeedX * Time.deltaTime;
            }

            if (autoScrollSpeedY != 0f)
            {
                _autoScrollOffsetAccumulatorY += autoScrollSpeedY * Time.deltaTime;
                moveY += autoScrollSpeedY * Time.deltaTime;
            }

            transform.position += new Vector3(moveX, moveY, 0f);

            // Handle infinite horizontal looping
            if (infiniteHorizontal && textureSizeX > 0f)
            {
                float cameraDistFromLayer = _cameraTransform.position.x - transform.position.x;
                if (Mathf.Abs(cameraDistFromLayer) >= textureSizeX)
                {
                    float offsetPositionX = (cameraDistFromLayer % textureSizeX);
                    transform.position = new Vector3(_cameraTransform.position.x - offsetPositionX, transform.position.y, transform.position.z);
                }
            }

            _lastCameraPosition = _cameraTransform.position;
        }
    }
}
