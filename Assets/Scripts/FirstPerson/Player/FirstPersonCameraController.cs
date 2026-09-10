using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastGod.FirstPerson.Player
{
    /// <summary>
    /// First-person cinematic camera controller for Aeron.
    /// Operates at human eye level (~1.70m) with 80° FOV, clamped pitch (-85° to +85°),
    /// anti-clipping near plane, head shadow-only culling, and restrained physical view bob.
    /// </summary>
    public class FirstPersonCameraController : MonoBehaviour
    {
        [Header("Camera & View Settings")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Transform playerBody;
        [Range(75f, 85f)]
        [SerializeField] private float fieldOfView = 80f;
        [SerializeField] private float nearClipPlane = 0.05f;

        [Header("Look Sensitivity & Smoothing")]
        [SerializeField] private float mouseSensitivity = 2.0f;
        [SerializeField] private float smoothing = 0.02f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;

        [Header("Head Culling / Shadows")]
        [Tooltip("Renderer containing head/hair geometry. Converted to ShadowsOnly locally to prevent camera clipping while keeping shadows.")]
        [SerializeField] private Renderer headRenderer;

        [Header("Physical Head Bob (Restrained)")]
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float bobFrequency = 8.0f;
        [SerializeField] private float bobHorizontalAmplitude = 0.012f;
        [SerializeField] private float bobVerticalAmplitude = 0.018f;

        // Runtime state
        private float _pitch;
        private float _yaw;
        private Vector2 _currentMouseDelta;
        private Vector2 _mouseDeltaSmoothVelocity;
        private Vector3 _originalHolderLocalPos;
        private float _bobTimer;
        private FirstPersonPlayerController _playerController;

        public Camera PlayerCamera => playerCamera;
        public float Pitch => _pitch;
        public float Yaw => _yaw;

        private void Awake()
        {
            if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
            if (cameraHolder == null) cameraHolder = transform;
            if (playerBody == null && transform.parent != null) playerBody = transform.parent;

            _playerController = playerBody != null ? playerBody.GetComponent<FirstPersonPlayerController>() : null;
            _originalHolderLocalPos = cameraHolder.localPosition;

            ConfigureCamera();
            ConfigureHeadCulling();
        }

        private void Start()
        {
            // Lock cursor during gameplay
            if (Application.isPlaying)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private void ConfigureCamera()
        {
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = fieldOfView;
                playerCamera.nearClipPlane = nearClipPlane;
            }
        }

        private void ConfigureHeadCulling()
        {
            if (headRenderer != null)
            {
                // Head casts shadows in environment but does not obstruct first-person view
                headRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            }
        }

        private void Update()
        {
            HandleMouseLook();
            HandleHeadBob();
        }

        private void HandleMouseLook()
        {
            float mouseX = UnityEngine.Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float mouseY = UnityEngine.Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            Vector2 targetDelta = new Vector2(mouseX, mouseY);
            _currentMouseDelta = Vector2.SmoothDamp(_currentMouseDelta, targetDelta, ref _mouseDeltaSmoothVelocity, smoothing);

            _pitch -= _currentMouseDelta.y;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            // Apply pitch rotation to CameraHolder
            cameraHolder.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            // Apply yaw rotation to PlayerBody
            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * _currentMouseDelta.x);
            }
        }

        private void HandleHeadBob()
        {
            if (!enableHeadBob || _playerController == null) return;

            if (_playerController.IsGrounded && _playerController.IsMoving)
            {
                float speedFactor = Mathf.Clamp01(_playerController.CurrentSpeed / 6.5f);
                _bobTimer += Time.deltaTime * bobFrequency * Mathf.Max(0.5f, speedFactor);

                float hBob = Mathf.Sin(_bobTimer) * bobHorizontalAmplitude * speedFactor;
                float vBob = Mathf.Abs(Mathf.Cos(_bobTimer)) * bobVerticalAmplitude * speedFactor;

                playerCamera.transform.localPosition = new Vector3(hBob, vBob, 0);
            }
            else
            {
                _bobTimer = 0;
                playerCamera.transform.localPosition = Vector3.MoveTowards(
                    playerCamera.transform.localPosition,
                    Vector3.zero,
                    Time.deltaTime * 2f
                );
            }
        }
    }
}
