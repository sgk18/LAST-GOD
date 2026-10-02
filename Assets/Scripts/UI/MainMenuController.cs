using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LastGod.UI
{
    /// <summary>
    /// Controller for Screen B: Main Menu.
    /// Implements Section 06, 07, 08, 09, 11, 12 & 26:
    /// Architectural asymmetrical layout (Left 45% Title & Dossier codes, Right 30% Brutalist Menu),
    /// negative space, laboratory illustration with subtle cyan containment light, microdetails, and audio events.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Items")]
        [SerializeField] private UIMenuItem itemNewGame;
        [SerializeField] private UIMenuItem itemContinue;
        [SerializeField] private UIMenuItem itemSettings;
        [SerializeField] private UIMenuItem itemQuit;

        [Header("Canvas & Visuals")]
        [SerializeField] private CanvasGroup menuCanvasGroup;
        [SerializeField] private Image laboratoryIllustration;
        [SerializeField] private Image cyanContainmentDot;
        [SerializeField] private Image emblemWatermark;

        [Header("Microdetails & Telemetry")]
        [SerializeField] private TextMeshProUGUI facilityCodeText;
        [SerializeField] private TextMeshProUGUI securityLevelText;
        [SerializeField] private TextMeshProUGUI systemClockText;
        [SerializeField] private TextMeshProUGUI containmentIntegrityText;

        public event Action OnNewGameRequested;
        public event Action OnContinueRequested;
        public event Action OnSettingsRequested;
        public event Action OnQuitRequested;

        private float _cyanPulse;

        private void Awake()
        {
            if (itemNewGame != null)
            {
                itemNewGame.Setup("01", "NEW GAME", "SYSTEM INITIALIZATION // SPECIMEN DOSSIER");
                itemNewGame.OnItemClicked += HandleNewGameClicked;
            }

            if (itemContinue != null)
            {
                itemContinue.Setup("02", "CONTINUE", "RESUME CONTAINMENT LOG // CHECKPOINT ALPHA");
                itemContinue.OnItemClicked += HandleContinueClicked;
            }

            if (itemSettings != null)
            {
                itemSettings.Setup("03", "SETTINGS", "FACILITY TERMINAL CONFIGURATION");
                itemSettings.OnItemClicked += HandleSettingsClicked;
            }

            if (itemQuit != null)
            {
                itemQuit.Setup("04", "QUIT", "TERMINATE SECURE SESSION");
                itemQuit.OnItemClicked += HandleQuitClicked;
            }
        }

        private void OnDestroy()
        {
            if (itemNewGame != null) itemNewGame.OnItemClicked -= HandleNewGameClicked;
            if (itemContinue != null) itemContinue.OnItemClicked -= HandleContinueClicked;
            if (itemSettings != null) itemSettings.OnItemClicked -= HandleSettingsClicked;
            if (itemQuit != null) itemQuit.OnItemClicked -= HandleQuitClicked;
        }

        private void Update()
        {
            // Update microdetail clock
            if (systemClockText != null)
            {
                var now = DateTime.Now;
                systemClockText.text = $"UTC {now:HH:mm:ss.ff}";
            }

            // Subtle cyan light pulse at the distant containment pod (Section 09)
            if (cyanContainmentDot != null)
            {
                _cyanPulse += Time.unscaledDeltaTime * 1.8f;
                float alpha = 0.5f + Mathf.PingPong(_cyanPulse, 0.5f);
                cyanContainmentDot.color = new Color(UITheme.TechnologyCyan.r, UITheme.TechnologyCyan.g, UITheme.TechnologyCyan.b, alpha);
            }
        }

        private void HandleNewGameClicked(UIMenuItem item)
        {
            OnNewGameRequested?.Invoke();
        }

        private void HandleContinueClicked(UIMenuItem item)
        {
            OnContinueRequested?.Invoke();
        }

        private void HandleSettingsClicked(UIMenuItem item)
        {
            OnSettingsRequested?.Invoke();
        }

        private void HandleQuitClicked(UIMenuItem item)
        {
            OnQuitRequested?.Invoke();
        }

        public void SetVisible(bool visible)
        {
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = visible ? 1f : 0f;
                menuCanvasGroup.blocksRaycasts = visible;
                menuCanvasGroup.interactable = visible;
            }
            gameObject.SetActive(visible);
        }
    }
}
