using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LastGod.ThirdPerson.Dialogue;

namespace LastGod.UI
{
    /// <summary>
    /// Classified Facility Transmission & Dialogue Terminal for THE LAST GOD.
    /// Combines Graphic Novel speaker silhouettes with brutalist ink panel framing,
    /// animated typewriter text reveals, frequency classification tags, and audio hooks.
    /// </summary>
    public class DialogueTerminalController : MonoBehaviour
    {
        public static DialogueTerminalController Instance { get; private set; }

        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Speaker Identity UI")]
        [SerializeField] private Image speakerPortraitImage;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI frequencyTagText;

        [Header("Dialogue Content UI")]
        [SerializeField] private TextMeshProUGUI dialogueBodyText;
        [SerializeField] private GameObject advancePromptObject;
        [SerializeField] private TextMeshProUGUI advancePromptText;

        [Header("Speaker Silhouettes")]
        [SerializeField] private Sprite scientistSprite;
        [SerializeField] private Sprite overwatchSprite;

        [Header("Pacing")]
        [SerializeField] private float typingSpeed = 0.028f;

        private readonly Queue<DialogueLine> _queue = new();
        private bool _isDisplaying = false;
        private bool _skipCurrentLine = false;
        private Coroutine _typewriterCoroutine;

        public bool IsActive => _isDisplaying || _queue.Count > 0;
        public event Action OnDialogueCompleted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            SetVisible(false);
        }

        private void Update()
        {
            if (!_isDisplaying) return;

            // Space or Left-Click to advance / skip typewriter
            bool advancePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
                advancePressed = true;
            if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
                advancePressed = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Space) || UnityEngine.Input.GetMouseButtonDown(0))
                    advancePressed = true;
            }
            catch {}
#endif
            if (advancePressed)
            {
                _skipCurrentLine = true;
            }
        }

        public void PlayDialogue(DialogueData data, Action onComplete = null)
        {
            if (data == null || data.lines == null) return;
            foreach (var l in data.lines)
            {
                _queue.Enqueue(l);
            }

            if (!_isDisplaying)
            {
                StartCoroutine(ProcessDialogueQueue(onComplete));
            }
        }

        public void QueueMessage(string speaker, string message, float duration = 3.5f, string freq = "COMMS // 104.70 MHz")
        {
            _queue.Enqueue(new DialogueLine
            {
                speaker = speaker,
                text = message,
                duration = duration
            });

            if (!_isDisplaying)
            {
                StartCoroutine(ProcessDialogueQueue(null));
            }
        }

        private IEnumerator ProcessDialogueQueue(Action onComplete)
        {
            _isDisplaying = true;
            SetVisible(true);

            while (_queue.Count > 0)
            {
                DialogueLine line = _queue.Dequeue();
                _skipCurrentLine = false;

                UpdateSpeakerDetails(line.speaker);

                if (advancePromptObject != null) advancePromptObject.SetActive(false);
                if (dialogueBodyText != null) dialogueBodyText.text = "";

                // Typewriter effect
                string fullText = line.text;
                for (int i = 0; i <= fullText.Length; i++)
                {
                    if (_skipCurrentLine)
                    {
                        if (dialogueBodyText != null) dialogueBodyText.text = fullText;
                        break;
                    }

                    if (dialogueBodyText != null)
                        dialogueBodyText.text = fullText.Substring(0, i);

                    if (i % 2 == 0) UITheme.TriggerSound("OnHover");
                    yield return new WaitForSecondsRealtime(typingSpeed);
                }

                // Show advance prompt
                if (advancePromptObject != null) advancePromptObject.SetActive(true);
                _skipCurrentLine = false;

                // Wait for player advance or line duration timeout
                float elapsed = 0f;
                while (elapsed < line.duration && !_skipCurrentLine)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }

                UITheme.TriggerSound("OnSelect");
                yield return new WaitForSecondsRealtime(0.15f);
            }

            SetVisible(false);
            _isDisplaying = false;
            onComplete?.Invoke();
            OnDialogueCompleted?.Invoke();
        }

        private void UpdateSpeakerDetails(string speaker)
        {
            if (speakerNameText != null)
                speakerNameText.text = speaker.ToUpper();

            string spkUpper = speaker.ToUpper();
            if (frequencyTagText != null)
            {
                if (spkUpper.Contains("MALKHOV") || spkUpper.Contains("DIRECTOR"))
                    frequencyTagText.text = "TRANSMISSION // DIRECTORIAL OVERRIDE [SEC-01]";
                else if (spkUpper.Contains("GUARD") || spkUpper.Contains("OVERWATCH"))
                    frequencyTagText.text = "TRANSMISSION // SECURITY FREQUENCY [104.70 MHz]";
                else
                    frequencyTagText.text = "TRANSMISSION // UNKNOWN CARRIER FREQUENCY";
            }

            if (speakerPortraitImage != null)
            {
                if (spkUpper.Contains("MALKHOV") || spkUpper.Contains("DIRECTOR") || spkUpper.Contains("SCIENTIST"))
                {
                    if (scientistSprite != null) speakerPortraitImage.sprite = scientistSprite;
                }
                else
                {
                    if (overwatchSprite != null) speakerPortraitImage.sprite = overwatchSprite;
                }
            }
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
        }
    }
}
