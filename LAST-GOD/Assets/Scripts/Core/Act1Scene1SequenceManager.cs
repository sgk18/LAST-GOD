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

            if (shatterParticles != null) shatterParticles.Play();

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
            // STEP 4: PLAYER CONTROL UNLOCKS. AERON SPAWNS.
            // -------------------------------------------------------------
            if (uiController != null) yield return uiController.FadeBlackOverlay(0f, 0.5f);

            if (aeronPrefab != null && aeronSpawnPoint != null)
            {
                _spawnedAeron = Instantiate(aeronPrefab, aeronSpawnPoint.position, Quaternion.identity);

                if (cameraFollow != null)
                {
                    var field = cameraFollow.GetType().GetField("target");
                    if (field != null) field.SetValue(cameraFollow, _spawnedAeron.transform);
                }
            }

            // -------------------------------------------------------------
            // STEP 5: 2-3 GUARDS RUSH IN, FIRE.
            // -------------------------------------------------------------
            yield return new WaitForSeconds(0.6f);

            if (guardPrefab != null && guardSpawnPoints != null)
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
            // STEP 6: AFTERMATH — FREEZE PLAYER, VOICE (V.O.): THEY WILL FEAR YOU.
            // -------------------------------------------------------------
            yield return new WaitForSeconds(1.0f);

            if (_spawnedAeron != null)
            {
                var pc = _spawnedAeron.GetComponent("PlayerController") as MonoBehaviour;
                if (pc != null) pc.enabled = false;
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
    }
}
