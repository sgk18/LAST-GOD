using UnityEngine;
using LastGod.ThirdPerson.Combat;

namespace LastGod.ThirdPerson.AI
{
    public class GuardWeapon : MonoBehaviour
    {
        [Header("Weapon Config")]
        [SerializeField] private SlowMotionBullet bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireCooldown = 1.6f;
        [SerializeField] private float accuracySpread = 3.5f;

        [Header("VFX & Audio")]
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private LineRenderer laserSight;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip gunshotSFX;

        private float _lastFireTime;

        public bool CanFire => Time.time - _lastFireTime >= fireCooldown;

        public void AimAt(Vector3 targetPosition)
        {
            if (laserSight != null && firePoint != null)
            {
                laserSight.enabled = true;
                laserSight.SetPosition(0, firePoint.position);
                laserSight.SetPosition(1, targetPosition);
            }
        }

        public void DisableLaser()
        {
            if (laserSight != null) laserSight.enabled = false;
        }

        public void Fire(Vector3 targetPosition, Transform playerTarget)
        {
            _lastFireTime = Time.time;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + transform.forward * 0.8f + Vector3.up * 1.2f;
            Vector3 baseDir = (targetPosition - spawnPos).normalized;

            // Add slight spread
            Vector3 spread = Random.insideUnitSphere * (accuracySpread * 0.01f);
            Vector3 fireDir = (baseDir + spread).normalized;

            if (muzzleFlash != null) muzzleFlash.Play();
            if (audioSource != null && gunshotSFX != null) audioSource.PlayOneShot(gunshotSFX);

            if (bulletPrefab != null)
            {
                var bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(fireDir));
                bullet.Initialize(fireDir, playerTarget);
            }
            else
            {
                // Fallback direct projectile if prefab isn't linked yet
                GameObject fallback = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fallback.name = "SlowMo_Bullet_Instance";
                fallback.transform.position = spawnPos;
                fallback.transform.localScale = Vector3.one * 0.18f;
                var smb = fallback.AddComponent<SlowMotionBullet>();
                smb.Initialize(fireDir, playerTarget);
            }
        }
    }
}
