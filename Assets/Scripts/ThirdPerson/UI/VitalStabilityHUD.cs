using UnityEngine;
using LastGod.ThirdPerson.Player;
using LastGod.ThirdPerson.Combat;

namespace LastGod.ThirdPerson.UI
{
    public class VitalStabilityHUD : MonoBehaviour
    {
        [SerializeField] private ThirdPersonPlayerController player;
        [SerializeField] private Texture2D lockOnReticleTexture;

        private GUIStyle _headerStyle;
        private GUIStyle _subStyle;
        private GUIStyle _reticleStyle;

        private void Start()
        {
            if (player == null)
            {
                player = FindAnyObjectByType<ThirdPersonPlayerController>();
            }
        }

        private void OnGUI()
        {
            if (player == null) return;
            var health = player.Health;
            if (health == null) return;

            // Don't show HUD during cinematic opening if input is locked and health is full
            var director = FindAnyObjectByType<Cinematics.Act1OriginDirector>();
            if (director != null && !director.IsCombatActive && director.IsActComplete) return;

            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
                _headerStyle.normal.textColor = new Color(0.45f, 0.85f, 1.0f, 0.85f);

                _subStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 10,
                    alignment = TextAnchor.UpperLeft
                };
                _subStyle.normal.textColor = new Color(0.75f, 0.85f, 0.95f, 0.7f);

                _reticleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                _reticleStyle.normal.textColor = new Color(1.0f, 0.25f, 0.25f, 0.9f);
            }

            // Top-left diegetic HUD
            float x = 28f;
            float y = 24f;
            float barWidth = 220f;
            float barHeight = 10f;

            // Integrity Title
            GUI.Label(new Rect(x, y, 260, 16), "VITAL STABILITY // AERON INTEGRITY", _headerStyle);

            // Health Bar Background
            GUI.color = new Color(0.05f, 0.10f, 0.15f, 0.85f);
            GUI.DrawTexture(new Rect(x, y + 20, barWidth, barHeight), Texture2D.whiteTexture);

            // Health Fill
            float pct = health.HealthPercent;
            Color barColor = pct > 0.35f ? new Color(0.15f, 0.75f, 0.95f, 0.95f) : new Color(0.95f, 0.25f, 0.25f, 0.95f);
            GUI.color = barColor;
            GUI.DrawTexture(new Rect(x, y + 20, barWidth * pct, barHeight), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Ascension Surge Indicator
            var surge = player.Surge;
            if (surge != null)
            {
                float surgeY = y + 36;
                string surgeStatus = surge.IsActive ? "ASCENSION SURGE // ACTIVE [OVERDRIVE]" : (surge.IsOnCooldown ? $"SURGE RECHARGING [{surge.CooldownRemaining:F1}s]" : "SURGE READY [Q / 1]");
                Color surgeColor = surge.IsActive ? new Color(0.2f, 1.0f, 0.6f, 0.95f) : (surge.IsOnCooldown ? new Color(0.5f, 0.5f, 0.6f, 0.7f) : new Color(0.3f, 0.85f, 1.0f, 0.9f));

                GUIStyle curSurgeStyle = new GUIStyle(_subStyle);
                curSurgeStyle.normal.textColor = surgeColor;
                GUI.Label(new Rect(x, surgeY, 260, 16), surgeStatus, curSurgeStyle);

                // Surge gauge
                GUI.color = new Color(0.05f, 0.08f, 0.12f, 0.8f);
                GUI.DrawTexture(new Rect(x, surgeY + 18, barWidth, 4f), Texture2D.whiteTexture);

                float surgeProgress = surge.IsActive ? 1.0f : (1.0f - surge.CooldownPercent);
                GUI.color = surgeColor;
                GUI.DrawTexture(new Rect(x, surgeY + 18, barWidth * surgeProgress, 4f), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // Lock-On Reticle in World Space
            var cm = CombatManager.Instance;
            if (cm != null && cm.HasLockedTarget && Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(cm.CurrentLockedTarget.position + Vector3.up * 1.0f);
                if (screenPos.z > 0f)
                {
                    float rx = screenPos.x;
                    float ry = Screen.height - screenPos.y;
                    GUI.Label(new Rect(rx - 20, ry - 20, 40, 40), "[ + ]", _reticleStyle);
                }
            }
        }
    }
}
