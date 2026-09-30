using System;
using UnityEngine;

namespace LastGod.Weapons
{
    /// <summary>
    /// Manages active weapon aiming, weapon switching, and recoil camera impulse for the player character.
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("Weapon Slots")]
        [SerializeField] private MonoBehaviour defaultWeaponMono;
        private IWeapon _activeWeapon;

        [Header("Aiming and Recoil")]
        [SerializeField] private Transform aimPivot;
        [SerializeField] private float recoilKickbackDistance = 0.06f;
        [SerializeField] private float recoilRecoverySpeed = 12f;

        private Vector2 _aimDirection = Vector2.right;
        private Vector3 _originalPivotPos;
        private float _currentRecoilOffset;

        public IWeapon ActiveWeapon => _activeWeapon;

        private void Awake()
        {
            if (defaultWeaponMono is IWeapon w)
            {
                _activeWeapon = w;
            }
            if (aimPivot != null)
            {
                _originalPivotPos = aimPivot.localPosition;
            }
        }

        private void OnEnable()
        {
            if (_activeWeapon != null)
            {
                _activeWeapon.OnFired += HandleWeaponFired;
            }
        }

        private void OnDisable()
        {
            if (_activeWeapon != null)
            {
                _activeWeapon.OnFired -= HandleWeaponFired;
            }
        }

        private void Update()
        {
            RecoverRecoil();
        }

        public void SetAimDirection(Vector2 dir)
        {
            if (dir.sqrMagnitude > 0.01f)
            {
                _aimDirection = dir.normalized;
                _activeWeapon?.Aim(_aimDirection);

                if (aimPivot != null)
                {
                    float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;
                    aimPivot.rotation = Quaternion.Euler(0, 0, angle);
                }
            }
        }

        public bool TryFire()
        {
            return _activeWeapon != null && _activeWeapon.Fire();
        }

        public void TryReload()
        {
            _activeWeapon?.Reload();
        }

        private void HandleWeaponFired()
        {
            _currentRecoilOffset = recoilKickbackDistance;
        }

        private void RecoverRecoil()
        {
            if (aimPivot != null && _currentRecoilOffset > 0.001f)
            {
                _currentRecoilOffset = Mathf.MoveTowards(_currentRecoilOffset, 0f, recoilRecoverySpeed * Time.deltaTime);
                Vector3 kickback = (Vector3)(-_aimDirection * _currentRecoilOffset);
                aimPivot.localPosition = _originalPivotPos + kickback;
            }
        }
    }
}
