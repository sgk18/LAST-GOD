using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace LastGod.UI
{
    /// <summary>
    /// Brutalist interactive menu entry.
    /// Implements Section 11 & 12:
    /// Thin cyan rule reveal, number color shift, typographic offset,
    /// technical sub-label expansion, and mechanical sound hooks.
    /// </summary>
    public class UIMenuItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [Header("Typography")]
        [SerializeField] private TextMeshProUGUI numberText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI sublabelText;

        [Header("Brutalist Indicators")]
        [SerializeField] private Image cyanRule;
        [SerializeField] private Image indicatorTick;
        [SerializeField] private Image backgroundHighlight;

        [Header("Data")]
        [SerializeField] private string itemNumber = "01";
        [SerializeField] private string itemTitle = "NEW GAME";
        [SerializeField] private string itemSublabel = "SYSTEM INITIALIZATION";

        [Header("State Colors")]
        [SerializeField] private Color normalTextColor = new Color(0.85f, 0.88f, 0.92f, 1f);
        [SerializeField] private Color hoverTextColor = new Color(0.81f, 0.96f, 1.0f, 1f); // Bright Cyan accent
        [SerializeField] private Color normalNumberColor = new Color(0.29f, 0.33f, 0.36f, 1f); // Light metal
        [SerializeField] private Color hoverNumberColor = new Color(0.77f, 0.31f, 0.18f, 1f); // Warning Orange
        [SerializeField] private Color cyanAccent = new Color(0.43f, 0.89f, 1.0f, 1f); // Technology Cyan

        public event Action<UIMenuItem> OnItemClicked;

        private bool _isHovered;
        private float _hoverTransition;
        private RectTransform _titleRect;
        private Vector2 _titleOriginalPos;

        private void Awake()
        {
            if (titleText != null)
            {
                _titleRect = titleText.rectTransform;
                _titleOriginalPos = _titleRect.anchoredPosition;
            }
            ApplyContent();
            SetHoverVisuals(0f);
        }

        public void Setup(string number, string title, string sublabel)
        {
            itemNumber = number;
            itemTitle = title;
            itemSublabel = sublabel;
            ApplyContent();
        }

        private void ApplyContent()
        {
            if (numberText != null) numberText.text = itemNumber;
            if (titleText != null) titleText.text = itemTitle;
            if (sublabelText != null) sublabelText.text = itemSublabel;
        }

        private void Update()
        {
            float target = _isHovered ? 1f : 0f;
            if (!Mathf.Approximately(_hoverTransition, target))
            {
                _hoverTransition = Mathf.MoveTowards(_hoverTransition, target, Time.unscaledDeltaTime * 12f);
                SetHoverVisuals(_hoverTransition);
            }
        }

        public void SetHovered(bool hovered)
        {
            _isHovered = hovered;
            _hoverTransition = hovered ? 1f : 0f;
            SetHoverVisuals(_hoverTransition);
        }

        private void SetHoverVisuals(float t)
        {
            if (titleText != null)
            {
                titleText.color = Color.Lerp(normalTextColor, hoverTextColor, t);
                if (_titleRect != null)
                {
                    _titleRect.anchoredPosition = new Vector2(
                        _titleOriginalPos.x + (t * 8f),
                        _titleOriginalPos.y
                    );
                }
            }

            if (numberText != null)
            {
                numberText.color = Color.Lerp(normalNumberColor, hoverNumberColor, t);
            }

            if (cyanRule != null)
            {
                cyanRule.color = new Color(cyanAccent.r, cyanAccent.g, cyanAccent.b, t * 0.95f);
                RectTransform ruleRect = cyanRule.rectTransform;
                ruleRect.localScale = new Vector3(t, 1f, 1f);
            }

            if (indicatorTick != null)
            {
                indicatorTick.color = new Color(cyanAccent.r, cyanAccent.g, cyanAccent.b, t);
            }

            if (sublabelText != null)
            {
                Color subColor = Color.Lerp(new Color(0.4f, 0.45f, 0.5f, 0f), cyanAccent, t);
                sublabelText.color = subColor;
            }

            if (backgroundHighlight != null)
            {
                backgroundHighlight.color = new Color(
                    UITheme.DarkBlueGrey.r,
                    UITheme.DarkBlueGrey.g,
                    UITheme.DarkBlueGrey.b,
                    t * 0.25f
                );
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            UITheme.TriggerSound("OnHover");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            _isHovered = true;
            UITheme.TriggerSound("OnSelect");
        }

        public void OnDeselect(BaseEventData eventData)
        {
            _isHovered = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ExecuteClick();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            ExecuteClick();
        }

        private void ExecuteClick()
        {
            UITheme.TriggerSound("OnConfirm");
            OnItemClicked?.Invoke(this);
        }
    }
}
