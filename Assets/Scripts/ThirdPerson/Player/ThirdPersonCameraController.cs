using System.Collections;
using UnityEngine;
using LastGod.ThirdPerson.Combat;

namespace LastGod.ThirdPerson.Player
{
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Target & Positioning")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 pivotOffset = new Vector3(0f, 1.4f, 0f);
        [SerializeField] private Vector3 shoulderOffset = new Vector3(0.45f, 0.15f, 0f);
        [SerializeField] private float defaultDistance = 3.6f;
        [SerializeField] private float minDistance = 0.8f;
        [SerializeField] private float maxDistance = 5.0f;

        [Header("Rotation & Orbit")]
        [SerializeField] private float yawSpeed = 140f;
        [SerializeField] private float pitchSpeed = 100f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 65f;
        [SerializeField] private float rotationSmoothTime = 0.04f;

        [Header("Obstacle Collision (Spring Arm)")]
        [SerializeField] private LayerMask collisionLayers;
        [SerializeField] private float collisionRadius = 0.25f;
        [SerializeField] private float collisionSmoothSpeed = 16f;

        [Header("Lock-on Settings")]
        [SerializeField] private float lockOnPitch = 12f;
        [SerializeField] private float lockOnSmoothTime = 0.15f;

        private float _yaw;
        private float _pitch = 15f;
        private float _currentDistance;
        private float _targetDistance;
        private Vector3 _currentVelocity;
        private Vector2 _rotationVelocity;

        private bool _isCinematicControlled;
        private Vector3 _cinematicPos;
        private Quaternion _cinematicRot;
        private float _cinematicFOV = 60f;
        private Camera _cam;

        // Screen Shake
        private float _shakeIntensity;
        private float _shakeDuration;

        public bool IsCinematicControlled => _isCinematicControlled;
        public Camera MainCamera => _cam;

        private void Awake()
        {
            _cam = GetComponentInChildren<Camera>() ?? Camera.main;
            _currentDistance = defaultDistance;
            _targetDistance = defaultDistance;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetCinematicTransform(Vector3 position, Quaternion rotation, float fov = 60f)
        {
            _isCinematicControlled = true;
            _cinematicPos = position;
            _cinematicRot = rotation;
            _cinematicFOV = fov;
        }

        public void ReleaseCinematicControl()
        {
            _isCinematicControlled = false;
            if (_cam != null) _cam.fieldOfView = 60f;
            if (target != null)
            {
                _yaw = target.eulerAngles.y;
                _pitch = 15f;
            }
        }

        public void TriggerShake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeDuration = duration;
        }

        public void TriggerBulletFocus(Transform bullet)
        {
            StartCoroutine(BulletFocusRoutine(bullet));
        }

        private IEnumerator BulletFocusRoutine(Transform bullet)
        {
            if (target == null) yield break;

            float timer = 0f;
            float maxTime = 1.0f;

            while (timer < maxTime && bullet != null)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void LateUpdate()
        {
            if (_isCinematicControlled)
            {
                transform.position = Vector3.Lerp(transform.position, _cinematicPos, Time.deltaTime * 8f);
                transform.rotation = Quaternion.Slerp(transform.rotation, _cinematicRot, Time.deltaTime * 8f);
                if (_cam != null)
                {
                    _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, _cinematicFOV, Time.deltaTime * 5f);
                }
                return;
            }

            if (target == null) return;

            HandleInputAndOrbit();
            HandleCollision();
            ApplyTransform();
            ApplyScreenShake();
        }

        private void HandleInputAndOrbit()
        {
            var cm = CombatManager.Instance;
            if (cm != null && cm.HasLockedTarget)
            {
                // Auto track locked target
                Vector3 toTarget = cm.CurrentLockedTarget.position - target.position;
                float targetYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
                _yaw = Mathf.SmoothDampAngle(_yaw, targetYaw, ref _rotationVelocity.x, lockOnSmoothTime);
                _pitch = Mathf.SmoothDampAngle(_pitch, lockOnPitch, ref _rotationVelocity.y, lockOnSmoothTime);
            }
            else
            {
                float mx = Input.GetAxis("Mouse X") * (yawSpeed * 0.02f);
                float my = Input.GetAxis("Mouse Y") * (pitchSpeed * 0.02f);

                _yaw += mx;
                _pitch = Mathf.Clamp(_pitch - my, minPitch, maxPitch);
            }
        }

        private void HandleCollision()
        {
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 pivot = target.position + pivotOffset;
            Vector3 right = rot * Vector3.right;
            Vector3 adjustedPivot = pivot + right * shoulderOffset.x + Vector3.up * shoulderOffset.y;
            Vector3 camDir = rot * -Vector3.forward;

            float desiredDist = defaultDistance;

            if (Physics.SphereCast(adjustedPivot, collisionRadius, camDir, out RaycastHit hit, defaultDistance, collisionLayers, QueryTriggerInteraction.Ignore))
            {
                desiredDist = Mathf.Clamp(hit.distance - 0.1f, minDistance, defaultDistance);
            }

            _currentDistance = Mathf.Lerp(_currentDistance, desiredDist, Time.deltaTime * collisionSmoothSpeed);
        }

        private void ApplyTransform()
        {
            Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 pivot = target.position + pivotOffset;
            Vector3 right = rot * Vector3.right;
            Vector3 adjustedPivot = pivot + right * shoulderOffset.x + Vector3.up * shoulderOffset.y;
            Vector3 camPos = adjustedPivot + rot * (-Vector3.forward * _currentDistance);

            transform.position = camPos;
            transform.rotation = rot;
        }

        private void ApplyScreenShake()
        {
            if (_shakeDuration > 0f)
            {
                _shakeDuration -= Time.deltaTime;
                Vector3 shakeOffset = Random.insideUnitSphere * _shakeIntensity;
                transform.position += shakeOffset;
                _shakeIntensity = Mathf.Lerp(_shakeIntensity, 0f, Time.deltaTime * 6f);
            }
        }
    }
}
