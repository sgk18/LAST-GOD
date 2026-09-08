using UnityEngine;

namespace LastGod.Player
{
    /// <summary>
    /// Minimal hard-follow camera. No smoothing yet — keeps pixels locked to grid.
    /// Attach to Main Camera. Assign the player transform in Inspector.
    ///
    /// Smoothing / bounds clamping will be added in Prompt 4.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [Tooltip("Z offset for 2D cameras (keep negative, e.g. -10).")]
        [SerializeField] private float cameraZ = -10f;
        [Tooltip("Y offset to center camera on character's body instead of feet.")]
        [SerializeField] private float offsetY = 2.8f;
        [Tooltip("Smooth follow speed (0 = instant snap).")]
        [SerializeField] private float smoothSpeed = 0f;

        private void Awake()
        {
            FindPlayerTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindPlayerTarget();
                if (target == null) return;
            }

            Vector3 targetPos = new Vector3(target.position.x, target.position.y + offsetY, cameraZ);
            if (smoothSpeed > 0f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = targetPos;
            }
        }

        private void FindPlayerTarget()
        {
            if (target != null) return;
            var pc = FindAnyObjectByType<PlayerController>();
            if (pc != null) target = pc.transform;
        }

#if UNITY_EDITOR
        // If no target is assigned, try to auto-find the player in the editor.
        private void OnValidate()
        {
            if (target == null)
            {
                var pc = FindAnyObjectByType<PlayerController>();
                if (pc != null)
                {
                    target = pc.transform;
                    Debug.Log("[CameraFollow] Auto-assigned target to Player.");
                }
            }
        }
#endif
    }
}
