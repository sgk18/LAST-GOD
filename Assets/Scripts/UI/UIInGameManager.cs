using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastGod.UI
{
    /// <summary>
    /// Master In-Game UI Coordinator for THE LAST GOD (Phase 02).
    /// Orchestrates and manages the lifecycle of:
    /// - InGameHUDController (Aeron Vitals, Surge, Ammo, Reticle)
    /// - PauseMenuController (Pause, Subsystem Links)
    /// - SettingsMenuController (Audio, Display, Controls)
    /// - SaveLoadMenuController (Archive Matrix Slots)
    /// - DialogueTerminalController (Graphic Novel Comms Subtitles)
    /// - GameOverController (Critical Failure / Containment Breach)
    /// - InventoryCodexController (Classified Loadout & Lore Archive)
    /// </summary>
    public class UIInGameManager : MonoBehaviour
    {
        public static UIInGameManager Instance { get; private set; }

        [Header("Phase 02 Controllers")]
        [SerializeField] private InGameHUDController hudController;
        [SerializeField] private PauseMenuController pauseMenu;
        [SerializeField] private SettingsMenuController settingsMenu;
        [SerializeField] private SaveLoadMenuController saveLoadMenu;
        [SerializeField] private DialogueTerminalController dialogueTerminal;
        [SerializeField] private GameOverController gameOverScreen;
        [SerializeField] private InventoryCodexController inventoryCodex;

        [Header("Audio")]
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip hoverClip;
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip confirmClip;
        [SerializeField] private AudioClip transitionClip;

        public InGameHUDController HUD => hudController;
        public PauseMenuController PauseMenu => pauseMenu;
        public SettingsMenuController SettingsMenu => settingsMenu;
        public SaveLoadMenuController SaveLoadMenu => saveLoadMenu;
        public DialogueTerminalController Dialogue => dialogueTerminal;
        public GameOverController GameOver => gameOverScreen;
        public InventoryCodexController Codex => inventoryCodex;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            UITheme.OnAudioTriggered += HandleAudioTrigger;

            if (pauseMenu != null)
            {
                pauseMenu.OnPauseStateChanged += HandlePauseStateChanged;
            }
        }

        private void Start()
        {
            // Default active state: HUD is visible; modal screens are dormant
            if (hudController != null) hudController.SetVisible(true);
            if (pauseMenu != null) pauseMenu.Resume();
            if (settingsMenu != null) settingsMenu.SetVisible(false);
            if (saveLoadMenu != null) saveLoadMenu.SetVisible(false);
            if (dialogueTerminal != null) dialogueTerminal.SetVisible(false);
            if (gameOverScreen != null) gameOverScreen.SetVisible(false);
            if (inventoryCodex != null) inventoryCodex.SetVisible(false);
        }

        private void OnDestroy()
        {
            UITheme.OnAudioTriggered -= HandleAudioTrigger;

            if (pauseMenu != null)
            {
                pauseMenu.OnPauseStateChanged -= HandlePauseStateChanged;
            }
        }

        private void HandlePauseStateChanged(bool isPaused)
        {
            // Hide HUD while paused or browsing menus
            if (hudController != null)
            {
                hudController.SetVisible(!isPaused);
            }
        }

        private void HandleAudioTrigger(string eventName)
        {
            if (uiAudioSource == null) return;

            switch (eventName)
            {
                case "OnHover":
                    if (hoverClip != null) uiAudioSource.PlayOneShot(hoverClip, 0.35f);
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
