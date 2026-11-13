using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LastGod.Act1
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD Elements")]
        public GameObject hudPanel;
        public Image lifeBarFill;
        public Image focusBarFill;
        public Image humanityBarFill;
        public Text humanityText;
        public Text waveText;
        public GameObject brokenPromptObject;
        public Text controlsHintText;

        [Header("Dialogue Box")]
        public GameObject dialoguePanel;
        public Text speakerNameText;
        public Text dialogueBodyText;
        public Image portraitImage;
        public Sprite aeronPortrait;
        public Sprite vossPortrait;
        public Sprite voicePortrait;
        public Sprite guardPortrait;

        [Header("Notification / Banners")]
        public GameObject bannerPanel;
        public Text bannerText;

        [Header("Results Screen")]
        public GameObject resultsPanel;
        public Text resultsTitle;
        public Text resultsHumanityText;
        public Text resultsStatsText;
        public Text resultsQuoteText;

        [Header("Game Over Screen")]
        public GameObject gameOverPanel;

        private Coroutine activeDialogueRoutine;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (bannerPanel != null) bannerPanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (brokenPromptObject != null) brokenPromptObject.SetActive(false);
        }

        private void Update()
        {
            UpdateHUD();
        }

        private void UpdateHUD()
        {
            if (AeronController.Instance == null) return;

            var aeron = AeronController.Instance;

            if (lifeBarFill != null)
                lifeBarFill.fillAmount = aeron.currentLife / aeron.maxLife;

            if (focusBarFill != null)
                focusBarFill.fillAmount = aeron.currentFocus / aeron.maxFocus;

            if (humanityBarFill != null)
            {
                humanityBarFill.fillAmount = aeron.humanity / 100f;
                // Color shifts from vibrant teal to hollow slate
                Color hColor = Color.Lerp(new Color(0.25f, 0.35f, 0.45f), new Color(0.45f, 0.95f, 0.85f), aeron.humanity / 100f);
                humanityBarFill.color = hColor;
            }

            if (humanityText != null)
                humanityText.text = $"HUMANITY {Mathf.RoundToInt(aeron.humanity)}%";

            if (waveText != null && GameManager.Instance != null)
                waveText.text = $"WAVE {GameManager.Instance.currentWave}/{GameManager.Instance.maxWaves}";

            // Check if there is a broken guard nearby
            if (brokenPromptObject != null)
            {
                GuardAI broken = GuardAI.GetNearestBrokenGuard(aeron.transform.position, 2.5f);
                brokenPromptObject.SetActive(broken != null);
            }
        }

        public void ShowDialogue(string speaker, string message, float duration, string portrait = "voice")
        {
            if (dialoguePanel == null) return;

            if (activeDialogueRoutine != null)
                StopCoroutine(activeDialogueRoutine);

            activeDialogueRoutine = StartCoroutine(DialogueRoutine(speaker, message, duration, portrait));
        }

        private IEnumerator DialogueRoutine(string speaker, string message, float duration, string portrait)
        {
            dialoguePanel.SetActive(true);

            if (speakerNameText != null) speakerNameText.text = speaker;
            if (dialogueBodyText != null) dialogueBodyText.text = "";

            if (portraitImage != null)
            {
                if (portrait == "voss" && vossPortrait != null) portraitImage.sprite = vossPortrait;
                else if (portrait == "guard" && guardPortrait != null) portraitImage.sprite = guardPortrait;
                else if (portrait == "aeron" && aeronPortrait != null) portraitImage.sprite = aeronPortrait;
                else if (voicePortrait != null) portraitImage.sprite = voicePortrait;
            }

            // Typewriter effect
            for (int i = 0; i <= message.Length; i++)
            {
                if (dialogueBodyText != null) dialogueBodyText.text = message.Substring(0, i);
                yield return new WaitForSeconds(0.025f);
            }

            yield return new WaitForSeconds(duration);

            dialoguePanel.SetActive(false);
            activeDialogueRoutine = null;
        }

        public void ShowWaveBanner(string text)
        {
            StartCoroutine(BannerRoutine(text));
        }

        private IEnumerator BannerRoutine(string text)
        {
            if (bannerPanel == null) yield break;
            if (bannerText != null) bannerText.text = text;
            bannerPanel.SetActive(true);
            yield return new WaitForSeconds(2.0f);
            bannerPanel.SetActive(false);
        }

        public void ShowControlsHint(bool show)
        {
            if (controlsHintText != null)
                controlsHintText.gameObject.SetActive(show);
        }

        public void ShowResults(float humanity, int taken, int cut, int reflects, int echoes, int hits)
        {
            if (hudPanel != null) hudPanel.SetActive(false);
            if (resultsPanel == null) return;

            resultsPanel.SetActive(true);

            string rank = (humanity >= 70f) ? "STILL HUMAN." : (humanity >= 35f) ? "FRAYING." : "HOLLOW.";

            if (resultsHumanityText != null)
                resultsHumanityText.text = $"HUMANITY: {Mathf.RoundToInt(humanity)}%   [{rank}]";

            if (resultsStatsText != null)
            {
                resultsStatsText.text = $"TAKEN: {taken}    |    CUT DOWN: {cut}\n" +
                                        $"BULLETS REFLECTED: {reflects}    |    ECHO DODGES: {echoes}\n" +
                                        $"HITS SUSTAINED: {hits}";
            }

            if (resultsQuoteText != null)
            {
                resultsQuoteText.text = "\"EVERY ONE OF THEM WASN'T A TARGET. THEY WERE A WARNING.\"\n\nPRESS R TO RISE AGAIN";
            }
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }
    }
}

