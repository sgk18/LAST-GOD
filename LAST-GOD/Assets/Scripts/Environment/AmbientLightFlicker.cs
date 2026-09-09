using UnityEngine;

namespace LastGod.Environment
{
    /// <summary>
    /// Drives ambient light flicker on lab monitor screens and warning lights.
    /// Uses sine wave + random jitter for an organic feel.
    /// Attach to the SpriteRenderer of a monitor or light prop.
    /// </summary>
    public class AmbientLightFlicker : MonoBehaviour
    {
        public enum FlickerMode
        {
            MonitorGlow,   // Slow sine pulse with occasional stutter
            AlarmBlink,    // Sharp on/off blink at fixed interval
            SteamPulse,    // Slow fade-in, fast fade-out
        }

        [Header("Settings")]
        [SerializeField] private FlickerMode _mode = FlickerMode.MonitorGlow;
        [SerializeField] private float _baseFrequency = 2f;
        [SerializeField] private float _minAlpha = 0.3f;
        [SerializeField] private float _maxAlpha = 1.0f;
        [SerializeField] private Color _onColor = new Color(0f, 0.83f, 1f, 1f);    // Cyan monitor glow
        [SerializeField] private Color _offColor = new Color(0f, 0.2f, 0.3f, 1f);  // Dark off-state

        [Header("Jitter (Monitor only)")]
        [SerializeField] private float _stutterChance = 0.03f;   // per frame probability
        [SerializeField] private float _stutterDuration = 0.08f;

        private SpriteRenderer _sr;
        private float _time;
        private float _stutterTimer;
        private bool _stuttering;

        // ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _time = Random.Range(0f, Mathf.PI * 2f); // randomise phase per instance
        }

        private void Update()
        {
            _time += Time.deltaTime * _baseFrequency;

            switch (_mode)
            {
                case FlickerMode.MonitorGlow:
                    UpdateMonitorGlow();
                    break;
                case FlickerMode.AlarmBlink:
                    UpdateAlarmBlink();
                    break;
                case FlickerMode.SteamPulse:
                    UpdateSteamPulse();
                    break;
            }
        }

        // ── Monitor: smooth sine + random stutter ──────────────────────
        private void UpdateMonitorGlow()
        {
            if (!_stuttering && Random.value < _stutterChance)
            {
                _stuttering = true;
                _stutterTimer = _stutterDuration;
            }

            float alpha;
            if (_stuttering)
            {
                _stutterTimer -= Time.deltaTime;
                alpha = _stutterTimer > _stutterDuration * 0.5f ? _minAlpha : _maxAlpha;
                if (_stutterTimer <= 0f) _stuttering = false;
            }
            else
            {
                float t = (Mathf.Sin(_time) + 1f) * 0.5f;
                alpha = Mathf.Lerp(_minAlpha, _maxAlpha, t);
            }

            SetAlpha(alpha);
            _sr.color = Color.Lerp(_offColor, _onColor, alpha);
        }

        // ── Alarm: hard square-wave blink ──────────────────────────────
        private void UpdateAlarmBlink()
        {
            bool on = Mathf.Sin(_time) > 0f;
            _sr.color = on ? _onColor : _offColor;
        }

        // ── Steam: saw-wave fade ───────────────────────────────────────
        private void UpdateSteamPulse()
        {
            float t = Mathf.Repeat(_time / (Mathf.PI * 2f), 1f);
            // Slow rise (80% of cycle), fast fall (20%)
            float alpha = t < 0.8f
                ? Mathf.Lerp(_minAlpha, _maxAlpha, t / 0.8f)
                : Mathf.Lerp(_maxAlpha, _minAlpha, (t - 0.8f) / 0.2f);

            SetAlpha(alpha);
        }

        private void SetAlpha(float alpha)
        {
            var c = _sr.color;
            c.a = alpha;
            _sr.color = c;
        }
    }
}
