using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LastGod.Core;
using LastGod.Player;
using LastGod.Weapons;
using LastGod.ThirdPerson.Combat;
using LastGod.ThirdPerson.Player;

namespace LastGod.UI
{
    /// <summary>
    /// Diegetic Brutalist In-Game Combat & Vitality HUD for THE LAST GOD.
    /// Manages Aeron's Vital Stability, Ascension Surge, Weapon Ammunition,
    /// Lock-On Reticle, and Tactical Controls Guide.
    /// Replaces legacy IMGUI HUDs with high-performance TextMeshPro uGUI.
    /// </summary>
    public class InGameHUDController : MonoBehaviour
    {
        [Header("Canvas Group")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Aeron Vital Stability (Top-Left)")]
        [SerializeField] private RectTransform vitalPanel;
        [SerializeField] private TextMeshProUGUI subjectLabel;
        [SerializeField] private Image healthBarFill;
        [SerializeField] private Image healthBarCriticalChevrons;
        [SerializeField] private TextMeshProUGUI healthNumericText;

        [Header("Ascension Surge (Top-Left)")]
        [SerializeField] private Image surgeBarFill;
        [SerializeField] private TextMeshProUGUI surgeStatusText;

        [Header("Tactical Weapon & Ammunition (Bottom-Right)")]
        [SerializeField] private RectTransform ammoPanel;
        [SerializeField] private TextMeshProUGUI weaponNameText;
        [SerializeField] private TextMeshProUGUI ammoNumericText;
        [SerializeField] private Image[] bulletIcons = new Image[6];
        [SerializeField] private TextMeshProUGUI reloadAlertText;

        [Header("Tactical Controls Reminder (Bottom-Left)")]
        [SerializeField] private GameObject controlsGuidePanel;
        [SerializeField] private TextMeshProUGUI controlsGuideToggleHint;

        [Header("Lock-On Reticle")]
        [SerializeField] private RectTransform lockOnReticle;
        [SerializeField] private TextMeshProUGUI lockOnTargetLabel;

        [Header("References (Auto-Found if Null)")]
        [SerializeField] private Health playerHealth;
        [SerializeField] private PlayerController playerController2D;
        [SerializeField] private ThirdPersonPlayerController playerController3D;
        [SerializeField] private MonoBehaviour weaponMono;

        private IWeapon _weapon;
        private AscensionSurge _surge;
        private bool _showControlsGuide = true;
        private float _criticalBlinkTimer = 0f;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

            FindReferences();
        }

        private void Start()
        {
            if (controlsGuidePanel != null) controlsGuidePanel.SetActive(_showControlsGuide);
        }

        private void OnEnable()
        {
            FindReferences();
            if (_weapon != null) _weapon.OnAmmoChanged += HandleAmmoChanged;
            if (playerHealth != null) playerHealth.OnDamaged.AddListener(HandleHealthChanged);
        }

        private void OnDisable()
        {
            if (_weapon != null) _weapon.OnAmmoChanged -= HandleAmmoChanged;
            if (playerHealth != null) playerHealth.OnDamaged.RemoveListener(HandleHealthChanged);
        }

        private void FindReferences()
        {
            if (playerHealth == null)
                playerHealth = FindAnyObjectByType<Health>();

            if (playerController2D == null)
                playerController2D = FindAnyObjectByType<PlayerController>();

            if (playerController3D == null)
                playerController3D = FindAnyObjectByType<ThirdPersonPlayerController>();

            if (_surge == null)
            {
                if (playerController3D != null) _surge = playerController3D.Surge;
                if (_surge == null) _surge = FindAnyObjectByType<AscensionSurge>();
            }

            if (_weapon == null)
            {
                if (weaponMono is IWeapon w) _weapon = w;
                else _weapon = FindAnyObjectByType<SecurityPistol>();
            }
        }

        private void Update()
        {
            HandleInput();
            UpdateVitalStability();
            UpdateAscensionSurge();
            UpdateWeaponAmmunition();
            UpdateLockOnReticle();
        }

        private void HandleInput()
        {
            // Toggle Controls Guide with 'H'
            bool hPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.hKey.wasPressedThisFrame)
                hPressed = true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            try { if (UnityEngine.Input.GetKeyDown(KeyCode.H)) hPressed = true; } catch {}
#endif
            if (hPressed)
            {
                _showControlsGuide = !_showControlsGuide;
                if (controlsGuidePanel != null) controlsGuidePanel.SetActive(_showControlsGuide);
                UITheme.TriggerSound("OnSelect");
            }
        }

