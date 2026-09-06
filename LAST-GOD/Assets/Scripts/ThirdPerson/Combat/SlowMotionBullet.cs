using System.Collections;
using UnityEngine;
using LastGod.ThirdPerson.Player;

namespace LastGod.ThirdPerson.Combat
{
    public class SlowMotionBullet : MonoBehaviour
    {
        [Header("Bullet Dynamics")]
        [SerializeField] private float normalSpeed = 22f;
        [SerializeField] private float damage = 12f;
        [SerializeField] private float maxLifetime = 6f;
        [SerializeField] private LayerMask collisionLayers;

        [Header("Slow-Motion Zone")]
        [SerializeField] private float slowTriggerDistance = 4.5f;
        [SerializeField] private float slowTimeScale = 0.15f;
        [SerializeField] private float slowDurationRealtime = 1.2f;

        [Header("Visuals & Audio")]
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private ParticleSystem muzzleVFX;
        [SerializeField] private ParticleSystem impactVFX;

        private Vector3 _velocity;
        private bool _hasTriggeredSlowMo;
        private Transform _playerTransform;
        private static bool _globalSlowMoActive;

        public static bool IsGlobalSlowMoActive => _globalSlowMoActive;

        public void Initialize(Vector3 direction, Transform targetPlayer)
        {
            _velocity = direction.normalized * normalSpeed;
            _playerTransform = targetPlayer;
            transform.forward = direction;
            Destroy(gameObject, maxLifetime);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            Vector3 nextPos = transform.position + _velocity * dt;

            // Check distance to player for the cinematic slow-motion perception moment
            if (!_hasTriggeredSlowMo && !_globalSlowMoActive && _playerTransform != null)
            {
                float dist = Vector3.Distance(transform.position, _playerTransform.position);
                if (dist <= slowTriggerDistance)
                {
                    _hasTriggeredSlowMo = true;
                    StartCoroutine(TriggerPerceptionSlowdown());
                }
            }

            // Raycast collision detection to prevent tunneling
            if (Physics.Raycast(transform.position, _velocity.normalized, out RaycastHit hit, _velocity.magnitude * dt, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                OnHit(hit);
                return;
            }

            transform.position = nextPos;
        }

        private IEnumerator TriggerPerceptionSlowdown()
        {
            _globalSlowMoActive = true;
            float originalFixedDelta = Time.fixedDeltaTime;

            Time.timeScale = slowTimeScale;
            Time.fixedDeltaTime = 0.02f * slowTimeScale;

            // Notify player camera/controller of slow motion bullet dodge moment
            var cam = FindAnyObjectByType<ThirdPersonCameraController>();
            if (cam != null)
            {
                cam.TriggerBulletFocus(transform);
            }

            // Wait in unscaled real time
            float elapsed = 0f;
            while (elapsed < slowDurationRealtime)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Smoothly restore normal time scale
            float restoreDuration = 0.25f;
            float t = 0f;
            while (t < restoreDuration)
            {
                t += Time.unscaledDeltaTime;
                float progress = t / restoreDuration;
                Time.timeScale = Mathf.Lerp(slowTimeScale, 1.0f, progress);
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                yield return null;
            }

            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = originalFixedDelta;
            _globalSlowMoActive = false;
        }

        private void OnHit(RaycastHit hit)
        {
            if (hit.collider.TryGetComponent<IDamageReceiver>(out var receiver))
            {
                receiver.TakeDamage(new DamageInfo(
                    damage,
                    gameObject,
                    hit.point,
                    hit.normal,
                    _velocity.normalized,
                    5f
                ));
            }

            if (impactVFX != null)
            {
                impactVFX.transform.position = hit.point;
                impactVFX.transform.rotation = Quaternion.LookRotation(hit.normal);
                impactVFX.Play();
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (_hasTriggeredSlowMo && _globalSlowMoActive)
            {
                Time.timeScale = 1.0f;
                Time.fixedDeltaTime = 0.02f;
                _globalSlowMoActive = false;
            }
        }
    }
}
