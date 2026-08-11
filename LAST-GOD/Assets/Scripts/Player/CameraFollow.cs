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

        private void LateUpdate()
        {
            if (target == null) return;

            // Hard snap — every frame, no lerp.
            // Pixel Perfect Camera will handle sub-pixel snapping.
            transform.position = new Vector3(target.position.x, target.position.y, cameraZ);
        }

#if UNITY_EDITOR
        // If no target is assigned, try to auto-find the player in the editor.
        private void OnValidate()
        {
            if (target == null)
            {
                var pc = FindFirstObjectByType<PlayerController>();
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
