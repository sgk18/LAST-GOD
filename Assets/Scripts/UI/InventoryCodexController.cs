using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LastGod.UI
{
    /// <summary>
    /// Classified Equipment & Specimen Lore Codex for THE LAST GOD.
    /// Manages Aeron's recovered tactical loadout and classified laboratory logs.
    /// Toggleable via [I] or [TAB].
    /// </summary>
    public class InventoryCodexController : MonoBehaviour
    {
        public static InventoryCodexController Instance { get; private set; }

        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Equipment Slot Buttons")]
        [SerializeField] private Button weaponSlotButton;
        [SerializeField] private Button surgeSlotButton;
        [SerializeField] private Button collarSlotButton;

        [Header("Item Detail Elements")]
        [SerializeField] private TextMeshProUGUI itemTitleText;
        [SerializeField] private TextMeshProUGUI itemSerialText;
        [SerializeField] private TextMeshProUGUI itemSpecsText;
        [SerializeField] private TextMeshProUGUI itemLoreText;
        [SerializeField] private Button closeButton;

        private bool _isOpen = false;
        public bool IsOpen => _isOpen;

        private int _selectedItemIndex = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            if (weaponSlotButton != null) weaponSlotButton.onClick.AddListener(() => SelectItem(0));
            if (surgeSlotButton != null) surgeSlotButton.onClick.AddListener(() => SelectItem(1));
            if (collarSlotButton != null) collarSlotButton.onClick.AddListener(() => SelectItem(2));

            if (closeButton != null) closeButton.onClick.AddListener(CloseCodex);
        }

        private void Start()
        {
            SetVisible(false);
            SelectItem(0);
        }

        private void Update()
        {
            bool toggleKey = false;
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && (UnityEngine.InputSystem.Keyboard.current.iKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame))
                toggleKey = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.I) || UnityEngine.Input.GetKeyDown(KeyCode.Tab))
                    toggleKey = true;
            }
            catch {}
#endif
            if (toggleKey)
            {
                ToggleCodex();
            }
        }

        public void ToggleCodex()
        {
            SetVisible(!_isOpen);
        }

        public void CloseCodex()
        {
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            _isOpen = visible;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            gameObject.SetActive(visible);

            if (visible)
            {
                UITheme.TriggerSound("OnSelect");
                SelectItem(_selectedItemIndex);
            }
            else
            {
                UITheme.TriggerSound("OnBack");
            }
        }

        public void SelectItem(int index)
        {
            _selectedItemIndex = index;
            UITheme.TriggerSound("OnHover");

            switch (index)
            {
                case 0: // Security Pistol
                    if (itemTitleText != null) itemTitleText.text = "SEC-P9 // STASIS INTERCEPTOR";
                    if (itemSerialText != null) itemSerialText.text = "SERIAL: #09-441-K // CLEARANCE LEVEL 04";
                    if (itemSpecsText != null) itemSpecsText.text = "CALIBER: 9×19mm STASIS HOLLOW-POINT\nMAGAZINE: 6 ROUNDS CAPACITY\nVELOCITY: 380 M/S SUB-SONIC\nFIRE-RATE: SEMI-AUTOMATIC INTERCEPT";
                    if (itemLoreText != null) itemLoreText.text = "Standard issue facility sidearm. Modified with pressurized liquid stasis reservoirs to neutralize biological specimens exhibiting divine divergence.";
                    break;

                case 1: // Ascension Surge
                    if (itemTitleText != null) itemTitleText.text = "ASCENSION CATALYST // DIVINE CORE";
                    if (itemSerialText != null) itemSerialText.text = "BIO-CLASSIFICATION: ORGANIC SINGULARITY // PROJECT A-07";
                    if (itemSpecsText != null) itemSpecsText.text = "ENERGY YIELD: 4.8 GIGAWATTS\nDURATION: 4.0 SECONDS\nDAMAGE MULTIPLIER: 2.0x\nSPEED ENHANCEMENT: +90% MOTOR REFLEX";
                    if (itemLoreText != null) itemLoreText.text = "Extracted divine marrow surgically grafted into Aeron's spine. Triggering overdrive causes instantaneous neural dilation and overwhelming physical velocity.";
                    break;

                case 2: // Severed Collar
                    if (itemTitleText != null) itemTitleText.text = "STASIS RESTRAINT COLLAR // [SEVERED]";
                    if (itemSerialText != null) itemSerialText.text = "RESTRICTION: CLASS-A BIO-NEUTRALIZER";
                    if (itemSpecsText != null) itemSpecsText.text = "VOLTAGE: 50,000 VOLTS PULSED\nSTATUS: SHATTERED / INOPERATIVE\nORIGIN: STASIS VAULT 01";
                    if (itemLoreText != null) itemLoreText.text = "The collar held Subject Aeron in an induced coma for four hundred cycles. A violent structural breach shattered its locking pins, releasing the subject.";
                    break;
            }
        }
    }
}
