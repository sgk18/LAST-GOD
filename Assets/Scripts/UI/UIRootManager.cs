using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using LastGod.ThirdPerson.Save;

namespace LastGod.UI
{
    /// <summary>
    /// Master coordinator for UI_Root.
    /// Manages switching between:
    /// - Screen A: Loading Screen
    /// - Screen B: Main Menu
    /// - Screen C: New Game Dossier
    /// - Screen E: Settings Subsystem
    /// - Screen F: Save/Load Archive Terminal
    /// </summary>
    public class UIRootManager : MonoBehaviour
    {
        [Header("Layer Views")]
        [SerializeField] private LoadingScreenController loadingScreen;
        [SerializeField] private MainMenuController mainMenu;
        [SerializeField] private NewGameDossierController newGameDossier;
        [SerializeField] private SettingsMenuController settingsMenu;
        [SerializeField] private SaveLoadMenuController saveLoadMenu;

        [Header("Target Gameplay Scene")]
        [SerializeField] private string targetGameScene = "Act1_Origin";

        [Header("Audio")]
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip hoverClip;
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip confirmClip;
        [SerializeField] private AudioClip transitionClip;

        public enum ScreenState
        {
            Loading,
            MainMenu,
            NewGameDossier,
            Settings,
            SaveLoadArchive
        }

        [SerializeField] private ScreenState initialScreen = ScreenState.MainMenu;

        private void Awake()
        {
            UITheme.OnAudioTriggered += HandleAudioTrigger;

            if (mainMenu != null)
            {
                mainMenu.OnNewGameRequested += ShowNewGameScreen;
                mainMenu.OnContinueRequested += HandleContinue;
                mainMenu.OnSettingsRequested += HandleSettings;
                mainMenu.OnQuitRequested += HandleQuit;
            }

            if (newGameDossier != null)
            {
                newGameDossier.OnBackPressed += ShowMainMenuScreen;
                newGameDossier.OnInitializationComplete += LaunchGameScene;
            }

            if (settingsMenu != null)
            {
                settingsMenu.OnBackPressed += ShowMainMenuScreen;
            }

            if (saveLoadMenu != null)
            {
                saveLoadMenu.OnBackPressed += ShowMainMenuScreen;
                saveLoadMenu.OnSlotLoaded += HandleSlotLoaded;
            }
        }

        private void Start()
        {
            SwitchScreen(initialScreen);
        }

        private void OnDestroy()
        {
            UITheme.OnAudioTriggered -= HandleAudioTrigger;

            if (mainMenu != null)
            {
                mainMenu.OnNewGameRequested -= ShowNewGameScreen;
                mainMenu.OnContinueRequested -= HandleContinue;
                mainMenu.OnSettingsRequested -= HandleSettings;
                mainMenu.OnQuitRequested -= HandleQuit;
            }

            if (newGameDossier != null)
            {
                newGameDossier.OnBackPressed -= ShowMainMenuScreen;
                newGameDossier.OnInitializationComplete -= LaunchGameScene;
            }

            if (settingsMenu != null)
            {
                settingsMenu.OnBackPressed -= ShowMainMenuScreen;
            }

            if (saveLoadMenu != null)
            {
                saveLoadMenu.OnBackPressed -= ShowMainMenuScreen;
                saveLoadMenu.OnSlotLoaded -= HandleSlotLoaded;
            }
        }

        public void SwitchScreen(ScreenState state)
        {
            if (loadingScreen != null) loadingScreen.SetVisible(state == ScreenState.Loading);
            if (mainMenu != null) mainMenu.SetVisible(state == ScreenState.MainMenu);
            if (newGameDossier != null) newGameDossier.SetVisible(state == ScreenState.NewGameDossier);
            if (settingsMenu != null) settingsMenu.SetVisible(state == ScreenState.Settings);
            if (saveLoadMenu != null) saveLoadMenu.SetVisible(state == ScreenState.SaveLoadArchive, SaveLoadMenuController.TerminalMode.Load);
        }

        public void ShowLoadingScreen() => SwitchScreen(ScreenState.Loading);
        public void ShowMainMenuScreen() => SwitchScreen(ScreenState.MainMenu);
        public void ShowNewGameScreen() => SwitchScreen(ScreenState.NewGameDossier);
        public void ShowSettingsScreen() => SwitchScreen(ScreenState.Settings);
        public void ShowArchiveScreen() => SwitchScreen(ScreenState.SaveLoadArchive);

        private void HandleContinue()
        {
            if (saveLoadMenu != null)
            {
                ShowArchiveScreen();
            }
            else
            {
                StartCoroutine(LoadSceneWithLoadingScreen(targetGameScene));
            }
        }

        private void HandleSlotLoaded(int slot)
        {
            StartCoroutine(LoadSceneWithLoadingScreen(targetGameScene));
        }

        private void HandleSettings()
        {
            ShowSettingsScreen();
        }

        private void HandleQuit()
        {
            Debug.Log("[UIRootManager] Quitting application.");
            Application.Quit();
        }

        private void LaunchGameScene()
        {
            StartCoroutine(LoadSceneWithLoadingScreen(targetGameScene));
        }

        private IEnumerator LoadSceneWithLoadingScreen(string sceneName)
        {
            SwitchScreen(ScreenState.Loading);

            if (loadingScreen != null)
            {
                loadingScreen.SetProgress(0.1f);
            }

            yield return new WaitForSecondsRealtime(0.5f);

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            if (op != null)
            {
                while (!op.isDone)
                {
                    float p = Mathf.Clamp01(op.progress / 0.9f);
                    if (loadingScreen != null) loadingScreen.SetProgress(p);
                    yield return null;
                }
            }
        }

        private void HandleAudioTrigger(string eventName)
        {
            if (uiAudioSource == null) return;

            switch (eventName)
            {
                case "OnHover":
                    if (hoverClip != null) uiAudioSource.PlayOneShot(hoverClip, 0.4f);
                    break;
                case "OnSelect":
                case "OnConfirm":
                    if (confirmClip != null) uiAudioSource.PlayOneShot(confirmClip, 0.7f);
                    else if (clickClip != null) uiAudioSource.PlayOneShot(clickClip, 0.7f);
                    break;
                case "OnTransitionStart":
                    if (transitionClip != null) uiAudioSource.PlayOneShot(transitionClip, 0.8f);
                    break;
            }
        }
    }
}
