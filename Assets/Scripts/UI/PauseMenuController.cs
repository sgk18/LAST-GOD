using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using LastGod.ThirdPerson.Player;

namespace LastGod.UI
{
    /// <summary>
    /// Brutalist Pause Menu Controller for THE LAST GOD.
    /// Manages Time.timeScale, input suspension, cursor states, and sub-terminals (Settings, Archive Matrix).
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        public static PauseMenuController Instance { get; private set; }

        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject pausePanel;

        [Header("Menu Items")]
        [SerializeField] private UIMenuItem resumeItem;
        [SerializeField] private UIMenuItem restartItem;
        [SerializeField] private UIMenuItem settingsItem;
        [SerializeField] private UIMenuItem archiveItem;
        [SerializeField] private UIMenuItem quitItem;

        [Header("Sub-Terminals")]
        [SerializeField] private SettingsMenuController settingsMenu;
        [SerializeField] private SaveLoadMenuController saveLoadMenu;

        private bool _isPaused = false;
        public bool IsPaused => _isPaused;

        public event Action<bool> OnPauseStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            if (resumeItem != null) resumeItem.OnItemClicked += _ => Resume();
            if (restartItem != null) restartItem.OnItemClicked += _ => RestartCheckpoint();
            if (settingsItem != null) settingsItem.OnItemClicked += _ => OpenSettings();
            if (archiveItem != null) archiveItem.OnItemClicked += _ => OpenArchive();
            if (quitItem != null) quitItem.OnItemClicked += _ => QuitToTitle();

            if (settingsMenu != null) settingsMenu.OnBackPressed += ReturnToPauseMain;
            if (saveLoadMenu != null) saveLoadMenu.OnBackPressed += ReturnToPauseMain;
        }

        private void Start()
        {
            SetPauseInternal(false);
            if (settingsMenu != null) settingsMenu.SetVisible(false);
            if (saveLoadMenu != null) saveLoadMenu.SetVisible(false);
        }

        private void Update()
        {
            if (CheckEscapeKey())
            {
                // If sub-terminal is open, back out to pause main first
                if (_isPaused && ((settingsMenu != null && settingsMenu.gameObject.activeSelf) || (saveLoadMenu != null && saveLoadMenu.gameObject.activeSelf)))
                {
                    ReturnToPauseMain();
                }
                else
                {
                    TogglePause();
                }
            }
        }

        private bool CheckEscapeKey()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) return true;
            }
            catch {}
#endif
            return false;
        }

        public void TogglePause()
        {
            SetPauseInternal(!_isPaused);
        }

        public void Resume()
        {
            SetPauseInternal(false);
        }

        private void SetPauseInternal(bool paused)
        {
            _isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;

            var input = FindAnyObjectByType<ThirdPersonPlayerInput>();
            if (input != null) input.InputLocked = paused;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = paused ? 1f : 0f;
                canvasGroup.interactable = paused;
                canvasGroup.blocksRaycasts = paused;
            }
            gameObject.SetActive(paused);

            if (paused)
            {
                if (pausePanel != null) pausePanel.SetActive(true);
                if (settingsMenu != null) settingsMenu.SetVisible(false);
                if (saveLoadMenu != null) saveLoadMenu.SetVisible(false);
                UITheme.TriggerSound("OnSelect");
            }

            OnPauseStateChanged?.Invoke(paused);
        }

        public void RestartCheckpoint()
        {
            Time.timeScale = 1f;
            UITheme.TriggerSound("OnTransitionStart");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OpenSettings()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (saveLoadMenu != null) saveLoadMenu.SetVisible(false);
            if (settingsMenu != null) settingsMenu.SetVisible(true);
        }

        public void OpenArchive()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsMenu != null) settingsMenu.SetVisible(false);
            if (saveLoadMenu != null) saveLoadMenu.SetVisible(true, SaveLoadMenuController.TerminalMode.Save);
        }

        public void ReturnToPauseMain()
        {
            if (settingsMenu != null) settingsMenu.SetVisible(false);
            if (saveLoadMenu != null) saveLoadMenu.SetVisible(false);
            if (pausePanel != null) pausePanel.SetActive(true);
            UITheme.TriggerSound("OnBack");
        }

        public void QuitToTitle()
        {
            Time.timeScale = 1f;
            UITheme.TriggerSound("OnTransitionStart");
            SceneManager.LoadScene("MainMenu_Origin");
        }
    }
}
