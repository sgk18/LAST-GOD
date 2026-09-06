using UnityEngine;
using UnityEngine.Events;
using LastGod.Core;

namespace LastGod.Player
{
    /// <summary>
    /// Trigger detector placed at the glass chamber exit in Act 1 Scene 1.
    /// When the player exits the chamber area after shattering the glass,
    /// this script confirms the post-chamber power lock status on the Prototype Controller.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GlassChamberExitTrigger : MonoBehaviour
    {
        [Header("Chamber Settings")]
        [SerializeField] private bool lockPowersOnExit = true;
        [SerializeField] private string lockReason = "Post-Chamber Stasis Collars Active";

        [Header("Events")]
        public UnityEvent OnPlayerExitedChamber = new();

        private bool _hasTriggered = false;

        private void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_hasTriggered) return;

            if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
            {
                _hasTriggered = true;
                ExecuteChamberExit(other.gameObject);
            }
        }

        public void ExecuteChamberExit(GameObject playerInstance)
        {
            if (playerInstance == null) return;

            PrototypePowerController powerCtrl = playerInstance.GetComponent<PrototypePowerController>();
            if (powerCtrl == null)
            {
                powerCtrl = playerInstance.AddComponent<PrototypePowerController>();
            }

            if (lockPowersOnExit && powerCtrl != null)
            {
                powerCtrl.SetPowerLockState(true, lockReason);
                powerCtrl.ShowNotification("🔒 STASIS COLLAR ENGAGED: Divine Powers Suppressed Post-Chamber Exit!", 3.0f);
            }

            OnPlayerExitedChamber.Invoke();
            Debug.Log("[GlassChamberExitTrigger] Player exited glass chamber. Post-chamber power lock confirmed.");
        }
    }
}
