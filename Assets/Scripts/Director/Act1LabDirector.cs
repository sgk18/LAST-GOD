using System.Collections;
using UnityEngine;

namespace LastGod.Act1
{
    public class Act1LabDirector : MonoBehaviour
    {
        public static Act1LabDirector Instance { get; private set; }

        [Header("Scene Actors & Props")]
        public AeronController aeron;
        public GameObject stasisChamberIntact;
        public GameObject stasisChamberCracked;
        public GameObject stasisChamberShattered;
        public GameObject liquidSplashVfx;
        public GameObject glassShatterVfx;
        public GameObject alarmLightsGroup;
        public UnityEngine.Rendering.Universal.Light2D mainLabLight2D;
        public UnityEngine.Rendering.Universal.Light2D alarmLight2D;

        [Header("Intro Settings")]
        public bool skipIntroOnStart = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (skipIntroOnStart)
            {
                InstantCombatStart();
            }
            else
            {
                StartCoroutine(Scene1IntroTimeline());
            }
        }

        private void Update()
        {
            // Allow skipping intro with Enter / Escape
            if (Input.GetKeyDown(KeyCode.Return) && GameManager.Instance != null && GameManager.Instance.CurrentPhase == GameManager.GamePhase.NarrativeIntro)
            {
                StopAllCoroutines();
                InstantCombatStart();
            }
        }

        private IEnumerator Scene1IntroTimeline()
        {
            // 1. INT. LAB - NIGHT: Pitch darkness, faint heartbeat
            if (aeron != null) aeron.SetState(AeronController.AeronState.Suspended);
            if (stasisChamberIntact != null) stasisChamberIntact.SetActive(true);
            if (stasisChamberCracked != null) stasisChamberCracked.SetActive(false);
            if (stasisChamberShattered != null) stasisChamberShattered.SetActive(false);
            if (alarmLightsGroup != null) alarmLightsGroup.SetActive(false);

            if (mainLabLight2D != null) mainLabLight2D.intensity = 0.25f;
            if (alarmLight2D != null) alarmLight2D.enabled = false;

            yield return new WaitForSeconds(2.0f);

            // 2. THE VOICE SPEAKS
            UIManager.Instance?.ShowDialogue("VOICE (V.O.)", "WAKE UP.", 2.2f, "voice");
            yield return new WaitForSeconds(3.0f);

            UIManager.Instance?.ShowDialogue("VOICE (V.O.)", "YOU WERE NOT MADE TO SLEEP.", 2.6f, "voice");
            yield return new WaitForSeconds(2.8f);

            // 3. AERON EYES SNAP OPEN, ALARMS BLARE, GLASS CRACKS
            if (aeron != null) aeron.SetState(AeronController.AeronState.Awakening);
            if (alarmLightsGroup != null) alarmLightsGroup.SetActive(true);
            if (mainLabLight2D != null) mainLabLight2D.intensity = 0.85f;
            if (alarmLight2D != null) alarmLight2D.enabled = true;
            CameraFollow2D.Instance?.Shake(0.3f, 0.2f);

            yield return new WaitForSeconds(1.2f);

            // 4. CRACKING CHAMBER
            if (stasisChamberIntact != null) stasisChamberIntact.SetActive(false);
            if (stasisChamberCracked != null) stasisChamberCracked.SetActive(true);

            // 5. INT. LAB CONTROL ROOM - DR. VOSS
            UIManager.Instance?.ShowDialogue("DR. ILYA VOSS", "(whispers)  He's awake...", 2.4f, "voss");
            yield return new WaitForSeconds(3.2f);

            // 6. CHAMBER SHATTERS VIOLENTLY
            if (stasisChamberCracked != null) stasisChamberCracked.SetActive(false);
            if (stasisChamberShattered != null) stasisChamberShattered.SetActive(true);

            if (glassShatterVfx != null)
            {
                GameObject g = Instantiate(glassShatterVfx, stasisChamberShattered.transform.position, Quaternion.identity);
                Destroy(g, 1.5f);
            }
            if (liquidSplashVfx != null)
            {
                GameObject l = Instantiate(liquidSplashVfx, stasisChamberShattered.transform.position + Vector3.down * 1.0f, Quaternion.identity);
                Destroy(l, 1.5f);
            }

            CameraFollow2D.Instance?.Shake(0.8f, 0.65f);

            // Aeron collapses to ground
            if (aeron != null)
            {
                aeron.transform.position = stasisChamberShattered.transform.position + Vector3.right * 1.5f + Vector3.down * 0.8f;
                aeron.SetState(AeronController.AeronState.Collapsed);
            }

            yield return new WaitForSeconds(1.6f);

            // 7. GUARDS RUSH IN
            UIManager.Instance?.ShowDialogue("GUARD", "CONTAIN THE SUBJECT!", 2.0f, "guard");
            yield return new WaitForSeconds(2.2f);

            // 8. COMBAT AWAKENING
            if (aeron != null) aeron.SetState(AeronController.AeronState.Idle);
            GameManager.Instance?.StartCombatWaves();
        }

        private void InstantCombatStart()
        {
            if (stasisChamberIntact != null) stasisChamberIntact.SetActive(false);
            if (stasisChamberCracked != null) stasisChamberCracked.SetActive(false);
            if (stasisChamberShattered != null) stasisChamberShattered.SetActive(true);
            if (alarmLightsGroup != null) alarmLightsGroup.SetActive(true);
            if (mainLabLight2D != null) mainLabLight2D.intensity = 0.85f;
            if (alarmLight2D != null) alarmLight2D.enabled = true;

            if (aeron != null)
            {
                aeron.transform.position = new Vector3(2.0f, -0.6f, 0f);
                aeron.SetState(AeronController.AeronState.Idle);
            }

            GameManager.Instance?.StartCombatWaves();
        }

        public void StartOutroSequence()
        {
            StartCoroutine(OutroTimelineRoutine());
        }

        private IEnumerator OutroTimelineRoutine()
        {
            if (aeron != null)
            {
                aeron.SetState(AeronController.AeronState.Idle);
            }

            yield return new WaitForSeconds(1.0f);

            UIManager.Instance?.ShowDialogue("VOICE (V.O.)", "THEY WILL FEAR YOU.", 2.5f, "voice");
            yield return new WaitForSeconds(3.0f);

            UIManager.Instance?.ShowDialogue("VOICE (V.O.)", "(PAUSE)  THEY SHOULD.", 2.8f, "voice");
            yield return new WaitForSeconds(3.5f);

            // Fade and Results
            GameManager.Instance?.TriggerResultsScreen();
        }
    }
}

