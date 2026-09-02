using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LastGod.Core
{
    /// <summary>
    /// UI Manager for Act 1 Scene 1 cutscene text and screen transitions
    /// (fade in, fade out, typewriter text, red alarm flash).
    /// Uses TextMeshProUGUI for crisp GBA-pixel-style text rendering.
    /// </summary>
    public class CutsceneUIController : MonoBehaviour
    {
        public static CutsceneUIController Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private Image blackOverlay;
        [SerializeField] private Image redAlarmOverlay;
        [SerializeField] private TextMeshProUGUI dialogueText;

        [Header("Settings")]
        [SerializeField] private float typewriterSpeed = 0.05f;

        private Coroutine _textCoroutine;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            if (blackOverlay != null)
            {
                Color c = blackOverlay.color;
                c.a = 1f; // Start pitch black
                blackOverlay.color = c;
            }

            if (redAlarmOverlay != null)
            {
                Color c = redAlarmOverlay.color;
                c.a = 0f;
                redAlarmOverlay.color = c;
            }

            if (dialogueText != null)
            {
                dialogueText.text = "";
            }
        }

        /// <summary>Typewriter-display a single dialogue line and hold for holdTime seconds.</summary>
        public IEnumerator ShowText(string line, float holdTime = 1.5f)
        {
            if (dialogueText == null) yield break;

            dialogueText.text = "";
            dialogueText.color = Color.white;

            foreach (char letter in line.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typewriterSpeed);
            }

            yield return new WaitForSeconds(holdTime);
        }

        /// <summary>Fade dialogue text to transparent and clear it.</summary>
        public IEnumerator ClearText(float fadeDuration = 0.5f)
        {
            if (dialogueText == null) yield break;

            float elapsed = 0f;
            Color initial = dialogueText.color;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(initial.a, 0f, elapsed / fadeDuration);
                dialogueText.color = new Color(initial.r, initial.g, initial.b, a);
                yield return null;
            }
            dialogueText.text = "";
        }

        /// <summary>Fade the fullscreen black overlay to a target alpha over duration seconds.</summary>
        public IEnumerator FadeBlackOverlay(float targetAlpha, float duration)
        {
            if (blackOverlay == null) yield break;

            float elapsed = 0f;
            Color c = blackOverlay.color;
            float startAlpha = c.a;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                blackOverlay.color = c;
                yield return null;
            }

            c.a = targetAlpha;
            blackOverlay.color = c;
        }

        /// <summary>Start the red alarm overlay flickering for flickerDuration seconds.</summary>
        public void StartRedAlarmFlicker(float flickerDuration = 5f)
        {
            StartCoroutine(RedAlarmRoutine(flickerDuration));
        }

        private IEnumerator RedAlarmRoutine(float duration)
        {
            if (redAlarmOverlay == null) yield break;

            float elapsed = 0f;
            Color c = Color.red;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                // Oscillating red flash
                c.a = (Mathf.Sin(elapsed * 10f) + 1f) * 0.25f;
                redAlarmOverlay.color = c;
                yield return null;
            }

            c.a = 0f;
            redAlarmOverlay.color = c;
        }
    }
}
