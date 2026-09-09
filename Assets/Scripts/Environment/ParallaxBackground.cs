using UnityEngine;

namespace LastGod.Environment
{
    /// <summary>
    /// Drives multi-layer parallax scrolling for the lab background.
    /// Attach to a parent GameObject; each layer child has a configured speed multiplier.
    /// Camera movement is tracked and each layer is offset by (cameraMove * speedMultiplier).
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            [Tooltip("The SpriteRenderer or child GameObject for this layer.")]
            public Transform layerTransform;

            [Tooltip("0 = pinned to camera (skybox), 1 = moves with world (foreground).")]
            [Range(0f, 1f)]
            public float speedMultiplier = 0.5f;

            [Tooltip("Enable infinite horizontal tiling for this layer.")]
            public bool infiniteScrollX = true;

            [HideInInspector] public float startPositionX;
            [HideInInspector] public float layerWidth;
        }

        [Header("Layers (Far → Near)")]
        [SerializeField] private ParallaxLayer[] _layers;

        [Header("Camera Reference")]
        [SerializeField] private Transform _cameraTransform;

        private float _previousCameraX;

        // ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (_cameraTransform == null)
            {
                var cam = Camera.main;
                if (cam != null) _cameraTransform = cam.transform;
            }

            foreach (var layer in _layers)
            {
                if (layer.layerTransform == null) continue;
                layer.startPositionX = layer.layerTransform.position.x;

                var sr = layer.layerTransform.GetComponentInChildren<SpriteRenderer>();
                layer.layerWidth = sr != null ? sr.bounds.size.x : 0f;
            }

            if (_cameraTransform != null)
                _previousCameraX = _cameraTransform.position.x;
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null) return;

            float currentCameraX = _cameraTransform.position.x;
            float deltaX = currentCameraX - _previousCameraX;
            _previousCameraX = currentCameraX;

            foreach (var layer in _layers)
            {
                if (layer.layerTransform == null) continue;

                float parallaxOffset = deltaX * layer.speedMultiplier;
                float newX = layer.layerTransform.position.x + parallaxOffset;

                // Infinite scroll: reposition layer when it moves too far off-screen
                if (layer.infiniteScrollX && layer.layerWidth > 0f)
                {
                    float relativeX = currentCameraX * (1f - layer.speedMultiplier);
                    float temp = Mathf.Repeat(relativeX, layer.layerWidth);

                    newX = layer.startPositionX + relativeX - temp;
                }

                layer.layerTransform.position = new Vector3(
                    newX,
                    layer.layerTransform.position.y,
                    layer.layerTransform.position.z
                );
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Guard against null refs during inspector edits
            if (_layers == null) return;
        }
#endif
    }
}
