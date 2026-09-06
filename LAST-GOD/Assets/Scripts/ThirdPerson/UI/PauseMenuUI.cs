using UnityEngine;
using Input = UnityEngine.Input;
using UnityEngine.SceneManagement;
using LastGod.ThirdPerson.Save;
using LastGod.ThirdPerson.Player;

namespace LastGod.ThirdPerson.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        public static PauseMenuUI Instance { get; private set; }

        private bool _isPaused;
        private bool _showSettings;
        private GUIStyle _titleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _labelStyle;

        public bool IsPaused => _isPaused;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            SetPaused(!_isPaused);
        }

        public void SetPaused(bool paused)
        {
            _isPaused = paused;
            Time.timeScale = _isPaused ? 0f : 1f;
            Cursor.lockState = _isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = _isPaused;

            var input = FindAnyObjectByType<ThirdPersonPlayerInput>();
            if (input != null) input.InputLocked = _isPaused;
        }

        private void OnGUI()
        {
            if (!_isPaused) return;

            InitStyles();

            // Dark semi-transparent backdrop
            GUI.color = new Color(0.02f, 0.04f, 0.08f, 0.88f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float panelWidth = 360f;
            float panelHeight = _showSettings ? 460f : 340f;
            float x = (Screen.width - panelWidth) * 0.5f;
            float y = (Screen.height - panelHeight) * 0.5f;

            GUILayout.BeginArea(new Rect(x, y, panelWidth, panelHeight));

            if (!_showSettings)
            {
                GUILayout.Label("THE LAST GOD", _titleStyle);
                GUILayout.Label("SYSTEM PAUSED // PROTOCOL ORIGIN", _labelStyle);
                GUILayout.Space(25);

                if (GUILayout.Button("RESUME", _buttonStyle, GUILayout.Height(44)))
                {
                    SetPaused(false);
                }
                GUILayout.Space(10);

                if (GUILayout.Button("RESTART CHECKPOINT", _buttonStyle, GUILayout.Height(44)))
                {
                    Time.timeScale = 1.0f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                GUILayout.Space(10);

                if (GUILayout.Button("SETTINGS", _buttonStyle, GUILayout.Height(44)))
                {
                    _showSettings = true;
                }
                GUILayout.Space(10);

                if (GUILayout.Button("QUIT TO TITLE", _buttonStyle, GUILayout.Height(44)))
                {
                    Time.timeScale = 1.0f;
                    SceneManager.LoadScene("MainMenu_Origin");
                }
            }
            else
            {
                GUILayout.Label("SYSTEM SETTINGS", _titleStyle);
                GUILayout.Space(15);

                var data = SaveSystem.Data;

                GUILayout.Label($"MASTER VOLUME: {(data.masterVolume * 100):F0}%", _labelStyle);
                data.masterVolume = GUILayout.HorizontalSlider(data.masterVolume, 0f, 1f);
                AudioListener.volume = data.masterVolume;
                GUILayout.Space(10);

                GUILayout.Label($"MOUSE SENSITIVITY: {data.mouseSensitivity:F1}", _labelStyle);
                data.mouseSensitivity = GUILayout.HorizontalSlider(data.mouseSensitivity, 0.5f, 5f);
                var input = FindAnyObjectByType<ThirdPersonPlayerInput>();
                if (input != null) input.mouseSensitivity = data.mouseSensitivity;
                GUILayout.Space(10);

                data.invertY = GUILayout.Toggle(data.invertY, " INVERT Y-AXIS", _labelStyle);
                if (input != null) input.invertY = data.invertY;
                GUILayout.Space(15);

                if (GUILayout.Button("FULLSCREEN TOGGLE", _buttonStyle, GUILayout.Height(36)))
                {
                    Screen.fullScreen = !Screen.fullScreen;
                }
                GUILayout.Space(15);

                if (GUILayout.Button("APPLY & BACK", _buttonStyle, GUILayout.Height(42)))
                {
                    SaveSystem.Save();
                    _showSettings = false;
                }
            }

            GUILayout.EndArea();
        }

        private void InitStyles()
        {
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 24,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                _titleStyle.normal.textColor = new Color(0.85f, 0.92f, 1.0f);

                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter
                };
                _labelStyle.normal.textColor = new Color(0.45f, 0.8f, 1.0f);

                _buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }
    }
}
