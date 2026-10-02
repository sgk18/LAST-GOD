using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LastGod.ThirdPerson.Save;

namespace LastGod.UI
{
    /// <summary>
    /// Classified Facility Data Storage & Archive Terminal.
    /// Manages 3 Classified Archive slots with sector status, timestamps, and integrity stats.
    /// Supports both Save Mode (from Pause Menu) and Load Mode (from Title / Pause).
    /// </summary>
    public class SaveLoadMenuController : MonoBehaviour
    {
        public enum TerminalMode
        {
            Save,
            Load
        }

        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Header & Mode Labels")]
        [SerializeField] private TextMeshProUGUI headerTitleText;
        [SerializeField] private TextMeshProUGUI modeSubheaderText;

        [Header("Archive Slot 01 UI")]
        [SerializeField] private Button slot1Button;
        [SerializeField] private TextMeshProUGUI slot1TitleText;
        [SerializeField] private TextMeshProUGUI slot1DetailsText;
        [SerializeField] private Button slot1PurgeButton;

        [Header("Archive Slot 02 UI")]
        [SerializeField] private Button slot2Button;
        [SerializeField] private TextMeshProUGUI slot2TitleText;
        [SerializeField] private TextMeshProUGUI slot2DetailsText;
        [SerializeField] private Button slot2PurgeButton;

        [Header("Archive Slot 03 UI")]
        [SerializeField] private Button slot3Button;
        [SerializeField] private TextMeshProUGUI slot3TitleText;
        [SerializeField] private TextMeshProUGUI slot3DetailsText;
        [SerializeField] private Button slot3PurgeButton;

        [Header("Action Buttons")]
        [SerializeField] private Button backButton;

        [Header("Confirmation Overlay")]
        [SerializeField] private GameObject confirmOverlay;
        [SerializeField] private TextMeshProUGUI confirmPromptText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        public event Action OnBackPressed;
        public event Action<int> OnSlotLoaded;
        public event Action<int> OnSlotSaved;

        private TerminalMode _currentMode = TerminalMode.Load;
        private int _pendingSlot = -1;
        private bool _isPurgeAction = false;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            if (slot1Button != null) slot1Button.onClick.AddListener(() => OnSlotClicked(1));
            if (slot2Button != null) slot2Button.onClick.AddListener(() => OnSlotClicked(2));
            if (slot3Button != null) slot3Button.onClick.AddListener(() => OnSlotClicked(3));

            if (slot1PurgeButton != null) slot1PurgeButton.onClick.AddListener(() => PromptPurge(1));
            if (slot2PurgeButton != null) slot2PurgeButton.onClick.AddListener(() => PromptPurge(2));
            if (slot3PurgeButton != null) slot3PurgeButton.onClick.AddListener(() => PromptPurge(3));

            if (backButton != null) backButton.onClick.AddListener(HandleBack);

            if (confirmYesButton != null) confirmYesButton.onClick.AddListener(ExecuteConfirmedAction);
            if (confirmNoButton != null) confirmNoButton.onClick.AddListener(DismissConfirmOverlay);
        }

        private void Start()
        {
            DismissConfirmOverlay();
            RefreshSlotDisplay();
        }

        public void SetMode(TerminalMode mode)
        {
            _currentMode = mode;
            if (headerTitleText != null)
            {
                headerTitleText.text = mode == TerminalMode.Save ? "DATA ARCHIVE // RECORD MATRIX" : "ARCHIVE RETRIEVAL // RESTORE MATRIX";
            }
            if (modeSubheaderText != null)
            {
                modeSubheaderText.text = mode == TerminalMode.Save ? "FACILITY B-03 // SELECT SECTOR SLOT TO OVERWRITE DATA RECORD" : "FACILITY B-03 // SELECT CLASSIFIED LOG TO RESTORE CONTAINMENT STATE";
            }
            RefreshSlotDisplay();
        }

        public void SetVisible(bool visible, TerminalMode mode = TerminalMode.Load)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            gameObject.SetActive(visible);

