using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LastGod.ThirdPerson.Player;
using LastGod.ThirdPerson.AI;
using LastGod.ThirdPerson.Environment;
using LastGod.ThirdPerson.Dialogue;
using LastGod.ThirdPerson.Combat;
using LastGod.ThirdPerson.Save;

namespace LastGod.ThirdPerson.Cinematics
{
    public class Act1OriginDirector : MonoBehaviour
    {
        public static Act1OriginDirector Instance { get; private set; }

        [Header("Sequence Actors & Props")]
        [SerializeField] private ThirdPersonPlayerController player;
        [SerializeField] private ThirdPersonCameraController cameraController;
        [SerializeField] private StasisChamber chamber;
        [SerializeField] private LabLightingController lighting;
        [SerializeField] private InteractiveMonitor endTerminalMonitor;
        [SerializeField] private LabDoor securityDoor;
        [SerializeField] private Transform drVossNPC;
        [SerializeField] private List<GuardAI> guards = new();

        [Header("Audio Elements")]
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip heartbeatSFX;
        [SerializeField] private AudioClip ambientLabDrone;
        [SerializeField] private AudioClip combatPulseMusic;
        [SerializeField] private AudioClip ominousEndingDrone;

        [Header("GUI / Title Overlay")]
        private float _fadeAlpha = 0.0f;
        private string _centerBanner = "";
        private string _titleCardText = "";
        private bool _actComplete;

        private int _guardsDefeatedCount;
        private bool _isCombatActive;
        private bool _hasPlayedHandMoment;
        private bool _cutsceneFinished;
        private Coroutine _progressionCoroutine;

        public bool IsCombatActive => _isCombatActive;
        public bool IsActComplete => _actComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _progressionCoroutine = StartCoroutine(MasterAct1ProgressionRoutine());
        }

        private void Update()
        {
            if (!_cutsceneFinished && !_isCombatActive && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            {
                SkipCutscene();
            }
        }

        public void SkipCutscene()
        {
            if (_cutsceneFinished || _isCombatActive) return;
            if (_progressionCoroutine != null)
            {
                StopCoroutine(_progressionCoroutine);
            }
            StartCoroutine(ImmediateCombatStart());
        }

        private IEnumerator ImmediateCombatStart()
        {
            _cutsceneFinished = true;
            _fadeAlpha = 0f;
            if (lighting != null) lighting.SetLightingMode(LabLightingController.LightingMode.EmergencyAlert);
            if (chamber != null) chamber.ShatterChamber();
            if (securityDoor != null) securityDoor.UnlockAndOpen();
            if (cameraController != null) cameraController.ReleaseCinematicControl();
            if (player != null) player.SetInputLocked(false);
            SaveSystem.SaveCheckpoint(1);

            StartCoroutine(DisplayAwakenPrompt());

            _isCombatActive = true;
            foreach (var g in guards)
            {
                if (g != null) g.TriggerAlert();
            }

            if (ambientSource != null && combatPulseMusic != null)
            {
                ambientSource.clip = combatPulseMusic;
                ambientSource.loop = true;
                ambientSource.volume = 0.5f;
                ambientSource.Play();
            }

            _progressionCoroutine = StartCoroutine(CombatToClimaxRoutine());
            yield break;
        }

        private IEnumerator MasterAct1ProgressionRoutine()
        {
            // Lock player controls during opening
            if (player != null)
            {
                player.SetInputLocked(true);
            }

            // ==========================================
            // SEQUENCE 1: DARKNESS & HEARTBEAT
            // ==========================================
            _fadeAlpha = 1.0f;
            if (lighting != null) lighting.SetLightingMode(LabLightingController.LightingMode.Darkness);

            if (ambientSource != null && ambientLabDrone != null)
            {
                ambientSource.clip = ambientLabDrone;
                ambientSource.loop = true;
                ambientSource.volume = 0.4f;
                ambientSource.Play();
            }

            // Play distant heartbeat
            if (sfxSource != null && heartbeatSFX != null)
            {
                sfxSource.PlayOneShot(heartbeatSFX, 0.7f);
            }
            yield return new WaitForSeconds(1.6f);

            if (sfxSource != null && heartbeatSFX != null)
            {
                sfxSource.PlayOneShot(heartbeatSFX, 0.9f);
            }
            yield return new WaitForSeconds(1.4f);

            // ==========================================
            // SEQUENCE 2: 7-SHOT CINEMATIC CAMERA SYSTEM
            // ==========================================
            Vector3 chamberPos = chamber != null ? chamber.transform.position : (player != null ? player.transform.position : Vector3.zero);

            // SHOT 2: Extreme close-up of liquid bubbles
            if (cameraController != null)
            {
                cameraController.SetCinematicTransform(chamberPos + new Vector3(0.15f, 1.25f, 0.45f), Quaternion.Euler(15f, 180f, 0f), 35f);
            }
            yield return StartCoroutine(FadeScreen(1.0f, 0.0f, 2.0f));
            if (lighting != null) lighting.SetLightingMode(LabLightingController.LightingMode.Sterile);
            yield return new WaitForSeconds(2.0f);

            // SHOT 3: Close-up of Aeron's fingers
            if (cameraController != null)
            {
                cameraController.SetCinematicTransform(chamberPos + new Vector3(0.35f, 0.95f, 0.4f), Quaternion.Euler(25f, 220f, 0f), 38f);
            }
            yield return new WaitForSeconds(2.0f);

            // SHOT 4: Close-up of closed eyes
            if (cameraController != null)
            {
                cameraController.SetCinematicTransform(chamberPos + new Vector3(0f, 1.55f, 0.65f), Quaternion.Euler(10f, 180f, 0f), 30f);
            }
            yield return new WaitForSeconds(1.8f);

            // SHOT 5: Wide shot revealing containment chamber
            if (cameraController != null)
            {
                cameraController.SetCinematicTransform(chamberPos + new Vector3(0f, 2.0f, 4.8f), Quaternion.Euler(12f, 180f, 0f), 60f);
            }
            yield return new WaitForSeconds(2.2f);

            // SHOT 6: Camera rotates around chamber
            float rotTime = 0f;
            while (rotTime < 3.0f)
            {
                rotTime += Time.deltaTime;
                float angle = Mathf.Lerp(180f, 290f, rotTime / 3.0f);
                float rad = angle * Mathf.Deg2Rad;
                Vector3 camPos = chamberPos + new Vector3(Mathf.Sin(rad) * 4.2f, 1.8f, Mathf.Cos(rad) * 4.2f);
                Quaternion camRot = Quaternion.LookRotation((chamberPos + Vector3.up * 1.2f) - camPos);
                if (cameraController != null) cameraController.SetCinematicTransform(camPos, camRot, 55f);
                yield return null;
            }

            // SHOT 7: Control room visible through glass (Voss observation)
            if (drVossNPC != null && cameraController != null)
            {
                Vector3 vossPos = drVossNPC.position;
                cameraController.SetCinematicTransform(vossPos + new Vector3(0f, 1.2f, 2.5f), Quaternion.Euler(10f, 180f, 0f), 45f);
            }
            yield return new WaitForSeconds(2.0f);

            // ==========================================
            // SEQUENCE 3: THE VOICE & AWAKENING
            // ==========================================
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.QueueSubtitle("THE VOICE", "WAKE UP.", 2.5f);
            }
            yield return new WaitForSeconds(3.0f);

            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.QueueSubtitle("THE VOICE", "YOU WERE NOT MADE TO SLEEP.", 3.0f);
            }
            yield return new WaitForSeconds(3.5f);

