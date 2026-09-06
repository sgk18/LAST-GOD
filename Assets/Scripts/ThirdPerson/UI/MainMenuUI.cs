using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using LastGod.ThirdPerson.Save;

namespace LastGod.ThirdPerson.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Scene Loading")]
        [SerializeField] private string gameSceneName = "Act1_Origin";

        private bool _isLoading;
        private float _loadProgress;
        private bool _showSettings;

        private GUIStyle _titleStyle;
        private GUIStyle _subtitleStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _labelStyle;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SaveSystem.Load();
            AudioListener.volume = SaveSystem.Data.masterVolume;
        }

        public void StartNewGame()
        {
            SaveSystem.ResetSave();
            StartCoroutine(LoadGameSceneAsync());
        }

        public void ContinueGame()
        {
            StartCoroutine(LoadGameSceneAsync());
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private IEnumerator LoadGameSceneAsync()
        {
            _isLoading = true;
            AsyncOperation op = SceneManager.LoadSceneAsync(gameSceneName);

            while (!op.isDone)
            {
                _loadProgress = Mathf.Clamp01(op.progress / 0.9f);
                yield return null;
            }
        }

        private void OnGUI()
        {
            InitStyles();

            if (_isLoading)
            {
                // Loading screen overlay
                GUI.color = new Color(0.02f, 0.03f, 0.05f, 1.0f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;

                GUI.Label(new Rect(0, Screen.height * 0.45f, Screen.width, 30), "INITIALIZING CONTAINMENT MATRIX...", _labelStyle);

                float barWidth = 320f;
                float bx = (Screen.width - barWidth) * 0.5f;
                float by = Screen.height * 0.52f;

                GUI.color = new Color(0.1f, 0.15f, 0.2f, 0.9f);
                GUI.DrawTexture(new Rect(bx, by, barWidth, 8f), Texture2D.whiteTexture);

                GUI.color = new Color(0.2f, 0.8f, 1.0f, 0.95f);
                GUI.DrawTexture(new Rect(bx, by, barWidth * _loadProgress, 8f), Texture2D.whiteTexture);
                GUI.color = Color.white;
                return;
            }

            // Cinematic Menu Panel
            float panelWidth = 380f;
            float panelHeight = _showSettings ? 420f : 340f;
            float x = 60f;
            float y = Screen.height * 0.42f;

            GUILayout.BeginArea(new Rect(x, y, panelWidth, panelHeight));

            if (!_showSettings)
            {
                GUILayout.Label("THE LAST GOD", _titleStyle);
                GUILayout.Label("ACT I // ORIGIN", _subtitleStyle);
                GUILayout.Space(25);

                if (GUILayout.Button("NEW GAME", _buttonStyle, GUILayout.Height(46)))
                {
                    StartNewGame();
                }
                GUILayout.Space(10);

                if (GUILayout.Button("CONTINUE", _buttonStyle, GUILayout.Height(46)))
                {
                    ContinueGame();
                }
                GUILayout.Space(10);

                if (GUILayout.Button("SETTINGS", _buttonStyle, GUILayout.Height(46)))
                {
                    _showSettings = true;
                }
                GUILayout.Space(10);

                if (GUILayout.Button("QUIT", _buttonStyle, GUILayout.Height(46)))
                {
                    QuitGame();
                }
            }
            else
            {
                GUILayout.Label("SYSTEM CONFIGURATION", _titleStyle);
                GUILayout.Space(15);

                var data = SaveSystem.Data;

                GUILayout.Label($"AUDIO VOLUME: {(data.masterVolume * 100):F0}%", _labelStyle);
                data.masterVolume = GUILayout.HorizontalSlider(data.masterVolume, 0f, 1f);
                AudioListener.volume = data.masterVolume;
                GUILayout.Space(10);

                GUILayout.Label($"LOOK SENSITIVITY: {data.mouseSensitivity:F1}", _labelStyle);
                data.mouseSensitivity = GUILayout.HorizontalSlider(data.mouseSensitivity, 0.5f, 5f);
                GUILayout.Space(10);

                data.invertY = GUILayout.Toggle(data.invertY, " INVERT Y-AXIS", _labelStyle);
                GUILayout.Space(15);

                if (GUILayout.Button("FULLSCREEN TOGGLE", _buttonStyle, GUILayout.Height(36)))
                {
                    Screen.fullScreen = !Screen.fullScreen;
                }
                GUILayout.Space(15);

                if (GUILayout.Button("SAVE & RETURN", _buttonStyle, GUILayout.Height(44)))
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
                    fontSize = 32,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
                _titleStyle.normal.textColor = new Color(0.92f, 0.96f, 1.0f);

                _subtitleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.UpperLeft
                };
                _subtitleStyle.normal.textColor = new Color(0.35f, 0.85f, 1.0f);

                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.UpperLeft
                };
                _labelStyle.normal.textColor = new Color(0.8f, 0.9f, 1.0f);

                _buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(18, 10, 0, 0)
                };
            }
        }
    }
}
