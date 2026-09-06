using System.Collections.Generic;
using UnityEngine;

namespace LastGod.ThirdPerson.Combat
{
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        [Header("Targeting / Lock-on")]
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private float lockOnRadius = 18f;

        private readonly List<IDamageReceiver> _activeTargets = new();
        private Transform _currentLockedTarget;

        public Transform CurrentLockedTarget => _currentLockedTarget;
        public bool HasLockedTarget => _currentLockedTarget != null && _currentLockedTarget.gameObject.activeInHierarchy;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RegisterTarget(IDamageReceiver target)
        {
            if (!_activeTargets.Contains(target))
            {
                _activeTargets.Add(target);
            }
        }

        public void UnregisterTarget(IDamageReceiver target)
        {
            _activeTargets.Remove(target);
            if (_currentLockedTarget == target.transform)
            {
                _currentLockedTarget = null;
            }
        }

        public Transform ToggleLockOn(Vector3 playerPosition, Vector3 cameraForward)
        {
            if (HasLockedTarget)
            {
                _currentLockedTarget = null;
                return null;
            }

            // Clean dead targets
            _activeTargets.RemoveAll(t => t == null || t.IsDead || !t.transform.gameObject.activeInHierarchy);

            Transform bestTarget = null;
            float highestScore = -1f;

            foreach (var target in _activeTargets)
            {
                if (target == null || target.IsDead) continue;
                Vector3 toTarget = target.transform.position - playerPosition;
                float dist = toTarget.magnitude;
                if (dist > lockOnRadius) continue;

                float angle = Vector3.Angle(cameraForward, toTarget);
                if (angle > 75f) continue;

                // Higher score for closer to crosshair and closer in distance
                float score = (1f - (angle / 75f)) * 0.6f + (1f - (dist / lockOnRadius)) * 0.4f;
                if (score > highestScore)
                {
                    highestScore = score;
                    bestTarget = target.transform;
                }
            }

            _currentLockedTarget = bestTarget;
            return _currentLockedTarget;
        }

        public int PerformMeleeHit(Vector3 origin, Vector3 forward, float radius, float distance, DamageInfo damage, LayerMask hitMask)
        {
            int hits = 0;
            RaycastHit[] results = Physics.SphereCastAll(origin, radius, forward, distance, hitMask, QueryTriggerInteraction.Ignore);

            foreach (var r in results)
            {
                if (r.collider.gameObject == damage.Attacker) continue;

                if (r.collider.TryGetComponent<IDamageReceiver>(out var receiver) && !receiver.IsDead)
                {
                    DamageInfo applied = damage;
                    applied.HitPoint = r.point != Vector3.zero ? r.point : r.collider.bounds.center;
                    applied.HitNormal = r.normal != Vector3.zero ? r.normal : -forward;
                    applied.KnockbackDirection = forward;

                    receiver.TakeDamage(applied);
                    hits++;
                }
            }

            return hits;
        }
    }
}
