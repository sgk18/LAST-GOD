using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace LastGod.UI
{
    /// <summary>
    /// Reusable Redaction System for THE LAST GOD classified dossiers.
    /// Implements Section 17:
    /// Charcoal/black redaction blocks, classified stamps, and subtle glitch reveals.
    /// </summary>
    public class UIRedactionBlock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Components")]
        [SerializeField] private Image redactionPlate;
        [SerializeField] private TextMeshProUGUI secretText;
        [SerializeField] private Image warningBorder;

        [Header("Content")]
        [SerializeField] private string redactedContent = "SUBJECT GENETIC ANCHOR // EXPERIMENT 07";
        [SerializeField] private bool allowHoverReveal = true;
        [SerializeField] private float revealDuration = 0.8f;

        private Coroutine _glitchRoutine;
        private bool _isRevealed;
        private string _scrambledOriginal;

        private void Awake()
        {
            if (secretText != null)
            {
                secretText.text = redactedContent;
                _scrambledOriginal = redactedContent;
            }
            SetRedactedVisual(true);
        }

        public void SetContent(string content, bool allowReveal = true)
        {
            redactedContent = content;
            _scrambledOriginal = content;
            allowHoverReveal = allowReveal;
            if (secretText != null) secretText.text = content;
            SetRedactedVisual(true);
        }

        private void SetRedactedVisual(bool redacted)
        {
            if (redactionPlate != null)
            {
                redactionPlate.color = redacted ? UITheme.NearBlack : new Color(UITheme.DarkBlueGrey.r, UITheme.DarkBlueGrey.g, UITheme.DarkBlueGrey.b, 0.4f);
            }
            if (secretText != null)
            {
                secretText.color = redacted ? Color.clear : UITheme.BrightCyan;
            }
            if (warningBorder != null)
            {
                warningBorder.color = redacted ? UITheme.IndustrialGrey : UITheme.WarningOrange;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!allowHoverReveal || _isRevealed) return;
            UITheme.TriggerSound("OnHover");
            if (_glitchRoutine != null) StopCoroutine(_glitchRoutine);
            _glitchRoutine = StartCoroutine(GlitchRevealRoutine());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!allowHoverReveal || _isRevealed) return;
            if (_glitchRoutine != null) StopCoroutine(_glitchRoutine);
            SetRedactedVisual(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            UITheme.TriggerSound("OnSelect");
            _isRevealed = !_isRevealed;
            SetRedactedVisual(!_isRevealed);
        }

        private IEnumerator GlitchRevealRoutine()
        {
            float elapsed = 0f;
            while (elapsed < revealDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                bool flicker = (Random.value > 0.4f);
                if (redactionPlate != null)
                {
                    redactionPlate.color = flicker ? new Color(0.04f, 0.06f, 0.08f, 0.85f) : UITheme.NearBlack;
                }
                if (secretText != null)
                {
                    secretText.color = flicker ? UITheme.TechnologyCyan : Color.clear;
                }
                yield return new WaitForSecondsRealtime(0.06f);
            }
            SetRedactedVisual(true);
        }
    }
}