            if (visible)
            {
                SetMode(mode);
                DismissConfirmOverlay();
                UITheme.TriggerSound("OnSelect");
            }
        }

        public void RefreshSlotDisplay()
        {
            UpdateSlotView(1, slot1TitleText, slot1DetailsText, slot1PurgeButton);
            UpdateSlotView(2, slot2TitleText, slot2DetailsText, slot2PurgeButton);
            UpdateSlotView(3, slot3TitleText, slot3DetailsText, slot3PurgeButton);
        }

        private void UpdateSlotView(int slot, TextMeshProUGUI title, TextMeshProUGUI details, Button purgeBtn)
        {
            bool hasData = SaveSystem.HasSlot(slot);
            if (title != null)
            {
                title.text = $"ARCHIVE // 0{slot}   " + (hasData ? "<color=#6FE3FF>[CLASSIFIED RECORD]</color>" : "<color=#4A535C>[UNALLOCATED SECTOR]</color>");
            }

            if (details != null)
            {
                if (hasData)
                {
                    var data = SaveSystem.GetSlotData(slot);
                    string ts = string.IsNullOrEmpty(data?.timestamp) ? "2026-10-01 21:42:00 UTC" : data.timestamp;
                    string sector = string.IsNullOrEmpty(data?.sectorName) ? "SECTOR B-03 CATWALK" : data.sectorName;
                    int cp = data != null ? data.currentCheckpoint : 1;
                    float hp = data != null ? data.playerHealth : 100f;
                    details.text = $"LOCATION: {sector}   CHECKPOINT: CP-0{cp}\nVITAL INTEGRITY: {hp:F0}%   TIMESTAMP: {ts}";
                }
                else
                {
                    details.text = _currentMode == TerminalMode.Save
                        ? "READY FOR ARCHIVAL RECORDING // SECTOR CORRIDOR VACANT"
                        : "NO RECOVERABLE INCIDENT LOG LOCATED IN THIS VAULT";
                }
            }

            if (purgeBtn != null)
            {
                purgeBtn.gameObject.SetActive(hasData);
            }
        }

        private void OnSlotClicked(int slot)
        {
            _pendingSlot = slot;
            _isPurgeAction = false;

            if (_currentMode == TerminalMode.Save)
            {
                bool hasData = SaveSystem.HasSlot(slot);
                if (hasData)
                {
                    // Confirm overwrite
                    ShowConfirmOverlay($"OVERWRITE CLASSIFIED ARCHIVE 0{slot}?\n<color=#C4502E>WARNING: EXISTING INCIDENT TELEMETRY WILL BE ERASED.</color>");
                }
                else
                {
                    ExecuteSave(slot);
                }
            }
            else // Load Mode
            {
                bool hasData = SaveSystem.HasSlot(slot);
                if (hasData)
                {
                    ShowConfirmOverlay($"RESTORE ARCHIVE 0{slot}?\nCURRENT ACTIVE CONTAINMENT STATE WILL BE RELOADED.");
                }
                else
                {
                    UITheme.TriggerSound("OnBack");
                }
            }
        }

        private void PromptPurge(int slot)
        {
            _pendingSlot = slot;
            _isPurgeAction = true;
            ShowConfirmOverlay($"PURGE ARCHIVE 0{slot} FILE?\n<color=#C4502E>ALL CLASSIFIED DATA IN SECTOR 0{slot} WILL BE PURGED.</color>");
        }

        private void ShowConfirmOverlay(string prompt)
        {
            if (confirmOverlay != null) confirmOverlay.SetActive(true);
            if (confirmPromptText != null) confirmPromptText.text = prompt;
            UITheme.TriggerSound("OnSelect");
        }

        private void DismissConfirmOverlay()
        {
            if (confirmOverlay != null) confirmOverlay.SetActive(false);
            _pendingSlot = -1;
            _isPurgeAction = false;
        }

        private void ExecuteConfirmedAction()
        {
            if (_pendingSlot < 1) return;

            if (_isPurgeAction)
            {
                SaveSystem.DeleteSlot(_pendingSlot);
                UITheme.TriggerSound("OnConfirm");
                DismissConfirmOverlay();
                RefreshSlotDisplay();
                return;
            }

            if (_currentMode == TerminalMode.Save)
            {
                ExecuteSave(_pendingSlot);
            }
            else
            {
                ExecuteLoad(_pendingSlot);
            }
            DismissConfirmOverlay();
        }

        private void ExecuteSave(int slot)
        {
            SaveSystem.SaveSlot(slot);
            UITheme.TriggerSound("OnConfirm");
            RefreshSlotDisplay();
            OnSlotSaved?.Invoke(slot);
            Debug.Log($"[SaveLoadMenuController] Saved to Slot {slot}.");
        }

        private void ExecuteLoad(int slot)
        {
            SaveSystem.LoadSlot(slot);
            UITheme.TriggerSound("OnTransitionStart");
            OnSlotLoaded?.Invoke(slot);
            Debug.Log($"[SaveLoadMenuController] Loaded Slot {slot}.");
        }

        private void HandleBack()
        {
            UITheme.TriggerSound("OnBack");
            OnBackPressed?.Invoke();
        }
    }
}
