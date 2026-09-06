using UnityEngine;

namespace LastGod.Environment
{
    /// <summary>
    /// Smooth vertical and horizontal bobbing motion for 2D ambient props such as 
    /// floating fog patches, magical motes, floating platforms, lanterns, or airborne particles.
    /// </summary>
    public class FloatingProp : MonoBehaviour
    {
        [Header("Vertical Motion")]
        [Tooltip("Height amplitude of bobbing motion.")]
        [SerializeField] private float floatAmplitudeY = 0.25f;

        [Tooltip("Speed of vertical floating oscillation.")]
        [SerializeField] private float floatSpeedY = 1.5f;

        [Header("Horizontal Motion")]
        [Tooltip("Horizontal sway amplitude.")]
        [SerializeField] private float floatAmplitudeX = 0.1f;

        [Tooltip("Speed of horizontal floating oscillation.")]
        [SerializeField] private float floatSpeedX = 1f;

        [Header("Phase Offset")]
        [Tooltip("Random phase offset to prevent synchronized bobbing.")]
        [SerializeField] private float phaseOffset = 0f;

        private Vector3 _startPosition;
        private float _time;

        private void Start()
        {
            _startPosition = transform.localPosition;

            if (phaseOffset == 0f)
            {
                phaseOffset = Random.Range(0f, 100f);
            }
        }

        private void Update()
        {
            _time += Time.deltaTime;

            float offsetY = Mathf.Sin((_time * floatSpeedY) + phaseOffset) * floatAmplitudeY;
            float offsetX = Mathf.Cos((_time * floatSpeedX) + phaseOffset) * floatAmplitudeX;

            transform.localPosition = _startPosition + new Vector3(offsetX, offsetY, 0f);
        }
    }
}
