using UnityEngine;

namespace LastGod.Characters.Aeron
{
    /// <summary>
    /// Smoothly billboards Aeron's 2D sprite visual rig to face the active camera,
    /// while dynamically flipping the sprite according to sideways movement.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class AeronBillboard : MonoBehaviour
    {
        [Header("Camera Alignment")]
        [SerializeField] private bool lockYAxis = true;
        [SerializeField] private bool smoothFacing = true;
        [SerializeField] private float turnSpeed = 24f;

        [Header("Sprite Flipping")]
        [SerializeField] private bool autoFlipWithMovement = true;
        [SerializeField] private float flipDeadzone = 0.15f;

        private Camera _cam;
        private SpriteRenderer _sr;
        private CharacterController _parentCC;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _parentCC = GetComponentInParent<CharacterController>();
        }

        private void LateUpdate()
        {
            if (_cam == null || !_cam.gameObject.activeInHierarchy)
            {
                _cam = Camera.main;
            }

            if (_cam == null) return;

            // In Unity, 2D sprites lie in the XY plane.
            // The front face has normal pointing along -Z.
            // Therefore, pointing the transform's forward (+Z) away from the camera
            // causes the front face (-Z) to look directly at the camera.
            Vector3 camToMe = transform.position - _cam.transform.position;
            if (lockYAxis)
            {
                camToMe.y = 0f;
            }

            if (camToMe.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(camToMe);
                if (smoothFacing)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
                }
                else
                {
                    transform.rotation = targetRot;
                }
            }

            // Flip sprite when moving sideways relative to camera view
            if (autoFlipWithMovement && _sr != null && _parentCC != null)
            {
                Vector3 vel = _parentCC.velocity;
                float dotRight = Vector3.Dot(_cam.transform.right, vel);
                if (dotRight > flipDeadzone)
                {
                    _sr.flipX = false;
                }
                else if (dotRight < -flipDeadzone)
                {
                    _sr.flipX = true;
                }
            }
        }
    }
}
