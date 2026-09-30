using UnityEngine;
using LastGod.Weapons;

namespace LastGod.UI
{
    /// <summary>
    /// Minimalist 6-round magazine ammunition indicator on screen.
    /// </summary>
    public class AmmoHUD : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour weaponMono;
        private IWeapon _weapon;
        private int _current = 6;
        private int _max = 6;

        private void Awake()
        {
            if (weaponMono is IWeapon w)
            {
                _weapon = w;
            }
        }

        private void Start()
        {
            if (_weapon == null)
            {
                _weapon = FindAnyObjectByType<SecurityPistol>();
            }

            if (_weapon != null)
            {
                _weapon.OnAmmoChanged += HandleAmmoChanged;
                _current = _weapon.CurrentAmmo;
                _max = _weapon.MagazineCapacity;
            }
        }

        private void OnDestroy()
        {
            if (_weapon != null)
            {
                _weapon.OnAmmoChanged -= HandleAmmoChanged;
            }
        }

        private void HandleAmmoChanged(int current, int max)
        {
            _current = current;
            _max = max;
        }

        private void OnGUI()
        {
            if (_weapon == null) return;

            GUI.color = Color.white;
            GUI.backgroundColor = new Color(0.05f, 0.08f, 0.12f, 0.85f);

            Rect hudRect = new Rect(Screen.width - 120, Screen.height - 50, 100, 36);
            GUI.Box(hudRect, GUIContent.none);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = _current > 1 ? new Color(0.43f, 0.89f, 1.0f) : new Color(1.0f, 0.35f, 0.2f) }
            };

            GUI.Label(hudRect, $"{_current} / {_max}", labelStyle);
        }
    }
}
