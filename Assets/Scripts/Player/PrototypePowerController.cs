using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LastGod.Core;

namespace LastGod.Player
{
    /// <summary>
    /// Brain and state manager for Prototype Character powers post-glass chamber.
    /// Manages power locking, stasis dampening notifications, and activation logic.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PrototypePowerController : MonoBehaviour
    {
        [Header("Stasis Chamber Power Lock State")]
        [Tooltip("When true, all special prototype powers are locked due to post-chamber stasis dampening.")]
        [SerializeField] private bool isPowerLockActive = true;
        [SerializeField] private string currentLockReason = "Post-Chamber Stasis Dampener Active";

        [Header("Audio Feedback")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip powerLockedSFX;
        [SerializeField] private AudioClip powerUnlockedSFX;

        [Header("Events")]
        public UnityEvent<PrototypePowerType, bool> OnPowerLockStateChanged = new();
        public UnityEvent<PrototypePowerType, string> OnPowerAttemptFailed = new();

        // HashSet tracking explicitly unlocked powers
        private readonly HashSet<PrototypePowerType> _unlockedPowers = new();

        // UI Notification State
        private string _activeNotificationMessage = "";
        private float _notificationDisplayTimer = 0f;
        private GUIStyle _lockNotificationStyle;
        private Texture2D _lockBgTexture;

        public bool IsPowerLockActive => isPowerLockActive;
        public string CurrentLockReason => currentLockReason;

        private void Awake()
        {
            if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

            // Basic physical actions are ALWAYS unlocked
            _unlockedPowers.Add(PrototypePowerType.BasicMovement);
            _unlockedPowers.Add(PrototypePowerType.BasicAttack);
            _unlockedPowers.Add(PrototypePowerType.DodgeRoll);
            _unlockedPowers.Add(PrototypePowerType.ShieldBlock);

            // Special divine powers are LOCKED by default after leaving the glass chamber
            Debug.Log($"[PrototypePowerController] Initialized. Post-chamber power lock status: {isPowerLockActive} ({currentLockReason})");
        }

        private void Update()
        {
            if (_notificationDisplayTimer > 0f)
            {
                _notificationDisplayTimer -= Time.deltaTime;
            }

            CheckPowerHotkeys();
        }

        /// <summary>
        /// Monitors hotkeys for prototype character actions (1-5, Q, R, T, G, V, Tab).
        /// </summary>
        private void CheckPowerHotkeys()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.digit1Key.wasPressedThisFrame || kb.qKey.wasPressedThisFrame)
                    TryActivatePower(PrototypePowerType.EnergyWave);
                else if (kb.digit2Key.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
                    TryActivatePower(PrototypePowerType.Teleport);
                else if (kb.digit3Key.wasPressedThisFrame || kb.rKey.wasPressedThisFrame)
                    TryActivatePower(PrototypePowerType.MagicShield);
                else if (kb.digit4Key.wasPressedThisFrame || kb.tKey.wasPressedThisFrame)
                    TryActivatePower(PrototypePowerType.TimeSlow);
                else if (kb.digit5Key.wasPressedThisFrame || kb.gKey.wasPressedThisFrame)
                    TryActivatePower(PrototypePowerType.StealthMode);
                else if (kb.vKey.wasPressedThisFrame)
                    TryActivatePower(PrototypePowerType.PowerBoost);
                else if (kb.tabKey.wasPressedThisFrame)
                    TryActivatePower(PrototypePowerType.EnergyCharge);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Q))
                    TryActivatePower(PrototypePowerType.EnergyWave);
                else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.E))
                    TryActivatePower(PrototypePowerType.Teleport);
                else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.R))
                    TryActivatePower(PrototypePowerType.MagicShield);
                else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.T))
                    TryActivatePower(PrototypePowerType.TimeSlow);
                else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.G))
                    TryActivatePower(PrototypePowerType.StealthMode);
                else if (Input.GetKeyDown(KeyCode.V))
                    TryActivatePower(PrototypePowerType.PowerBoost);
                else if (Input.GetKeyDown(KeyCode.Tab))
                    TryActivatePower(PrototypePowerType.EnergyCharge);
            }
            catch {}
