using System.Collections;
using UnityEngine;

namespace LastGod.Characters.Aeron
{
    public enum IdleVariationType
    {
        NormalBreathing,     // 70%
        SubtlePostureShift,  // 15%
        SubtleHeadMovement,  // 10%
        MicroMovement        // 5%
    }

    /// <summary>
    /// Manages Aeron's subtle, cinematic idle animation states and micro-variations.
    /// Ensures natural breathing rhythm, stable foot grounding, and rare, organic secondary shifts.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class AeronIdleController : MonoBehaviour
    {
        [Header("Animator Control")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Range(0.1f, 2.0f)]
        [SerializeField] private float playbackSpeed = 1.0f;

        [Header("Grounding")]
        [SerializeField] private bool snapToGround = true;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckOffset = 0.1f;

        [Header("Idle Variation Tuning")]
        [SerializeField] private bool enableVariations = true;
        [SerializeField] private float minVariationInterval = 5.0f;
        [SerializeField] private float maxVariationInterval = 11.0f;

        [Header("Subtle Secondary Motion")]
        [SerializeField] private Transform headNode;
        [SerializeField] private Transform torsoNode;
        [SerializeField] private Transform backTubesNode;

        private float _variationTimer;
        private float _nextVariationDelay;
        private Vector3 _baseHeadLocalPos;
        private Quaternion _baseHeadLocalRot;
        private Vector3 _baseTorsoLocalPos;
        private bool _isGrounded = true;

        public bool IsGrounded => _isGrounded;
        public IdleVariationType LastVariation { get; private set; } = IdleVariationType.NormalBreathing;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            if (headNode != null)
            {
                _baseHeadLocalPos = headNode.localPosition;
                _baseHeadLocalRot = headNode.localRotation;
            }
            if (torsoNode != null)
            {
                _baseTorsoLocalPos = torsoNode.localPosition;
            }

            ScheduleNextVariation();
        }

        private void Start()
        {
            if (animator != null)
            {
                animator.speed = playbackSpeed;
            }

            if (snapToGround)
            {
                PerformGroundSnap();
            }
        }

        private void OnValidate()
        {
            if (animator != null)
            {
                animator.speed = playbackSpeed;
            }
        }

        private void Update()
        {
            if (enableVariations)
            {
                _variationTimer += Time.deltaTime;
                if (_variationTimer >= _nextVariationDelay)
                {
                    _variationTimer = 0f;
                    ScheduleNextVariation();
                    TriggerIdleVariation();
                }
            }
        }

        private void ScheduleNextVariation()
        {
            _nextVariationDelay = Random.Range(minVariationInterval, maxVariationInterval);
        }

        private void TriggerIdleVariation()
        {
            float roll = Random.value * 100f;

            if (roll < 70f)
            {
                LastVariation = IdleVariationType.NormalBreathing;
            }
            else if (roll < 85f)
            {
                LastVariation = IdleVariationType.SubtlePostureShift;
                StartCoroutine(SubtlePostureShiftRoutine());
            }
            else if (roll < 95f)
            {
                LastVariation = IdleVariationType.SubtleHeadMovement;
                StartCoroutine(SubtleHeadMovementRoutine());
            }
            else
            {
                LastVariation = IdleVariationType.MicroMovement;
                StartCoroutine(MicroMovementRoutine());
            }
        }

        private IEnumerator SubtlePostureShiftRoutine()
        {
            // Tiny, imperceptible weight shift (0.015 units)
            if (torsoNode == null) yield break;

            float duration = 1.8f;
            float elapsed = 0f;
            Vector3 target = _baseTorsoLocalPos + new Vector3(0.015f, 0f, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
                torsoNode.localPosition = Vector3.Lerp(_baseTorsoLocalPos, target, t);
                yield return null;
            }

            torsoNode.localPosition = _baseTorsoLocalPos;
        }

        private IEnumerator SubtleHeadMovementRoutine()
        {
            // Extremely subtle head tilt variation (< 1.5 degrees)
            if (headNode == null) yield break;

            float duration = 2.0f;
            float elapsed = 0f;
            Quaternion targetRot = _baseHeadLocalRot * Quaternion.Euler(0f, 0f, 1.2f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
                headNode.localRotation = Quaternion.Slerp(_baseHeadLocalRot, targetRot, t);
                yield return null;
            }

            headNode.localRotation = _baseHeadLocalRot;
        }

        private IEnumerator MicroMovementRoutine()
        {
            // Tiny back tube micro-sway
            if (backTubesNode == null) yield break;

            float duration = 1.5f;
            float elapsed = 0f;
            Quaternion baseRot = backTubesNode.localRotation;
            Quaternion target = baseRot * Quaternion.Euler(0f, 0f, -1.0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
                backTubesNode.localRotation = Quaternion.Slerp(baseRot, target, t);
                yield return null;
            }

            backTubesNode.localRotation = baseRot;
        }

        public void PerformGroundSnap()
        {
            // Raycast downward to ensure exact foot-to-ground alignment
            Vector3 origin = transform.position + Vector3.up * groundCheckOffset;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 2.0f, groundLayer.value != 0 ? groundLayer : ~0, QueryTriggerInteraction.Ignore))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                _isGrounded = true;
            }
            else if (Physics2D.Raycast(origin, Vector2.down, 2.0f).collider != null)
            {
                var hit2D = Physics2D.Raycast(origin, Vector2.down, 2.0f);
                transform.position = new Vector3(transform.position.x, hit2D.point.y, transform.position.z);
                _isGrounded = true;
            }
        }
    }
}
