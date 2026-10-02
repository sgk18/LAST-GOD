using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LastGod.UI
{
    /// <summary>
    /// Controller for Screen A: Loading Screen.
    /// Implements Section 13, 14 & 15:
    /// Asymmetrical brutalist layout, isolated containment chamber illustration,
    /// subtle line progression, blinking facility indicators, and cycling classified telemetry.
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        [Header("Canvas & Hierarchy")]
        [SerializeField] private CanvasGroup screenGroup;
        [SerializeField] private Image illustrationImage;
        [SerializeField] private Image cyanPulseLight;

        [Header("Progress & Telemetry")]
        [SerializeField] private RectTransform progressBarRect;
        [SerializeField] private Image progressBarFill;
        [SerializeField] private TextMeshProUGUI progressPercentText;
        [SerializeField] private TextMeshProUGUI statusTelemetryText;
        [SerializeField] private TextMeshProUGUI facilityCodeText;
        [SerializeField] private TextMeshProUGUI projectCodeText;
        [SerializeField] private Image[] indicatorBlinkers;

        private float _simulatedProgress = 0f;
        private readonly string[] _statusSequence = new string[]
        {
            "CONTAINMENT STATUS: STABLE // BIO-CHAMBER 07",
            "CONTAINMENT STATUS: MONITORING NEURAL DIVERGENCE",
            "SECTOR B-03 PRESSURE INTEGRITY: 418.2 PSI",
            "HYDRAULIC CRYO-VALVES: ENGAGED [ACTIVE STASIS]",
            "CLASSIFICATION ARCHIVE: LEVEL 05 VERIFIED",
            "INITIALIZING NEURAL LINK... READY"
        };

        private int _statusIndex = 0;
        private float _statusTimer = 0f;
        private float _pulseTimer = 0f;

        private void OnEnable()
        {
            _simulatedProgress = 0f;
            _statusIndex = 0;
            _statusTimer = 0f;
            UpdateProgressUI(0f);
            UITheme.TriggerSound("OnLoadingStart");
        }

        private void Update()
        {
            // Simulate / advance loading
            if (_simulatedProgress < 1f)
            {
                _simulatedProgress += Time.unscaledDeltaTime * 0.18f;
                if (_simulatedProgress >= 1f)
                {
                    _simulatedProgress = 1f;
                    UITheme.TriggerSound("OnLoadingComplete");
                }
                UpdateProgressUI(_simulatedProgress);
            }

            // Cycling telemetry text (Section 14, point 6)
            _statusTimer += Time.unscaledDeltaTime;
            if (_statusTimer > 2.4f)
            {
                _statusTimer = 0f;
                _statusIndex = (_statusIndex + 1) % _statusSequence.Length;
                if (statusTelemetryText != null)
                {
                    statusTelemetryText.text = _statusSequence[_statusIndex];
                }
            }

            // Subtle cyan light pulse (Section 14, point 3)
            if (cyanPulseLight != null)
            {
                _pulseTimer += Time.unscaledDeltaTime * 1.5f;
                float alpha = 0.35f + Mathf.PingPong(_pulseTimer, 0.65f);
                cyanPulseLight.color = new Color(UITheme.TechnologyCyan.r, UITheme.TechnologyCyan.g, UITheme.TechnologyCyan.b, alpha);
            }

            // Blinking indicators (Section 14, point 2)
            if (indicatorBlinkers != null)
            {
                for (int i = 0; i < indicatorBlinkers.Length; i++)
                {
                    if (indicatorBlinkers[i] != null)
                    {
                        bool blink = (Mathf.FloorToInt(Time.unscaledTime * (2f + (i * 1.3f))) % 2 == 0);
                        indicatorBlinkers[i].color = blink ? UITheme.WarningOrange : UITheme.IndustrialGrey;
                    }
                }
            }
        }

        public void SetProgress(float progress)
        {
            _simulatedProgress = Mathf.Clamp01(progress);
            UpdateProgressUI(_simulatedProgress);
        }

        private void UpdateProgressUI(float p)
        {
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = p;
            }
            if (progressPercentText != null)
            {
                progressPercentText.text = $"[ {(p * 100f):00.0}% ]";
            }
        }

        public void SetVisible(bool visible)
        {
            if (screenGroup != null)
            {
                screenGroup.alpha = visible ? 1f : 0f;
                screenGroup.blocksRaycasts = visible;
                screenGroup.interactable = visible;
            }
            gameObject.SetActive(visible);
        }
    }
}
