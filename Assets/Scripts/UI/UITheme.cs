using UnityEngine;

namespace LastGod.UI
{
    /// <summary>
    /// Master design token repository for THE LAST GOD UI Visual Identity Phase 01.
    /// Dark Ink | Industrial Brutalism | Graphic Novel | Scientific Documentation.
    /// </summary>
    public static class UITheme
    {
        // 04 — MASTER PALETTE (Exact Hex Tokens)
        public static readonly Color NearBlack       = new Color(0x08 / 255f, 0x0B / 255f, 0x0F / 255f, 1f); // #080B0F
        public static readonly Color DeepCharcoal    = new Color(0x11 / 255f, 0x16 / 255f, 0x1C / 255f, 1f); // #11161C
        public static readonly Color DarkBlueGrey   = new Color(0x1B / 255f, 0x24 / 255f, 0x30 / 255f, 1f); // #1B2430
        public static readonly Color IndustrialGrey  = new Color(0x30 / 255f, 0x38 / 255f, 0x41 / 255f, 1f); // #303841
        public static readonly Color LightMetal      = new Color(0x4A / 255f, 0x53 / 255f, 0x5C / 255f, 1f); // #4A535C
        public static readonly Color TechnologyCyan  = new Color(0x6F / 255f, 0xE3 / 255f, 0xFF / 255f, 1f); // #6FE3FF
        public static readonly Color BrightCyan      = new Color(0xCF / 255f, 0xF4 / 255f, 0xFF / 255f, 1f); // #CFF4FF
        public static readonly Color DarkCyan        = new Color(0x24 / 255f, 0x5B / 255f, 0x70 / 255f, 1f); // #245B70
        public static readonly Color WarningOrange   = new Color(0xC4 / 255f, 0x50 / 255f, 0x2E / 255f, 1f); // #C4502E
        public static readonly Color IndustrialYellow = new Color(0xB3 / 255f, 0x9A / 255f, 0x45 / 255f, 1f); // #B39A45

        // Editorial Accent Variants
        public static readonly Color RedactionBlack  = new Color(0x04 / 255f, 0x06 / 255f, 0x08 / 255f, 1f);
        public static readonly Color RedactionHover  = new Color(0x1B / 255f, 0x24 / 255f, 0x30 / 255f, 0.95f);
        public static readonly Color TextMuted       = new Color(0x8A / 255f, 0x96 / 255f, 0xA0 / 255f, 1f);
        public static readonly Color TextBright      = new Color(0xEE / 255f, 0xF2 / 255f, 0xF6 / 255f, 1f);
        public static readonly Color TextClassified  = new Color(0xC4 / 255f, 0x50 / 255f, 0x2E / 255f, 0.9f);

        // Sound Hooks
        public delegate void UIAudioEvent(string eventName);
        public static event UIAudioEvent OnAudioTriggered;

        public static void TriggerSound(string eventName)
        {
            OnAudioTriggered?.Invoke(eventName);
        }
    }
}
