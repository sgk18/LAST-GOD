using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LastGod.Core
{
    /// <summary>
    /// Master Sequence Director for Act 1 Scene 1 (INT. LAB – NIGHT).
    /// Drives opening cutscene, chamber breach, combat unlock, guard spawning, and ending beat.
    /// Uses IDamageable to stay strictly decoupled in LastGod.Core assembly.
    /// </summary>
    public class Act1Scene1SequenceManager : MonoBehaviour
    {
        [Header("Audio Sources & Clips")]
        [SerializeField] private AudioSource ambientHumSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip heartbeatSFX;
        [SerializeField] private AudioClip glassShatterSFX;
        [SerializeField] private AudioClip alarmSFX;

        [Header("Chamber References")]
        [SerializeField] private SpriteRenderer chamberRenderer;
        [SerializeField] private Sprite crackedChamberSprite;
        [SerializeField] private Sprite shatteredChamberSprite;
        [SerializeField] private ParticleSystem shatterParticles;

        [Header("Actors & Spawning")]
        [SerializeField] private GameObject aeronPrefab;
        [SerializeField] private Transform aeronSpawnPoint;
        [SerializeField] private GameObject guardPrefab;
        [SerializeField] private Transform[] guardSpawnPoints;

        [Header("Camera & UI")]
        [SerializeField] private MonoBehaviour cameraFollow;
        [SerializeField] private CutsceneUIController uiController;

        [Header("Debug")]
        [Tooltip("Skip the intro cutscene and jump straight into gameplay for testing.")]
        [SerializeField] private bool skipIntroSequence = false;

        private GameObject _spawnedAeron;
        private List<IDamageable> _activeGuards = new List<IDamageable>();
        private bool _combatActive = false;

        private void Start()
        {
            if (skipIntroSequence)
                StartCoroutine(SkipToGameplay());
            else
                StartCoroutine(PlaySequence());
        }

        /// <summary>Instantly clears the black overlay and spawns actors for quick testing.</summary>
        private IEnumerator SkipToGameplay()
        {
            // Instantly clear the black overlay
            if (uiController != null)
                yield return uiController.FadeBlackOverlay(0f, 0f);

            // Spawn Aeron
            if (aeronPrefab != null && aeronSpawnPoint != null)
            {
                _spawnedAeron = Instantiate(aeronPrefab, aeronSpawnPoint.position, Quaternion.identity);

                // Initialize Prototype Power Lock on Aeron after leaving the glass chamber (decoupled via reflection/SendMessage)
                var powerCtrlType = System.Type.GetType("LastGod.Player.PrototypePowerController, LastGod.Player");
                if (powerCtrlType != null && _spawnedAeron.GetComponent(powerCtrlType) == null)
                {
                    _spawnedAeron.AddComponent(powerCtrlType);
                }
                _spawnedAeron.SendMessage("SetPowerLockState", true, SendMessageOptions.DontRequireReceiver);

                if (cameraFollow != null)
                {
                    var field = cameraFollow.GetType().GetField("target");
                    if (field != null) field.SetValue(cameraFollow, _spawnedAeron.transform);
                }
            }

            // Spawn guards
            if (guardPrefab != null && guardSpawnPoints != null)
            {
                foreach (var sp in guardSpawnPoints)
                {
                    if (sp == null) continue;
                    GameObject gObj = Instantiate(guardPrefab, sp.position, Quaternion.identity);
                    if (_spawnedAeron != null)
                        gObj.SendMessage("SetTarget", _spawnedAeron.transform, SendMessageOptions.DontRequireReceiver);
                    if (gObj.TryGetComponent<IDamageable>(out var guardDmg))
                        _activeGuards.Add(guardDmg);
                }
            }

            _combatActive = true;
            Debug.Log("[Act1Scene1] DEBUG: Skipped intro sequence.");
        }

        private IEnumerator PlaySequence()
        {
            // -------------------------------------------------------------
            // STEP 1: PITCH DARKNESS. FAINT HEARTBEAT. MACHINES HUM.
            // -------------------------------------------------------------
            if (ambientHumSource != null)
            {
                ambientHumSource.loop = true;
                ambientHumSource.Play();
            }

            if (sfxSource != null && heartbeatSFX != null)
            {
                sfxSource.PlayOneShot(heartbeatSFX);
                yield return new WaitForSeconds(1.2f);
                sfxSource.PlayOneShot(heartbeatSFX);
                yield return new WaitForSeconds(1.2f);
            }
            else
            {
                yield return new WaitForSeconds(2.0f);
            }

            // -------------------------------------------------------------
            // STEP 2: VOICE (V.O.): "WAKE UP." / "YOU WERE NOT MADE TO SLEEP."
            // -------------------------------------------------------------
            if (uiController != null)
            {
                yield return uiController.ShowText("WAKE UP.", 1.2f);
                yield return uiController.ClearText(0.4f);
                yield return new WaitForSeconds(0.3f);

                yield return uiController.ShowText("YOU WERE NOT MADE TO SLEEP.", 1.5f);
                yield return uiController.ClearText(0.4f);
                yield return new WaitForSeconds(0.5f);

                yield return uiController.FadeBlackOverlay(0.0f, 1.0f);
            }

            // -------------------------------------------------------------
            // STEP 3: AERON'S EYES SNAP OPEN. CHAMBER CRACKS -> SHATTERS.
            // -------------------------------------------------------------
            yield return new WaitForSeconds(0.5f);

            if (chamberRenderer != null && crackedChamberSprite != null)
            {
                chamberRenderer.sprite = crackedChamberSprite;
            }

            if (CameraShake2D.Instance != null) CameraShake2D.Instance.TriggerShake(0.3f, 0.1f);
            yield return new WaitForSeconds(0.6f);

            if (chamberRenderer != null && shatteredChamberSprite != null)
            {
                chamberRenderer.sprite = shatteredChamberSprite;
            }

            TriggerGlassParticles();

            if (sfxSource != null && glassShatterSFX != null)
            {
                sfxSource.PlayOneShot(glassShatterSFX);
            }

            if (CameraShake2D.Instance != null) CameraShake2D.Instance.TriggerShake(0.6f, 0.25f);

            if (uiController != null) uiController.StartRedAlarmFlicker(12f);

            if (sfxSource != null && alarmSFX != null)
            {
                sfxSource.clip = alarmSFX;
                sfxSource.loop = true;
                sfxSource.Play();
            }

            yield return new WaitForSeconds(0.5f);

            // -------------------------------------------------------------
            // STEP 4: PLAYER CONTROL UNLOCKS. AERON AWAKENS & EMITS CHRONOS AURA.
            // -------------------------------------------------------------
            if (uiController != null) yield return uiController.FadeBlackOverlay(0f, 0.5f);

            if (_spawnedAeron == null && aeronPrefab != null && aeronSpawnPoint != null)
            {
                _spawnedAeron = Instantiate(aeronPrefab, aeronSpawnPoint.position, Quaternion.identity);
            }
            if (_spawnedAeron == null)
            {
                _spawnedAeron = GameObject.FindWithTag("Player");
            }

            if (_spawnedAeron != null)
            {
                // Initialize Prototype Power Lock on Aeron after leaving the glass chamber
                var powerCtrlType = System.Type.GetType("LastGod.Player.PrototypePowerController, LastGod.Player");
                if (powerCtrlType != null && _spawnedAeron.GetComponent(powerCtrlType) == null)
                {
                    _spawnedAeron.AddComponent(powerCtrlType);
                }
                _spawnedAeron.SendMessage("SetPowerLockState", true, SendMessageOptions.DontRequireReceiver);

                // Aeron awakens: cyan eye flare + Chronos Aura surges
                _spawnedAeron.SendMessage("TriggerAwakening", 2.0f, SendMessageOptions.DontRequireReceiver);

                if (cameraFollow != null)
                {
                    var field = cameraFollow.GetType().GetField("target");
                    if (field != null) field.SetValue(cameraFollow, _spawnedAeron.transform);
                }
            }

            yield return new WaitForSeconds(2.0f);

            // -------------------------------------------------------------
            // STEP 5: 2 GUARDS RUSH IN, FIRE BULLETS (SLOWED BY CHRONOS AURA).
            // -------------------------------------------------------------
            yield return new WaitForSeconds(0.6f);

            if (guardPrefab != null && guardSpawnPoints != null && guardSpawnPoints.Length > 0)
            {
                foreach (var sp in guardSpawnPoints)
                {
                    if (sp == null) continue;
                    GameObject gObj = Instantiate(guardPrefab, sp.position, Quaternion.identity);

                    if (_spawnedAeron != null)
                        gObj.SendMessage("SetTarget", _spawnedAeron.transform, SendMessageOptions.DontRequireReceiver);

                    if (gObj.TryGetComponent<IDamageable>(out var guardDmg))
                    {
                        _activeGuards.Add(guardDmg);
                    }
                }
            }
            else
            {
                // Look for existing guards placed in scene
                var existingMonos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
                foreach (var mb in existingMonos)
                {
                    if (mb.GetType().Name == "EnemyAI")
                    {
                        if (_spawnedAeron != null)
                            mb.SendMessage("SetTarget", _spawnedAeron.transform, SendMessageOptions.DontRequireReceiver);
                        if (mb.TryGetComponent<IDamageable>(out var gDmg))
                            _activeGuards.Add(gDmg);
                    }
                }
            }

            _combatActive = true;

            // Wait until all guards are dead
            while (_combatActive)
            {
                bool allDead = true;
                foreach (var g in _activeGuards)
                {
                    if (g != null && !g.IsDead)
                    {
                        allDead = false;
                        break;
                    }
                }

                if (allDead && _activeGuards.Count > 0)
                {
                    _combatActive = false;
                }

                yield return new WaitForSeconds(0.2f);
            }

            // -------------------------------------------------------------
            // STEP 6: AFTERMATH — FREEZE PLAYER, CONCLUDING V.O.
            // -------------------------------------------------------------
            yield return new WaitForSeconds(1.0f);

            if (_spawnedAeron != null)
            {
                _spawnedAeron.SendMessage("LockControls", true, SendMessageOptions.DontRequireReceiver);
            }

            if (sfxSource != null && sfxSource.clip == alarmSFX)
            {
                sfxSource.Stop();
            }

            yield return new WaitForSeconds(1.0f);

            if (uiController != null)
            {
                yield return uiController.ShowText("THEY WILL FEAR YOU.", 1.5f);
                yield return uiController.ClearText(0.5f);

                yield return new WaitForSeconds(0.8f);

                yield return uiController.ShowText("THEY SHOULD.", 1.5f);
                yield return uiController.ClearText(0.5f);

                yield return uiController.FadeBlackOverlay(1.0f, 1.5f);
            }

            Debug.Log("[Act1Scene1] Opening sequence complete. Cut to Black.");
        }

        private void TriggerGlassParticles()
        {
            if (shatterParticles != null)
            {
                shatterParticles.Play();
                return;
            }

            if (chamberRenderer != null)
            {
                GameObject psObj = new GameObject("GlassShatterBurst");
                psObj.transform.position = chamberRenderer.transform.position;
                ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
                var main = ps.main;
                main.startLifetime = 0.8f;
                main.startSpeed = 6f;
                main.startSize = 0.15f;
                main.startColor = new Color(0.6f, 0.9f, 1f, 0.9f);
                main.stopAction = ParticleSystemStopAction.Destroy;

                var emission = ps.emission;
                emission.rateOverTime = 0;
                emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 35) });

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(1.5f, 2.5f, 0.2f);

                ps.Play();
            }
        }
    }
}