#endif
        }

        /// <summary>
        /// Attempts to activate a prototype power. If locked, plays warning SFX and displays UI notification.
        /// </summary>
        public bool TryActivatePower(PrototypePowerType power)
        {
            if (IsPowerUnlocked(power))
            {
                ExecutePowerAction(power);
                return true;
            }

            // Power is LOCKED
            string message = $"🔒 POWER LOCKED [{power}]: {currentLockReason}";
            ShowNotification(message, 2.2f);

            if (sfxSource != null && powerLockedSFX != null)
            {
                sfxSource.PlayOneShot(powerLockedSFX);
            }

            OnPowerAttemptFailed.Invoke(power, message);
            Debug.LogWarning($"[PrototypePowerController] Attempted locked power: {power}. Reason: {currentLockReason}");
            return false;
        }

        private void ExecutePowerAction(PrototypePowerType power)
        {
            Debug.Log($"[PrototypePowerController] Executing unlocked power: {power}");
            // Unlocked power execution logic
        }

        /// <summary>
        /// Checks whether a power is currently unlocked.
        /// </summary>
        public bool IsPowerUnlocked(PrototypePowerType power)
        {
            // Basic physical skills are always unlocked
            if (power == PrototypePowerType.BasicMovement ||
                power == PrototypePowerType.BasicAttack ||
                power == PrototypePowerType.DodgeRoll ||
                power == PrototypePowerType.ShieldBlock)
            {
                return true;
            }

            // If global power lock is active, all special divine powers are locked unless explicitly unlocked
            if (isPowerLockActive)
            {
                return _unlockedPowers.Contains(power);
            }

            return true;
        }

        /// <summary>
        /// Sets master power lock state (e.g. called when leaving glass chamber).
        /// </summary>
        public void SetPowerLockState(bool locked, string reason = "Post-Chamber Stasis Dampener Active")
        {
            isPowerLockActive = locked;
            currentLockReason = reason;

            string statusMsg = locked ? $"🔒 POWERS LOCKED: {reason}" : "⚡ POWERS UNLOCKED!";
            ShowNotification(statusMsg, 3.0f);

            if (!locked && sfxSource != null && powerUnlockedSFX != null)
            {
                sfxSource.PlayOneShot(powerUnlockedSFX);
            }

            Debug.Log($"[PrototypePowerController] Power lock state set to {locked}. Reason: {reason}");
        }

        /// <summary>
        /// Unlocks a specific prototype power.
        /// </summary>
        public void UnlockPower(PrototypePowerType power)
        {
            if (!_unlockedPowers.Contains(power))
            {
                _unlockedPowers.Add(power);
                OnPowerLockStateChanged.Invoke(power, true);
                ShowNotification($"⚡ UNLOCKED POWER: {power}!", 2.5f);
                Debug.Log($"[PrototypePowerController] Power unlocked: {power}");
            }
        }

        /// <summary>
        /// Locks a specific prototype power.
        /// </summary>
        public void LockPower(PrototypePowerType power)
        {
            if (_unlockedPowers.Contains(power))
            {
                _unlockedPowers.Remove(power);
                OnPowerLockStateChanged.Invoke(power, false);
                Debug.Log($"[PrototypePowerController] Power locked: {power}");
            }
        }

        /// <summary>
        /// Unlocks all special prototype powers.
        /// </summary>
        public void UnlockAllPowers()
        {
            SetPowerLockState(false, "Stasis Field Deactivated");
            foreach (PrototypePowerType p in Enum.GetValues(typeof(PrototypePowerType)))
            {
                _unlockedPowers.Add(p);
            }
            ShowNotification("⚡ ALL DIVINE POWERS UNLOCKED!", 3.5f);
        }

        /// <summary>
        /// Locks all special divine powers (stasis lock active).
        /// </summary>
        public void LockAllSpecialPowers()
        {
            _unlockedPowers.Clear();
            _unlockedPowers.Add(PrototypePowerType.BasicMovement);
            _unlockedPowers.Add(PrototypePowerType.BasicAttack);
            _unlockedPowers.Add(PrototypePowerType.DodgeRoll);
            _unlockedPowers.Add(PrototypePowerType.ShieldBlock);

            SetPowerLockState(true, "Post-Chamber Stasis Suppression");
        }

        public void ShowNotification(string msg, float duration = 2.0f)
        {
            _activeNotificationMessage = msg;
            _notificationDisplayTimer = duration;
        }

        private void OnGUI()
        {
            if (_notificationDisplayTimer <= 0f || string.IsNullOrEmpty(_activeNotificationMessage))
                return;

            InitGUIStyle();

            float screenW = Screen.width;
            float boxW = Mathf.Min(480f, screenW * 0.9f);
            float boxH = 38f;
            float x = (screenW - boxW) * 0.5f;
            float y = 70f;

            // Background
            GUI.color = new Color(0.12f, 0.02f, 0.02f, 0.92f);
            GUI.DrawTexture(new Rect(x, y, boxW, boxH), _lockBgTexture);

            // Gold/Red Border
            GUI.color = _activeNotificationMessage.Contains("UNLOCKED") ? new Color(0.2f, 0.9f, 0.3f, 1f) : new Color(0.95f, 0.25f, 0.25f, 1f);
            GUI.DrawTexture(new Rect(x, y, boxW, 2), _lockBgTexture);
            GUI.DrawTexture(new Rect(x, y + boxH - 2, boxW, 2), _lockBgTexture);

            // Label
            GUI.color = Color.white;
            GUI.Label(new Rect(x + 10, y + 8, boxW - 20, boxH - 16), _activeNotificationMessage, _lockNotificationStyle);
        }

        private void InitGUIStyle()
        {
            if (_lockNotificationStyle == null)
            {
                _lockNotificationStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    richText = true,
                    normal = { textColor = Color.white }
                };
            }

            if (_lockBgTexture == null)
            {
                _lockBgTexture = new Texture2D(2, 2);
                Color[] pix = new Color[4];
                for (int i = 0; i < 4; i++) pix[i] = Color.white;
                _lockBgTexture.SetPixels(pix);
                _lockBgTexture.Apply();
            }
        }

        private void OnDestroy()
        {
            if (_lockBgTexture != null) Destroy(_lockBgTexture);
        }
    }
}
