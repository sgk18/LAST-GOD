using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LastGod.Core
{
    [Serializable]
    public struct RadioMessage
    {
        public string speaker;
        [TextArea(2, 4)] public string message;
        public float duration;
        public AudioClip sfx;

        public RadioMessage(string spk, string msg, float dur = 3.5f, AudioClip clip = null)
        {
            speaker = spk;
            message = msg;
            duration = dur;
            sfx = clip;
        }
    }

    /// <summary>
    /// Facility Control radio communications dispatch system.
    /// </summary>
    public class RadioSystem : MonoBehaviour
    {
        public static RadioSystem Instance { get; private set; }

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip defaultTransmissionChime;

        public event Action<RadioMessage> OnTransmissionReceived;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        public void Transmit(RadioMessage message)
        {
            if (message.sfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(message.sfx);
            }
            else if (defaultTransmissionChime != null && audioSource != null)
            {
                audioSource.PlayOneShot(defaultTransmissionChime);
            }

            OnTransmissionReceived?.Invoke(message);
        }
    }
}
