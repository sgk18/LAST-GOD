using System.Collections;
using UnityEngine;

namespace LastGod.Core
{
    /// <summary>
    /// Minimalist monospace typewriter UI rendering Facility Control radio transmissions in upper-left screen.
    /// </summary>
    public class RadioDialogueUI : MonoBehaviour
    {
        private RadioMessage _currentMessage;
        private string _displayedText = "";
        private bool _isDisplaying = false;
        private float _displayTimer = 0f;

        private void Start()
        {
            if (RadioSystem.Instance != null)
            {
                RadioSystem.Instance.OnTransmissionReceived += HandleTransmission;
            }
        }

        private void OnDestroy()
        {
            if (RadioSystem.Instance != null)
            {
                RadioSystem.Instance.OnTransmissionReceived -= HandleTransmission;
            }
        }

        private void HandleTransmission(RadioMessage msg)
        {
            _currentMessage = msg;
            StopAllCoroutines();
            StartCoroutine(TypewriterRoutine(msg));
        }

        private IEnumerator TypewriterRoutine(RadioMessage msg)
        {
            _isDisplaying = true;
            _displayedText = "";
            for (int i = 0; i <= msg.message.Length; i++)
            {
                _displayedText = msg.message.Substring(0, i);
                yield return new WaitForSeconds(0.025f);
            }

            yield return new WaitForSeconds(msg.duration);
            _isDisplaying = false;
        }

        private void OnGUI()
        {
            if (!_isDisplaying) return;

            GUI.color = Color.white;
            GUI.backgroundColor = new Color(0.05f, 0.08f, 0.12f, 0.85f);

            Rect boxRect = new Rect(24, 24, 380, 75);
            GUI.Box(boxRect, GUIContent.none);

            GUIStyle headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.43f, 0.89f, 1.0f) }
            };

            GUIStyle bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                normal = { textColor = new Color(0.9f, 0.95f, 1.0f) }
            };

            GUI.Label(new Rect(34, 30, 360, 20), $"[COMMS] {_currentMessage.speaker}:", headerStyle);
            GUI.Label(new Rect(34, 48, 360, 45), _displayedText, bodyStyle);
        }
    }
}
