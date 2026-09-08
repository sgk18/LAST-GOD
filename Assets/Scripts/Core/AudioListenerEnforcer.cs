using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastGod.Core
{
    /// <summary>
    /// Guarantees that every active/loaded scene has an enabled AudioListener in the scene.
    /// Eliminates the Unity warning: "There are no audio listeners in the scene. Please ensure there is always one audio listener in the scene."
    /// </summary>
    public static class AudioListenerEnforcer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnRuntimeMethodLoad()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnsureAudioListener();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnsureAudioListener();
        }

        /// <summary>
        /// Ensures at least one enabled AudioListener exists in the active scene.
        /// </summary>
        public static void EnsureAudioListener()
        {
            AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            foreach (var l in listeners)
            {
                if (l != null && l.enabled && l.gameObject.activeInHierarchy)
                {
                    return; // An active listener already exists
                }
            }

            // If a disabled listener was found, enable it
            if (listeners.Length > 0 && listeners[0] != null)
            {
                listeners[0].enabled = true;
                return;
            }

            // Otherwise attach to Camera.main or any camera in the scene
            Camera targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = Object.FindFirstObjectByType<Camera>();
            }

            if (targetCamera != null)
            {
                if (!targetCamera.TryGetComponent<AudioListener>(out _))
                {
                    targetCamera.gameObject.AddComponent<AudioListener>();
                }
            }
            else
            {
                // Fallback: create dedicated GameObject for AudioListener
                GameObject listenerObj = new GameObject("AudioListener_AutoEnforced");
                listenerObj.AddComponent<AudioListener>();
            }
        }
    }
}