            // Aeron eyes open & Chamber Failure sequence
            if (cameraController != null)
            {
                cameraController.SetCinematicTransform(chamberPos + new Vector3(0f, 1.5f, 1.4f), Quaternion.Euler(8f, 180f, 0f), 40f);
                cameraController.TriggerShake(0.08f, 2.5f);
            }

            // Chamber drains, alarms activate
            if (chamber != null)
            {
                chamber.StartDrainLiquid(2.5f);
                chamber.CrackGlass();
            }
            if (lighting != null)
            {
                lighting.SetLightingMode(LabLightingController.LightingMode.EmergencyAlert);
            }
            yield return new WaitForSeconds(1.8f);

            // Dr. Voss visibly afraid
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.QueueSubtitle("DR. ILYA VOSS", "He's awake...", 2.6f);
            }
            yield return new WaitForSeconds(2.5f);

            // Chamber Shatters!
            if (chamber != null)
            {
                chamber.ShatterChamber();
            }
            if (cameraController != null)
            {
                cameraController.TriggerShake(0.35f, 0.8f);
            }
            yield return new WaitForSeconds(1.2f);

            // Save Checkpoint 1 (After awakening)
            SaveSystem.SaveCheckpoint(1);

            // ==========================================
            // SEQUENCE 4: GUARDS ARRIVE & COMBAT
            // ==========================================
            if (securityDoor != null)
            {
                securityDoor.UnlockAndOpen();
            }

            // Guards shout
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.QueueSubtitle("GUARD", "CONTAIN THE SUBJECT!", 2.2f);
            }
            yield return new WaitForSeconds(1.0f);

            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.QueueSubtitle("GUARD", "DO NOT LET IT OUT!", 2.2f);
            }

            // Release camera and enable player controls
            if (cameraController != null)
            {
                cameraController.ReleaseCinematicControl();
            }
            if (player != null)
            {
                player.SetInputLocked(false);
            }

            // Display subtle "AWAKEN" prompt
            StartCoroutine(DisplayAwakenPrompt());

            // Alert guards
            _isCombatActive = true;
            foreach (var g in guards)
            {
                if (g != null) g.TriggerAlert();
            }

            // Start combat music pulse
            if (ambientSource != null && combatPulseMusic != null)
            {
                ambientSource.clip = combatPulseMusic;
                ambientSource.loop = true;
                ambientSource.volume = 0.5f;
                ambientSource.Play();
            }

            yield return StartCoroutine(CombatToClimaxRoutine());
        }

        private IEnumerator CombatToClimaxRoutine()
        {
            // Wait until guards are defeated
            while (AreGuardsAlive())
            {
                yield return new WaitForSeconds(0.5f);
            }

            // ==========================================
            // SEQUENCE 5: POST-COMBAT SILENCE & HANDS MOMENT
            // ==========================================
            _isCombatActive = false;

            // Combat music drops to silence
            if (ambientSource != null)
            {
                ambientSource.Stop();
            }

            yield return new WaitForSeconds(2.0f);

            // Aeron stands still, looks at hands
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.QueueSubtitle("THE VOICE", "They will fear you.", 3.0f);
            }
            yield return new WaitForSeconds(4.0f);

            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.QueueSubtitle("THE VOICE", "They should.", 3.0f);
            }
            yield return new WaitForSeconds(3.5f);

            // Save Checkpoint 2 (Pre-exit)
            SaveSystem.SaveCheckpoint(2);

            // ==========================================
            // SEQUENCE 6: ACT 1 ENDING & PROJECT ASCENSION
            // ==========================================
            if (endTerminalMonitor != null)
            {
                endTerminalMonitor.SetContent(InteractiveMonitor.MonitorContent.PhaseOneInitiated);
            }

            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.QueueSubtitle("THE VOICE", "Run.", 2.5f);
            }
            yield return new WaitForSeconds(3.0f);

            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.QueueSubtitle("THE VOICE", "They are already coming.", 3.2f);
            }
            yield return new WaitForSeconds(4.0f);

            // Final fade to black and title card
            yield return StartCoroutine(FadeScreen(0.0f, 1.0f, 2.5f));

            _titleCardText = "THE LAST GOD\n\nACT I — ORIGIN\n\nMISSION COMPLETE";
            _actComplete = true;

            // Save Checkpoint 3 (Act 1 complete)
            SaveSystem.SaveCheckpoint(3);

            if (ambientSource != null && ominousEndingDrone != null)
            {
                ambientSource.clip = ominousEndingDrone;
                ambientSource.loop = false;
                ambientSource.Play();
            }
        }

        private IEnumerator DisplayAwakenPrompt()
        {
            _centerBanner = "AWAKEN";
            yield return new WaitForSeconds(3.0f);
            _centerBanner = "";
        }

        private bool AreGuardsAlive()
        {
            int alive = 0;
            foreach (var g in guards)
            {
                if (g != null && !g.IsDead) alive++;
            }
            return alive > 0;
        }

        public void OnGuardKilled(GuardAI guard)
        {
            _guardsDefeatedCount++;
        }

        private IEnumerator FadeScreen(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _fadeAlpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _fadeAlpha = to;
        }

        private void OnGUI()
        {
            // Fullscreen fade overlay
            if (Application.isPlaying && _fadeAlpha > 0.01f)
            {
                GUI.color = new Color(0f, 0f, 0f, _fadeAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // Cutscene skip hint
            if (Application.isPlaying && !_cutsceneFinished && !_isCombatActive)
            {
                GUIStyle skipStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.LowerRight
                };
                skipStyle.normal.textColor = new Color(0.8f, 0.9f, 1f, 0.5f);
                GUI.Label(new Rect(0, Screen.height - 45, Screen.width - 25, 30), "[SPACE / ENTER] Skip Cinematic", skipStyle);
            }

            // Awakening subtle message
            if (!string.IsNullOrEmpty(_centerBanner))
            {
                GUIStyle bannerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 32,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
                bannerStyle.normal.textColor = new Color(0.8f, 0.95f, 1.0f, 0.9f);
                GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 60), _centerBanner, bannerStyle);
            }

            // Final Title Card
            if (_actComplete && !string.IsNullOrEmpty(_titleCardText))
            {
                GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 34,
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };
                titleStyle.normal.textColor = new Color(0.9f, 0.9f, 0.95f, 1.0f);
                GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 240), _titleCardText, titleStyle);

                GUIStyle subStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    alignment = TextAnchor.MiddleCenter
                };
                subStyle.normal.textColor = new Color(0.5f, 0.8f, 1.0f, 0.8f);
                GUI.Label(new Rect(0, Screen.height * 0.65f, Screen.width, 40), "[ESC] PAUSE MENU / TITLE SCREEN", subStyle);
            }
        }
    }
}
