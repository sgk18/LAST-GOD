using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace LastGod.Player.Guard
{
    /// <summary>
    /// Controls the Guard's shoulder/handheld tactical flashlight (URP Light2D spot cone).
    /// Provides mechanical toggle, directional aiming, and soft falloff.
    /// </summary>
    public class FlashlightController2D : MonoBehaviour
    {
        [Header("Light2D Component")]
        [SerializeField] private Light2D spotLight;

        [Header("Tuning")]
        [SerializeField] private float lightRange = 7.0f;
        [SerializeField] private float innerAngle = 25.0f;
        [SerializeField] private float outerAngle = 45.0f;
        [SerializeField] private float targetIntensity = 1.25f;
        [SerializeField] private float aimFollowSpeed = 15.0f;

        [Header("Audio SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip toggleClickSfx;

        private bool _isOn = true;
        private Vector2 _targetAimDir = Vector2.right;

        public bool IsOn => _isOn;

        private void Awake()
        {
            if (spotLight == null) spotLight = GetComponentInChildren<Light2D>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            ApplyLightSettings();
        }

        private void ApplyLightSettings()
        {
            if (spotLight != null)
            {
                spotLight.pointLightInnerRadius = 0.2f;
                spotLight.pointLightOuterRadius = lightRange;
                spotLight.pointLightInnerAngle = innerAngle;
                spotLight.pointLightOuterAngle = outerAngle;
                spotLight.intensity = _isOn ? targetIntensity : 0f;
                spotLight.enabled = _isOn;
            }
        }

        private void Update()
        {
            SmoothAim();
        }

        public void SetAimDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude > 0.01f)
            {
                _targetAimDir = dir.normalized;
            }
        }

        public void Toggle()
        {
            SetEnabled(!_isOn);
        }

        public void SetEnabled(bool state)
        {
            _isOn = state;
            if (spotLight != null)
            {
                spotLight.enabled = _isOn;
                spotLight.intensity = _isOn ? targetIntensity : 0f;
            }
            if (toggleClickSfx != null && audioSource != null)
            {
                audioSource.PlayOneShot(toggleClickSfx);
            }
        }

        private void SmoothAim()
        {
            if (spotLight != null)
            {
                float targetAngle = Mathf.Atan2(_targetAimDir.y, _targetAimDir.x) * Mathf.Rad2Deg;
                Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
                spotLight.transform.rotation = Quaternion.Slerp(spotLight.transform.rotation, targetRot, aimFollowSpeed * Time.deltaTime);
            }
        }
    }
}
