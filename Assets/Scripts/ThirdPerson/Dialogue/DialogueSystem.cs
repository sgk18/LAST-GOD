using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LastGod.ThirdPerson.Dialogue
{
    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        [Header("UI Bindings (Optional)")]
        [SerializeField] private Text speakerTextUI;
        [SerializeField] private Text subtitleTextUI;
        [SerializeField] private CanvasGroup subtitleCanvasGroup;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip typewriterTickSFX;

        [Header("Pacing")]
        [SerializeField] private float typingSpeed = 0.035f;

        private readonly Queue<DialogueLine> _queue = new();
        private bool _isDisplaying;
        private string _currentSpeaker = "";
        private string _currentDisplayedText = "";
        private float _displayTimer = 0f;
        private GUIStyle _speakerStyle;
        private GUIStyle _dialogueStyle;

        public bool IsBusy => _isDisplaying || _queue.Count > 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public void PlayDialogue(DialogueData data, Action onComplete = null)
        {
            if (data == null || data.lines == null) return;
            foreach (var line in data.lines)
            {
                _queue.Enqueue(line);
            }

            if (!_isDisplaying)
            {
                StartCoroutine(ProcessQueue(onComplete));
            }
        }

        public void QueueSubtitle(string speaker, string text, float duration = 3.0f, AudioClip clip = null)
        {
            _queue.Enqueue(new DialogueLine
            {
                speaker = speaker,
                text = text,
                duration = duration,
                voiceClip = clip
            });

            if (!_isDisplaying)
            {
                StartCoroutine(ProcessQueue(null));
            }
        }

        private IEnumerator ProcessQueue(Action onAllComplete)
        {
            _isDisplaying = true;

            while (_queue.Count > 0)
            {
                DialogueLine line = _queue.Dequeue();
                _currentSpeaker = line.speaker;
                _currentDisplayedText = "";

                if (speakerTextUI != null) speakerTextUI.text = line.speaker;
                if (subtitleCanvasGroup != null) subtitleCanvasGroup.alpha = 1f;

                if (line.voiceClip != null && audioSource != null)
                {
                    audioSource.PlayOneShot(line.voiceClip);
                }

                // Typewriter effect
                for (int i = 0; i <= line.text.Length; i++)
                {
                    _currentDisplayedText = line.text.Substring(0, i);
                    if (subtitleTextUI != null) subtitleTextUI.text = _currentDisplayedText;

                    if (typewriterTickSFX != null && audioSource != null && i % 3 == 0)
                    {
                        audioSource.PlayOneShot(typewriterTickSFX, 0.4f);
                    }

                    yield return new WaitForSecondsRealtime(typingSpeed);
                }

                // Hold duration
                yield return new WaitForSecondsRealtime(line.duration);

                // Fade out
                if (subtitleCanvasGroup != null)
                {
                    float fade = 0.3f;
                    float elapsed = 0f;
                    while (elapsed < fade)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        subtitleCanvasGroup.alpha = 1f - (elapsed / fade);
                        yield return null;
                    }
                    subtitleCanvasGroup.alpha = 0f;
                }

                _currentSpeaker = "";
                _currentDisplayedText = "";
            }

            _isDisplaying = false;
            onAllComplete?.Invoke();
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(_currentDisplayedText)) return;

            // Guaranteed high-readability cinematic subtitle rendering if UI Canvas is not explicitly bound
            if (_speakerStyle == null)
            {
                _speakerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                _speakerStyle.normal.textColor = new Color(0.35f, 0.85f, 1.0f, 0.95f);

                _dialogueStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };
                _dialogueStyle.normal.textColor = Color.white;
            }

            float boxWidth = Mathf.Min(800f, Screen.width * 0.85f);
            float boxHeight = 110f;
            float x = (Screen.width - boxWidth) * 0.5f;
            float y = Screen.height - boxHeight - 55f;

            // Semi-transparent cinematic backing
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(new Rect(x - 20, y - 10, boxWidth + 40, boxHeight + 20), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Speaker Title
            if (!string.IsNullOrEmpty(_currentSpeaker))
            {
                GUI.Label(new Rect(x, y, boxWidth, 26), _currentSpeaker.ToUpper(), _speakerStyle);
            }

            // Subtitle Line
            GUI.Label(new Rect(x + 10, y + 26, boxWidth - 20, boxHeight - 26), _currentDisplayedText, _dialogueStyle);
        }
    }
}