        private void UpdateVitalStability()
        {
            int maxHp = playerHealth != null ? playerHealth.MaxHP : 10;
            int curHp = playerHealth != null ? playerHealth.CurrentHP : 10;
            float pct = Mathf.Clamp01((float)curHp / Mathf.Max(1, maxHp));

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = pct;
                bool isCritical = pct <= 0.35f;

                if (isCritical)
                {
                    _criticalBlinkTimer += Time.unscaledDeltaTime * 4f;
                    float blink = Mathf.PingPong(_criticalBlinkTimer, 1f);
                    healthBarFill.color = Color.Lerp(UITheme.WarningOrange, new Color(0.95f, 0.15f, 0.15f, 1f), blink);

                    if (healthBarCriticalChevrons != null)
                    {
                        healthBarCriticalChevrons.gameObject.SetActive(true);
                        healthBarCriticalChevrons.color = new Color(UITheme.WarningOrange.r, UITheme.WarningOrange.g, UITheme.WarningOrange.b, 0.6f + 0.4f * blink);
                    }
                }
                else
                {
                    healthBarFill.color = UITheme.TechnologyCyan;
                    if (healthBarCriticalChevrons != null)
                        healthBarCriticalChevrons.gameObject.SetActive(false);
                }
            }

            if (healthNumericText != null)
            {
                healthNumericText.text = $"{curHp} / {maxHp} HP  [{(pct * 100f):F0}%]";
                healthNumericText.color = pct <= 0.35f ? UITheme.WarningOrange : UITheme.BrightCyan;
            }
        }

        private void UpdateAscensionSurge()
        {
            if (_surge == null)
            {
                if (surgeStatusText != null) surgeStatusText.text = "SURGE LINK: DORMANT";
                if (surgeBarFill != null) surgeBarFill.fillAmount = 0f;
                return;
            }

            if (_surge.IsActive)
            {
                if (surgeBarFill != null)
                {
                    surgeBarFill.fillAmount = 1f;
                    surgeBarFill.color = UITheme.BrightCyan;
                }
                if (surgeStatusText != null)
                {
                    surgeStatusText.text = "<color=#CFF4FF><b>ASCENSION OVERDRIVE // ACTIVE</b></color>";
                }
            }
            else if (_surge.IsOnCooldown)
            {
                float prog = 1f - _surge.CooldownPercent;
                if (surgeBarFill != null)
                {
                    surgeBarFill.fillAmount = prog;
                    surgeBarFill.color = UITheme.DarkBlueGrey;
                }
                if (surgeStatusText != null)
                {
                    surgeStatusText.text = $"RECHARGING [{_surge.CooldownRemaining:F1}s]";
                }
            }
            else
            {
                if (surgeBarFill != null)
                {
                    surgeBarFill.fillAmount = 1f;
                    surgeBarFill.color = UITheme.TechnologyCyan;
                }
                if (surgeStatusText != null)
                {
                    surgeStatusText.text = "SURGE READY [Q / 1]";
                }
            }
        }

        private void UpdateWeaponAmmunition()
        {
            int current = _weapon != null ? _weapon.CurrentAmmo : 6;
            int max = _weapon != null ? _weapon.MagazineCapacity : 6;
            bool reloading = _weapon != null && _weapon.IsReloading;

            if (weaponNameText != null)
            {
                string wName = _weapon != null ? _weapon.WeaponName : "SEC-P9 // STASIS INTERCEPTOR";
                weaponNameText.text = wName;
            }

            if (ammoNumericText != null)
            {
                if (reloading)
                {
                    ammoNumericText.text = "<color=#C4502E>RELOADING...</color>";
                }
                else
                {
                    string colorTag = current <= 1 ? "<color=#C4502E>" : (current <= 2 ? "<color=#B39A45>" : "<color=#6FE3FF>");
                    ammoNumericText.text = $"{colorTag}{current:00}</color> / {max:00}";
                }
            }

            // Bullet icon array
            for (int i = 0; i < bulletIcons.Length; i++)
            {
                if (bulletIcons[i] == null) continue;
                bool isLoaded = i < current;
                bulletIcons[i].color = isLoaded ? UITheme.TechnologyCyan : new Color(0.2f, 0.25f, 0.3f, 0.35f);
            }

            if (reloadAlertText != null)
            {
                reloadAlertText.gameObject.SetActive(current == 0 && !reloading);
            }
        }

        private void UpdateLockOnReticle()
        {
            if (lockOnReticle == null) return;

            var cm = CombatManager.Instance;
            if (cm != null && cm.HasLockedTarget && Camera.main != null)
            {
                Vector3 targetWorld = cm.CurrentLockedTarget.position + Vector3.up * 1.0f;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(targetWorld);

                if (screenPos.z > 0f)
                {
                    lockOnReticle.gameObject.SetActive(true);
                    lockOnReticle.position = screenPos;
                    if (lockOnTargetLabel != null)
                    {
                        lockOnTargetLabel.text = $"[TARGET: {cm.CurrentLockedTarget.name.ToUpper()}]";
                    }
                    return;
                }
            }

            lockOnReticle.gameObject.SetActive(false);
        }

        private void HandleAmmoChanged(int current, int max)
        {
            UpdateWeaponAmmunition();
        }

        private void HandleHealthChanged(int remaining)
        {
            UpdateVitalStability();
        }

        public void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            gameObject.SetActive(visible);
        }
    }
}
