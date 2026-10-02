using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LastGod.UI
{
    /// <summary>
    /// Controller for Screen C: New Game Screen (Classified Narrative Dossier).
    /// Implements Section 16, 17 & 18:
    /// Narrative dossier layout, specimen biometric telemetry, interactive redactions,
    /// and cinematic sequence transitioning into Act 1.
    /// </summary>
    public class NewGameDossierController : MonoBehaviour
    {
        [Header("Canvas & Visuals")]
        [SerializeField] private CanvasGroup dossierCanvasGroup;
        [SerializeField] private Image dossierBackgroundImage;
        [SerializeField] private Image specimenSilhouetteImage;

        [Header("Redactions")]
        [SerializeField] private UIRedactionBlock redactionOrigin;
        [SerializeField] private UIRedactionBlock redactionAge;
        [SerializeField] private UIRedactionBlock redactionDirector;
        [SerializeField] private UIRedactionBlock redactionObjective;

        [Header("Buttons & Interactions")]
        [SerializeField] private Button initializeButton;
        [SerializeField] private TextMeshProUGUI initializeButtonText;
        [SerializeField] private Button backButton;

        [Header("Transition Elements (Section 18)")]
        [SerializeField] private CanvasGroup transitionOverlay;
        [SerializeField] private RectTransform scanLineSweep;
        [SerializeField] private TextMeshProUGUI transitionTerminalText;

        public event Action OnInitializationComplete;
        public event Action OnBackPressed;

        private bool _isTransitioning;

        private void Awake()
        {
            if (initializeButton != null)
            {
                initializeButton.onClick.AddListener(HandleInitializeClicked);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(HandleBackClicked);
            }

            if (redactionOrigin != null)
            {
                redactionOrigin.SetContent("FORBIDDEN DIVINITY // SECTOR ZERO");
            }
            if (redactionAge != null)
            {
                redactionAge.SetContent("CYCLE 419 // UNMEASURABLE BIOMASS");
            }
            if (redactionDirector != null)
            {
                redactionDirector.SetContent("DR. V. MALKHOV [TERMINATED]");
            }
            if (redactionObjective != null)
            {
                redactionObjective.SetContent("CELLULAR EXTRACTION & DIVINE DEICIDE");
            }

            if (transitionOverlay != null)
            {
                transitionOverlay.alpha = 0f;
                transitionOverlay.blocksRaycasts = false;
            }
        }

        private void OnDestroy()
        {
            if (initializeButton != null) initializeButton.onClick.RemoveListener(HandleInitializeClicked);
            if (backButton != null) backButton.onClick.RemoveListener(HandleBackClicked);
        }

        public void SetVisible(bool visible)
        {
            if (dossierCanvasGroup != null)
            {
                dossierCanvasGroup.alpha = visible ? 1f : 0f;
                dossierCanvasGroup.blocksRaycasts = visible;
                dossierCanvasGroup.interactable = visible;
            }
            gameObject.SetActive(visible);
            _isTransitioning = false;
        }

        private void HandleInitializeClicked()
        {
            if (_isTransitioning) return;
            UITheme.TriggerSound("OnConfirm");
            StartCoroutine(ExecuteTransitionSequence());
        }

        private void HandleBackClicked()
        {
            if (_isTransitioning) return;
            UITheme.TriggerSound("OnBack");
            OnBackPressed?.Invoke();
        }

        /// <summary>
        /// Section 18 Sequence:
        /// PROJECT A-07 -> screen darkens -> technical line appears -> small cyan indicator -> laboratory image -> gameplay begins
        /// </summary>
        private IEnumerator ExecuteTransitionSequence()
        {
            _isTransitioning = true;
            UITheme.TriggerSound("OnTransitionStart");

            if (transitionOverlay != null)
            {
                transitionOverlay.gameObject.SetActive(true);
                transitionOverlay.blocksRaycasts = true;
            }

            // Phase 1: Screen darkens with classified text
            float elapsed = 0f;
            while (elapsed < 0.6f)
            {
                elapsed += Time.unscaledDeltaTime;
                if (transitionOverlay != null)
                {
                    transitionOverlay.alpha = Mathf.Clamp01(elapsed / 0.6f);
                }
                yield return null;
            }

            if (transitionTerminalText != null)
            {
                transitionTerminalText.text = "PROJECT A-07 // AUTHORIZATION GRANTED\nBREAKING CONTAINMENT SEALS...";
            }

            // Phase 2: Technical scan line sweep
            if (scanLineSweep != null)
            {
                scanLineSweep.gameObject.SetActive(true);
                float lineElapsed = 0f;
                while (lineElapsed < 0.8f)
                {
                    lineElapsed += Time.unscaledDeltaTime;
                    float normalized = lineElapsed / 0.8f;
                    scanLineSweep.anchorMin = new Vector2(0f, 1f - normalized);
                    scanLineSweep.anchorMax = new Vector2(1f, 1.02f - normalized);
                    yield return null;
                }
                scanLineSweep.gameObject.SetActive(false);
            }

            if (transitionTerminalText != null)
            {
                transitionTerminalText.text = "CONTAINMENT BREACH INITIATED.\nSUBJECT WAKING...";
            }

            yield return new WaitForSecondsRealtime(0.6f);

            UITheme.TriggerSound("OnTransitionComplete");
            OnInitializationComplete?.Invoke();
        }
    }
}
