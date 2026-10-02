using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LastGod.ThirdPerson.Save;

namespace LastGod.UI
{
    /// <summary>
    /// Classified Facility Settings Matrix.
    /// Manages Audio Calibration, Display & CRT Emulation, and Neural Link (Controls).
    /// Integrates seamlessly with SaveSystem and Unity PlayerSettings.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Tab Navigation Buttons")]
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button displayTabButton;
        [SerializeField] private Button controlsTabButton;
        [SerializeField] private TextMeshProUGUI audioTabText;
        [SerializeField] private TextMeshProUGUI displayTabText;
        [SerializeField] private TextMeshProUGUI controlsTabText;

        [Header("Panels")]
        [SerializeField] private GameObject audioPanel;
        [SerializeField] private GameObject displayPanel;
        [SerializeField] private GameObject controlsPanel;

        [Header("Audio Controls")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeLabel;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI sfxVolumeLabel;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeLabel;

        [Header("Display Controls")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TextMeshProUGUI fullscreenToggleLabel;
        [SerializeField] private Button resolutionButton;
        [SerializeField] private TextMeshProUGUI resolutionLabel;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private TextMeshProUGUI vsyncToggleLabel;
        [SerializeField] private Slider scanlineSlider;
        [SerializeField] private TextMeshProUGUI scanlineLabel;

        [Header("Controls (Input)")]
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private TextMeshProUGUI sensitivityLabel;
        [SerializeField] private Toggle invertYToggle;
        [SerializeField] private TextMeshProUGUI invertYLabel;

        [Header("Action Buttons")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button defaultsButton;
        [SerializeField] private Button backButton;

        public event Action OnBackPressed;

        public enum SettingsTab
        {
            Audio,
            Display,
            Controls
        }

        private SettingsTab _currentTab = SettingsTab.Audio;
        private readonly Resolution[] _supportedResolutions = new Resolution[]
        {
            new Resolution { width = 1280, height = 720 },
            new Resolution { width = 1920, height = 1080 },
            new Resolution { width = 2560, height = 1440 },
            new Resolution { width = 2560, height = 1080 }
        };
        private int _currentResIndex = 1; // Default 1080p

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            if (audioTabButton != null) audioTabButton.onClick.AddListener(() => SwitchTab(SettingsTab.Audio));
            if (displayTabButton != null) displayTabButton.onClick.AddListener(() => SwitchTab(SettingsTab.Display));
            if (controlsTabButton != null) controlsTabButton.onClick.AddListener(() => SwitchTab(SettingsTab.Controls));

            if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);
            if (resolutionButton != null) resolutionButton.onClick.AddListener(CycleResolution);
            if (vsyncToggle != null) vsyncToggle.onValueChanged.AddListener(OnVsyncToggled);
            if (scanlineSlider != null) scanlineSlider.onValueChanged.AddListener(OnScanlineChanged);

            if (sensitivitySlider != null) sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            if (invertYToggle != null) invertYToggle.onValueChanged.AddListener(OnInvertYToggled);

            if (applyButton != null) applyButton.onClick.AddListener(ApplySettings);
            if (defaultsButton != null) defaultsButton.onClick.AddListener(RestoreDefaults);
            if (backButton != null) backButton.onClick.AddListener(HandleBack);
        }

        private void Start()
        {
            LoadSettingsFromSystem();
            SwitchTab(SettingsTab.Audio);
        }

        public void SetVisible(bool visible)
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
                LoadSettingsFromSystem();
                SwitchTab(_currentTab);
                UITheme.TriggerSound("OnSelect");
            }
        }

        public void SwitchTab(SettingsTab tab)
        {
            _currentTab = tab;

            if (audioPanel != null) audioPanel.SetActive(tab == SettingsTab.Audio);
            if (displayPanel != null) displayPanel.SetActive(tab == SettingsTab.Display);
            if (controlsPanel != null) controlsPanel.SetActive(tab == SettingsTab.Controls);

            UpdateTabButtonStyles();
            UITheme.TriggerSound("OnHover");
        }

        private void UpdateTabButtonStyles()
        {
            if (audioTabText != null)
                audioTabText.color = _currentTab == SettingsTab.Audio ? UITheme.BrightCyan : UITheme.LightMetal;
            if (displayTabText != null)
                displayTabText.color = _currentTab == SettingsTab.Display ? UITheme.BrightCyan : UITheme.LightMetal;
            if (controlsTabText != null)
                controlsTabText.color = _currentTab == SettingsTab.Controls ? UITheme.BrightCyan : UITheme.LightMetal;
        }

        public void LoadSettingsFromSystem()
        {
            var data = SaveSystem.Data;

            if (masterVolumeSlider != null) masterVolumeSlider.value = data.masterVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = 0.8f;
            if (musicVolumeSlider != null) musicVolumeSlider.value = 0.75f;

            if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;
            if (vsyncToggle != null) vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
            if (scanlineSlider != null) scanlineSlider.value = 0.04f;

            if (sensitivitySlider != null) sensitivitySlider.value = data.mouseSensitivity;
            if (invertYToggle != null) invertYToggle.isOn = data.invertY;

            UpdateLabels();
        }

        private void UpdateLabels()
        {
            if (masterVolumeLabel != null && masterVolumeSlider != null)
                masterVolumeLabel.text = $"{(masterVolumeSlider.value * 100):F0}%";
            if (sfxVolumeLabel != null && sfxVolumeSlider != null)
                sfxVolumeLabel.text = $"{(sfxVolumeSlider.value * 100):F0}%";
            if (musicVolumeLabel != null && musicVolumeSlider != null)
                musicVolumeLabel.text = $"{(musicVolumeSlider.value * 100):F0}%";

            if (fullscreenToggleLabel != null && fullscreenToggle != null)
                fullscreenToggleLabel.text = fullscreenToggle.isOn ? "[ FULLSCREEN: ENABLED ]" : "[ WINDOWED MODE ]";

            if (resolutionLabel != null)
            {
                var r = _supportedResolutions[_currentResIndex];
                resolutionLabel.text = $"{r.width} × {r.height}";
            }

            if (vsyncToggleLabel != null && vsyncToggle != null)
                vsyncToggleLabel.text = vsyncToggle.isOn ? "[ V-SYNC: LOCKED 60HZ ]" : "[ V-SYNC: DISABLED ]";

            if (scanlineLabel != null && scanlineSlider != null)
                scanlineLabel.text = $"{(scanlineSlider.value * 100):F0}% CRT";

            if (sensitivityLabel != null && sensitivitySlider != null)
                sensitivityLabel.text = $"{sensitivitySlider.value:F1}x";

            if (invertYLabel != null && invertYToggle != null)
                invertYLabel.text = invertYToggle.isOn ? "[ INVERT Y: ACTIVE ]" : "[ INVERT Y: NORMAL ]";
        }

        private void OnMasterVolumeChanged(float val)
        {
            AudioListener.volume = val;
            SaveSystem.Data.masterVolume = val;
            UpdateLabels();
        }

        private void OnSfxVolumeChanged(float val)
        {
            UpdateLabels();
        }

        private void OnMusicVolumeChanged(float val)
        {
            UpdateLabels();
        }

        private void OnFullscreenToggled(bool val)
        {
            Screen.fullScreen = val;
            UpdateLabels();
        }

        private void CycleResolution()
        {
            _currentResIndex = (_currentResIndex + 1) % _supportedResolutions.Length;
            var r = _supportedResolutions[_currentResIndex];
            Screen.SetResolution(r.width, r.height, Screen.fullScreen);
            UpdateLabels();
            UITheme.TriggerSound("OnSelect");
        }

        private void OnVsyncToggled(bool val)
        {
            QualitySettings.vSyncCount = val ? 1 : 0;
            UpdateLabels();
        }

        private void OnScanlineChanged(float val)
        {
            UpdateLabels();
        }

        private void OnSensitivityChanged(float val)
        {
            SaveSystem.Data.mouseSensitivity = val;
            UpdateLabels();
        }

        private void OnInvertYToggled(bool val)
        {
            SaveSystem.Data.invertY = val;
            UpdateLabels();
        }

        public void ApplySettings()
        {
            SaveSystem.Save();
            UITheme.TriggerSound("OnConfirm");
            Debug.Log("[SettingsMenuController] Settings persisted to SaveSystem.");
        }

        public void RestoreDefaults()
        {
            SaveSystem.Data.masterVolume = 1.0f;
            SaveSystem.Data.mouseSensitivity = 2.0f;
            SaveSystem.Data.invertY = false;

            LoadSettingsFromSystem();
            ApplySettings();
        }

        private void HandleBack()
        {
            UITheme.TriggerSound("OnBack");
            OnBackPressed?.Invoke();
        }
    }
}
