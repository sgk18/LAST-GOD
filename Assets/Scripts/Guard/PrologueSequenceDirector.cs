using System;
using System.Collections;
using UnityEngine;
using LastGod.Core;
using LastGod.Environment;

namespace LastGod.Core
{
    /// <summary>
    /// Master narrative progression director for Act 1: Origin Guard Playable Prologue.
    /// Orchestrates radio transmissions, environmental hazard discoveries, and stasis pod approach.
    /// </summary>
    public class PrologueSequenceDirector : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RadioSystem radioSystem;
        [SerializeField] private DamagedConduitHazard conduitHazard;
        [SerializeField] private Transform guardTransform;

        [Header("Audio Squelch")]
        [SerializeField] private AudioClip radioSfx;

        private bool _conduitWarningTriggered = false;
        private bool _chamberApproached = false;

        private void Start()
        {
            if (radioSystem == null) radioSystem = RadioSystem.Instance;
            StartCoroutine(IntroRadioRoutine());
        }

        private IEnumerator IntroRadioRoutine()
        {
            yield return new WaitForSeconds(1.2f);
            Transmit("CONTROL", "Unit B-3, report.", 3.0f);

            yield return new WaitForSeconds(3.5f);
            Transmit("GUARD (B-3)", "B-3 on site. Proceeding into primary research corridor.", 4.0f);

            yield return new WaitForSeconds(4.5f);
            Transmit("CONTROL", "Movement detected in lower laboratory. Any survivors? ... Negative. Investigate power drop ahead.", 5.5f);
        }

        private void Update()
        {
            if (guardTransform == null)
            {
                var guard = GameObject.FindWithTag("Player");
                if (guard != null) guardTransform = guard.transform;
                return;
            }

            // Trigger 1: Approaching damaged conduit hazard (x > -1.5)
            if (!_conduitWarningTriggered && guardTransform.position.x > -1.5f)
            {
                _conduitWarningTriggered = true;
                Transmit("CONTROL", "Power fluctuation ahead. Damaged conduit is blocking the line. Can you bypass it?", 5.0f);
            }

            // Trigger 2: Approaching central stasis pod (x > 2.0)
            if (!_chamberApproached && guardTransform.position.x > 2.0f)
            {
                _chamberApproached = true;
                Transmit("GUARD (B-3)", "I have eyes on the central chamber. Subject A-07 is inside... The seal is cracking.", 6.0f);
            }
        }

        public void OnConduitStabilized()
        {
            Transmit("CONTROL", "Auxiliary power restored. Path clear. Proceed to central containment immediately.", 5.0f);
        }

        private void Transmit(string speaker, string text, float duration)
        {
            if (radioSystem != null)
            {
                radioSystem.Transmit(new RadioMessage(speaker, text, duration, radioSfx));
            }
        }
    }
}
