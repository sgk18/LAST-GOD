using UnityEngine;
using LastGod.Core;

namespace LastGod.Player
{
    /// <summary>
    /// Renders an on-screen HUD with Aeron's health bar and responsive controls reminder.
    /// Uses Unity IMGUI (OnGUI) for zero-dependency, guaranteed rendering on any resolution.
    /// </summary>
    public class PlayerControlsHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Health playerHealth;

        [Header("Display Options")]
        [SerializeField] private bool showControlsGuide = true;

        private Texture2D _bgTexture;
        private Texture2D _hpBgTexture;
        private Texture2D _hpBarTexture;
        private Texture2D _borderTexture;
        private GUIStyle _headerStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _keyStyle;

        private void Awake()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            if (playerController == null)
                playerController = FindAnyObjectByType<PlayerController>();

            if (playerHealth == null && playerController != null)
                playerHealth = playerController.GetComponent<Health>();
            if (playerHealth == null)
                playerHealth = FindAnyObjectByType<Health>();

            // Create solid color textures for clean styling
            _bgTexture = MakeTex(2, 2, new Color(0.08f, 0.08f, 0.12f, 0.85f));
            _hpBgTexture = MakeTex(2, 2, new Color(0.2f, 0.05f, 0.05f, 0.9f));
            _hpBarTexture = MakeTex(2, 2, new Color(0.85f, 0.2f, 0.2f, 1f));
            _borderTexture = MakeTex(2, 2, new Color(0.85f, 0.72f, 0.42f, 0.9f)); // Mythic Gold
        }

        private void Update()
        {
            if (IsToggleKeyPressed())
            {
                showControlsGuide = !showControlsGuide;
            }
        }

        private bool IsToggleKeyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.hKey.wasPressedThisFrame)
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.H)) return true;
            }
            catch
            {
                // Legacy input disabled in player settings
            }
#endif
            return false;
        }

        private void OnGUI()
        {
            InitStyles();

            DrawHealthBar();

            if (showControlsGuide)
            {
                DrawControlsOverlay();
            }
        }

        private void DrawHealthBar()
        {
            int maxHp = playerHealth != null ? playerHealth.MaxHP : 10;
            int currentHp = playerHealth != null ? playerHealth.CurrentHP : 10;
            float hpPct = Mathf.Clamp01((float)currentHp / Mathf.Max(1, maxHp));

            float x = 20f;
            float y = 20f;
            float width = 220f;
            float height = 24f;

            // Border
            GUI.color = new Color(0.85f, 0.72f, 0.42f, 1f);
            GUI.DrawTexture(new Rect(x - 2, y - 2, width + 4, height + 4), _borderTexture);

            // Background
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(x, y, width, height), _hpBgTexture);

            // Health Fill
            float fillWidth = width * hpPct;
            GUI.DrawTexture(new Rect(x, y, fillWidth, height), _hpBarTexture);

            // Label
            GUI.color = Color.white;
            GUI.Label(new Rect(x + 8, y + 2, width, height), $"<b>AERON</b>   HP {currentHp} / {maxHp}", _headerStyle);
        }

        private void DrawControlsOverlay()
        {
            float screenH = Screen.height;
            float boxW = 320f;
            float boxH = 205f;
            float x = 20f;
            float y = screenH - boxH - 20f;

            // Box Border
            GUI.color = new Color(0.85f, 0.72f, 0.42f, 0.7f);
            GUI.DrawTexture(new Rect(x - 2, y - 2, boxW + 4, boxH + 4), _borderTexture);

            // Box Background
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(x, y, boxW, boxH), _bgTexture);

            // Content
            GUILayout.BeginArea(new Rect(x + 12, y + 8, boxW - 24, boxH - 16));

            GUILayout.Label("<color=#D8B468><b>CONTROLS</b></color> <size=10>(Press H to toggle)</size>", _headerStyle);
            GUILayout.Space(4);

            DrawControlRow("A / D  or  ← / →", "Move / Run");
            DrawControlRow("SPACE", "Jump & Double Jump!");
            DrawControlRow("Left Click / J / Z", "3-Hit Sword Combo");
            DrawControlRow("Left Shift / L / C", "Dodge Roll / Dash");
            DrawControlRow("W / S (on Ladder)", "Climb Up / Down");
            DrawControlRow("Right Click / K", "Shield Guard / Block");

            GUILayout.EndArea();
        }

        private void DrawControlRow(string key, string action)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<color=#64D2FF><b>{key}</b></color>", _keyStyle, GUILayout.Width(130));
            GUILayout.Label(action, _bodyStyle);
            GUILayout.EndHorizontal();
        }

        private void InitStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    richText = true,
                    normal = { textColor = Color.white }
                };
            }

            if (_bodyStyle == null)
            {
                _bodyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    richText = true,
                    normal = { textColor = new Color(0.9f, 0.9f, 0.95f) }
                };
            }

            if (_keyStyle == null)
            {
                _keyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    richText = true,
                    normal = { textColor = new Color(0.4f, 0.85f, 1f) }
                };
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void OnDestroy()
        {
            if (_bgTexture != null) Destroy(_bgTexture);
            if (_hpBgTexture != null) Destroy(_hpBgTexture);
            if (_hpBarTexture != null) Destroy(_hpBarTexture);
            if (_borderTexture != null) Destroy(_borderTexture);
        }
    }
}
