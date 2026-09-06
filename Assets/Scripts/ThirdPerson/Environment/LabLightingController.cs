using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LastGod.ThirdPerson.Environment
{
    public class LabLightingController : MonoBehaviour
    {
        public enum LightingMode
        {
            Sterile,
            EmergencyAlert,
            Darkness
        }

        [Header("Lighting Groups")]
        [SerializeField] private List<Light> sterileLights = new();
        [SerializeField] private List<Light> emergencyRedLights = new();
        [SerializeField] private AudioSource alarmAudioSource;
        [SerializeField] private AudioClip alarmKlaxonSFX;

        [Header("Alarm Dynamics")]
        [SerializeField] private float strobeSpeed = 6.0f;
        [SerializeField] private float maxEmergencyIntensity = 3.5f;

        private LightingMode _currentMode = LightingMode.Sterile;
        private float _strobeTime;

        private void Start()
        {
            SetLightingMode(LightingMode.Sterile);
        }

        public void SetLightingMode(LightingMode mode)
        {
            _currentMode = mode;

            switch (mode)
            {
                case LightingMode.Darkness:
                    foreach (var l in sterileLights) if (l != null) l.enabled = false;
                    foreach (var l in emergencyRedLights) if (l != null) l.enabled = false;
                    if (alarmAudioSource != null) alarmAudioSource.Stop();
                    RenderSettings.ambientLight = new Color(0.01f, 0.01f, 0.02f);
                    break;

                case LightingMode.Sterile:
                    foreach (var l in sterileLights)
                    {
                        if (l != null)
                        {
                            l.enabled = true;
                            l.color = new Color(0.7f, 0.85f, 1.0f);
                            l.intensity = 1.8f;
                        }
                    }
                    foreach (var l in emergencyRedLights) if (l != null) l.enabled = false;
                    if (alarmAudioSource != null) alarmAudioSource.Stop();
                    RenderSettings.ambientLight = new Color(0.08f, 0.10f, 0.14f);
                    break;

                case LightingMode.EmergencyAlert:
                    foreach (var l in sterileLights)
                    {
                        if (l != null)
                        {
                            l.intensity = 0.4f; // Dim overheads
                            l.color = new Color(0.2f, 0.3f, 0.4f);
                        }
                    }
                    foreach (var l in emergencyRedLights)
                    {
                        if (l != null)
                        {
                            l.enabled = true;
                            l.color = new Color(1.0f, 0.05f, 0.05f);
                        }
                    }
                    if (alarmAudioSource != null && alarmKlaxonSFX != null)
                    {
                        alarmAudioSource.clip = alarmKlaxonSFX;
                        alarmAudioSource.loop = true;
                        alarmAudioSource.Play();
                    }
                    RenderSettings.ambientLight = new Color(0.18f, 0.02f, 0.02f);
                    break;
            }
        }

        private void Update()
        {
            if (_currentMode == LightingMode.EmergencyAlert)
            {
                _strobeTime += Time.deltaTime * strobeSpeed;
                float pulse = (Mathf.Sin(_strobeTime) + 1f) * 0.5f * maxEmergencyIntensity;

                foreach (var l in emergencyRedLights)
                {
                    if (l != null)
                    {
                        l.intensity = pulse;
                    }
                }
            }
        }
    }
}
