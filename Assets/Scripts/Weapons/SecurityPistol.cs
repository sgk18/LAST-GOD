using System;
using System.Collections;
using UnityEngine;
using LastGod.Core;

namespace LastGod.Weapons
{
    /// <summary>
    /// Compact utilitarian semi-automatic security pistol issued to laboratory security personnel.
    /// Capacity: 6 rounds. Fire Rate: 0.35s. Hitscan raycast with muzzle flash and impact feedback.
    /// </summary>
    public class SecurityPistol : MonoBehaviour, IWeapon
    {
        [Header("Pistol Metrics")]
        [SerializeField] private string weaponName = "Sec-9 Sidearm";
        [SerializeField] private int magazineCapacity = 6;
        [SerializeField] private float fireCooldown = 0.32f;
        [SerializeField] private float reloadDuration = 1.25f;
        [SerializeField] private int damagePerShot = 10;
        [SerializeField] private float maxRange = 18.0f;
        [SerializeField] private LayerMask hitLayers;

        [Header("Muzzle & Visuals")]
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private LineRenderer tracerLinePrefab;
        [SerializeField] private GameObject impactSparkPrefab;

        [Header("Audio SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip fireSfx;
        [SerializeField] private AudioClip dryFireSfx;
        [SerializeField] private AudioClip reloadStartSfx;
        [SerializeField] private AudioClip reloadFinishSfx;

        private int _currentAmmo;
        private float _lastFireTime;
        private bool _isReloading;
        private Vector2 _aimDirection = Vector2.right;

        public string WeaponName => weaponName;
        public int CurrentAmmo => _currentAmmo;
        public int MagazineCapacity => magazineCapacity;
        public bool IsReloading => _isReloading;
        public bool CanFire => !_isReloading && _currentAmmo > 0 && Time.time >= _lastFireTime + fireCooldown;

        public event Action<int, int> OnAmmoChanged;
        public event Action OnFired;
        public event Action OnReloadStarted;
        public event Action OnReloadFinished;
        public event Action OnDryFired;

        private void Awake()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            _currentAmmo = magazineCapacity;
            if (hitLayers == 0)
            {
                hitLayers = LayerMask.GetMask("Ground", "Default", "Enemy");
            }
        }

        private void Start()
        {
            OnAmmoChanged?.Invoke(_currentAmmo, magazineCapacity);
        }

        public void Aim(Vector2 direction)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                _aimDirection = direction.normalized;
            }
        }

        public bool Fire()
        {
            if (_isReloading) return false;

            if (_currentAmmo <= 0)
            {
                PlayClip(dryFireSfx);
                OnDryFired?.Invoke();
                return false;
            }

            if (Time.time < _lastFireTime + fireCooldown) return false;

            _lastFireTime = Time.time;
            _currentAmmo--;
            OnAmmoChanged?.Invoke(_currentAmmo, magazineCapacity);

            PlayClip(fireSfx);
            OnFired?.Invoke();

            Vector2 origin = muzzlePoint != null ? (Vector2)muzzlePoint.position : (Vector2)transform.position;
            RaycastHit2D hit = Physics2D.Raycast(origin, _aimDirection, maxRange, hitLayers);

            Vector2 endPoint = hit.collider != null ? hit.point : origin + _aimDirection * maxRange;

            // Spawn Muzzle Flash
            if (muzzleFlashPrefab != null && muzzlePoint != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
                Destroy(flash, 0.08f);
            }

            // Impact Response
            if (hit.collider != null)
            {
                if (impactSparkPrefab != null)
                {
                    GameObject spark = Instantiate(impactSparkPrefab, hit.point, Quaternion.identity);
                    Destroy(spark, 0.25f);
                }

                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damagePerShot, _aimDirection);
                }
            }

            return true;
        }

        public void Reload()
        {
            if (_isReloading || _currentAmmo == magazineCapacity) return;
            StartCoroutine(ReloadRoutine());
        }

        private IEnumerator ReloadRoutine()
        {
            _isReloading = true;
            PlayClip(reloadStartSfx);
            OnReloadStarted?.Invoke();

            yield return new WaitForSeconds(reloadDuration);

            _currentAmmo = magazineCapacity;
            _isReloading = false;
            PlayClip(reloadFinishSfx);
            OnReloadFinished?.Invoke();
            OnAmmoChanged?.Invoke(_currentAmmo, magazineCapacity);
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
