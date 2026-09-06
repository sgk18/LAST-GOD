using UnityEngine;

namespace LastGod.Environment
{
    /// <summary>
    /// Animates 2D background props (trees, bushes, banners, hanging lamps, signs, chains)
    /// with realistic pendulum sway or wind oscillation using trigonometric sine/perlin noise.
    /// </summary>
    public class SwayingProp : MonoBehaviour
    {
        [Header("Sway Motion")]
        [Tooltip("Maximum rotation angle in degrees.")]
        [SerializeField] private float swayAngle = 5f;

        [Tooltip("Speed of the sway movement.")]
        [SerializeField] private float swaySpeed = 2f;

        [Header("Wind Turbulence")]
        [Tooltip("Random phase offset so multiple props don't sway synchronously.")]
        [SerializeField] private float phaseOffset = 0f;

        [Tooltip("If true, adds subtle Perlin noise turbulence for natural wind gusts.")]
        [SerializeField] private bool useWindGusts = true;

        [Tooltip("Intensity of wind gust turbulence.")]
        [SerializeField] private float gustIntensity = 0.3f;

        [Header("Pivot Settings")]
        [Tooltip("Pivot point anchor relative to base object transform.")]
        [SerializeField] private Vector3 pivotOffset = Vector3.zero;

        private Quaternion _initialRotation;
        private float _time;

        private void Start()
        {
            _initialRotation = transform.localRotation;

            // Randomize phase offset if 0 to give natural variety between props
            if (phaseOffset == 0f)
            {
                phaseOffset = Random.Range(0f, 100f);
            }
        }

        private void Update()
        {
            _time += Time.deltaTime * swaySpeed;

            // Sine wave base sway
            float sineAngle = Mathf.Sin(_time + phaseOffset) * swayAngle;

            // Wind gust perlin noise
            if (useWindGusts)
            {
                float noise = (Mathf.PerlinNoise(_time * 0.5f, phaseOffset) - 0.5f) * 2f;
                sineAngle += noise * swayAngle * gustIntensity;
            }

            // Apply rotation around pivot offset
            transform.localRotation = _initialRotation * Quaternion.Euler(0f, 0f, sineAngle);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position + pivotOffset, 0.1f);
        }
    }
}
