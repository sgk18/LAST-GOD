using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using LastGod.Core;

namespace LastGod.UI
{
    /// <summary>
    /// Containment Failure / Game Over Screen for THE LAST GOD.
    /// Stark graphic novel blackout with Warning Orange / Emergency Red accents,
    /// incident telemetry, and checkpoint/archive recovery options.
    /// </summary>
    public class GameOverController : MonoBehaviour
    {
        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Menu Items")]
        [SerializeField] private UIMenuItem retryCheckpointItem;
        [SerializeField] private UIMenuItem loadArchiveItem;
        [SerializeField] private UIMenuItem quitToTitleItem;

        [Header("Save/Load Terminal Ref")]
        [SerializeField] private SaveLoadMenuController saveLoadMenu;

        [Header("Player Health Binding")]
        [SerializeField] private Health playerHealth;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            if (retryCheckpointItem != null) retryCheckpointItem.OnItemClicked += _ => RetryCheckpoint();
            if (loadArchiveItem != null) loadArchiveItem.OnItemClicked += _ => OpenArchiveRecovery();
            if (quitToTitleItem != null) quitToTitleItem.OnItemClicked += _ => QuitToTitle();

            if (saveLoadMenu != null)
            {
                saveLoadMenu.OnBackPressed += ReturnToGameOverMain;
                saveLoadMenu.OnSlotLoaded += HandleArchiveLoaded;
            }
        }

        private void Start()
        {
            SetVisible(false);

            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<Health>();
            }

            if (playerHealth != null)
            {
                playerHealth.OnDeath.AddListener(TriggerGameOver);
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath.RemoveListener(TriggerGameOver);
            }
        }

        public void TriggerGameOver()
        {
            SetVisible(true);
            UITheme.TriggerSound("OnTransitionStart");
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
                if (saveLoadMenu != null) saveLoadMenu.SetVisible(false);
            }
        }

        public void RetryCheckpoint()
        {
            Time.timeScale = 1f;
            UITheme.TriggerSound("OnTransitionStart");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OpenArchiveRecovery()
        {
            if (saveLoadMenu != null)
            {
                saveLoadMenu.SetVisible(true, SaveLoadMenuController.TerminalMode.Load);
            }
        }

        private void ReturnToGameOverMain()
        {
            if (saveLoadMenu != null) saveLoadMenu.SetVisible(false);
            UITheme.TriggerSound("OnBack");
        }

        private void HandleArchiveLoaded(int slot)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void QuitToTitle()
        {
            Time.timeScale = 1f;
            UITheme.TriggerSound("OnTransitionStart");
            SceneManager.LoadScene("MainMenu_Origin");
        }
    }
}
