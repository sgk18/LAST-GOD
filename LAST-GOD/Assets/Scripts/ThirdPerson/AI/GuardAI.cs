using System.Collections;
using UnityEngine;
using LastGod.ThirdPerson.Combat;
using LastGod.ThirdPerson.Dialogue;

namespace LastGod.ThirdPerson.AI
{
    public enum GuardState
    {
        Idle,
        Patrol,
        Investigate,
        Alert,
        Chase,
        Attack,
        Stagger,
        Dead
    }

    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Health3D))]
    [RequireComponent(typeof(DamageReceiver))]
    public class GuardAI : MonoBehaviour
    {
        [Header("Patrol & Detection")]
        [SerializeField] private float detectionRadius = 14f;
        [SerializeField] private float combatDistance = 7.5f;
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private float runSpeed = 5.4f;
        [SerializeField] private Transform[] patrolWaypoints;

        [Header("Tactical Dialogue")]
        [SerializeField] private string[] alertCallouts = new string[]
        {
            "CONTAIN THE SUBJECT!",
            "DO NOT LET IT OUT!",
            "TARGET ESCAPED CONTAINMENT! OPEN FIRE!",
            "SUBDUE IT IMMEDIATELY!"
        };

        [Header("Components")]
        [SerializeField] private GuardWeapon weapon;
        [SerializeField] private Light flashlight;
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private AudioClip alertSFX;

        private CharacterController _cc;
        private Health3D _health;
        private GuardState _state = GuardState.Idle;
        private Transform _targetPlayer;
        private int _currentWaypointIndex;
        private float _stateTimer;
        private bool _hasAlerted;

        public GuardState CurrentState => _state;
        public bool IsDead => _state == GuardState.Dead;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _health = GetComponent<Health3D>();
            if (weapon == null) weapon = GetComponentInChildren<GuardWeapon>();

            _health.OnDamaged.AddListener(OnTakeDamage);
            _health.OnDeath.AddListener(OnDeath);
        }

        private void Start()
        {
            var cm = CombatManager.Instance;
            if (cm != null && TryGetComponent<IDamageReceiver>(out var receiver))
            {
                cm.RegisterTarget(receiver);
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) _targetPlayer = player.transform;
        }

        private void Update()
        {
            if (_state == GuardState.Dead) return;

            if (_targetPlayer == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) _targetPlayer = player.transform;
            }

            switch (_state)
            {
                case GuardState.Idle:
                    UpdateIdle();
                    break;
                case GuardState.Patrol:
                    UpdatePatrol();
                    break;
                case GuardState.Alert:
                    UpdateAlert();
                    break;
                case GuardState.Chase:
                    UpdateChase();
                    break;
                case GuardState.Attack:
                    UpdateAttack();
                    break;
                case GuardState.Stagger:
                    // Handled in coroutine
                    break;
            }

            // Apply gravity
            if (!_cc.isGrounded)
            {
                _cc.Move(Vector3.down * (9.81f * Time.deltaTime));
            }
        }

        private void UpdateIdle()
        {
            _stateTimer += Time.deltaTime;
            CheckPlayerDetection();

            if (_stateTimer > 2.5f && patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                _state = GuardState.Patrol;
                _stateTimer = 0f;
            }
        }

        private void UpdatePatrol()
        {
            CheckPlayerDetection();
            if (patrolWaypoints == null || patrolWaypoints.Length == 0) return;

            Transform targetWp = patrolWaypoints[_currentWaypointIndex];
            Vector3 diff = targetWp.position - transform.position;
            diff.y = 0f;

            if (diff.magnitude < 0.8f)
            {
                _currentWaypointIndex = (_currentWaypointIndex + 1) % patrolWaypoints.Length;
                _state = GuardState.Idle;
                _stateTimer = 0f;
                return;
            }

            MoveToward(targetWp.position, moveSpeed);
        }

        private void UpdateAlert()
        {
            _stateTimer += Time.deltaTime;
            if (_targetPlayer != null)
            {
                LookAtTarget(_targetPlayer.position);
            }

            if (_stateTimer > 0.8f)
            {
                _state = GuardState.Chase;
            }
        }

        private void UpdateChase()
        {
            if (_targetPlayer == null) return;

            float dist = Vector3.Distance(transform.position, _targetPlayer.position);
            if (dist <= combatDistance)
            {
                _state = GuardState.Attack;
                return;
            }

            MoveToward(_targetPlayer.position, runSpeed);
        }

        private void UpdateAttack()
        {
            if (_targetPlayer == null) return;

            float dist = Vector3.Distance(transform.position, _targetPlayer.position);
            LookAtTarget(_targetPlayer.position);

            if (weapon != null)
            {
                Vector3 aimPos = _targetPlayer.position + Vector3.up * 1.1f;
                weapon.AimAt(aimPos);

                if (weapon.CanFire)
                {
                    weapon.Fire(aimPos, _targetPlayer);
                }
            }

            // If player moves too far, chase again
            if (dist > combatDistance * 1.5f)
            {
                if (weapon != null) weapon.DisableLaser();
                _state = GuardState.Chase;
            }
            // Reposition slightly if too close
            else if (dist < 3.0f)
            {
                Vector3 retreatDir = (transform.position - _targetPlayer.position).normalized;
                retreatDir.y = 0f;
                _cc.Move(retreatDir * (moveSpeed * Time.deltaTime));
            }
        }

        private void CheckPlayerDetection()
        {
            if (_targetPlayer == null) return;

            float dist = Vector3.Distance(transform.position, _targetPlayer.position);
            if (dist <= detectionRadius)
            {
                Vector3 eyePos = transform.position + Vector3.up * 1.5f;
                Vector3 toPlayer = (_targetPlayer.position + Vector3.up * 1.0f) - eyePos;

                if (!Physics.Raycast(eyePos, toPlayer.normalized, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore) || hit.collider.CompareTag("Player"))
                {
                    TriggerAlert();
                }
            }
        }

        public void TriggerAlert()
        {
            if (_hasAlerted) return;
            _hasAlerted = true;
            _state = GuardState.Alert;
            _stateTimer = 0f;

            if (voiceSource != null && alertSFX != null) voiceSource.PlayOneShot(alertSFX);

            // Shouted dialogue
            if (alertCallouts.Length > 0)
            {
                string line = alertCallouts[Random.Range(0, alertCallouts.Length)];
                var ds = DialogueSystem.Instance;
                if (ds != null)
                {
                    ds.QueueSubtitle("GUARD", line, 2.0f);
                }
            }
        }

        private void MoveToward(Vector3 targetPos, float speed)
        {
            Vector3 dir = (targetPos - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
                _cc.Move(dir.normalized * (speed * Time.deltaTime));
            }
        }

        private void LookAtTarget(Vector3 targetPos)
        {
            Vector3 dir = (targetPos - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 12f * Time.deltaTime);
            }
        }

        private void OnTakeDamage(DamageInfo damage)
        {
            if (_state == GuardState.Dead) return;

            TriggerAlert();
            StartCoroutine(StaggerRoutine(damage));
        }

        private IEnumerator StaggerRoutine(DamageInfo damage)
        {
            GuardState prevState = _state;
            _state = GuardState.Stagger;

            // Apply knockback impulse
            float elapsed = 0f;
            float staggerTime = damage.IsHeavy ? 0.45f : 0.22f;
            Vector3 knockDir = damage.KnockbackDirection != Vector3.zero ? damage.KnockbackDirection : -transform.forward;
            knockDir.y = 0f;

            while (elapsed < staggerTime)
            {
                elapsed += Time.deltaTime;
                _cc.Move(knockDir * (damage.KnockbackForce * (1f - elapsed / staggerTime) * Time.deltaTime));
                yield return null;
            }

            _state = GuardState.Chase;
        }

        private void OnDeath(GameObject killer)
        {
            _state = GuardState.Dead;
            _cc.enabled = false;
            if (weapon != null) weapon.DisableLaser();
            if (flashlight != null) flashlight.enabled = false;

            var cm = CombatManager.Instance;
            if (cm != null && TryGetComponent<IDamageReceiver>(out var receiver))
            {
                cm.UnregisterTarget(receiver);
            }

            // Collapse to floor (ragdoll simulation or rotation flop)
            transform.rotation = Quaternion.Euler(75f, transform.eulerAngles.y, 15f);
            transform.position += Vector3.down * 0.4f;

            // Notify act sequence director of guard death
            var director = FindAnyObjectByType<Cinematics.Act1OriginDirector>();
            if (director != null)
            {
                director.OnGuardKilled(this);
            }
        }
    }
}
