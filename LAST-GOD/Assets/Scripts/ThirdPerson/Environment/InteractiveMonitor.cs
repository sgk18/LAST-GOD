using System.Collections;
using UnityEngine;

namespace LastGod.ThirdPerson.Environment
{
    public class InteractiveMonitor : MonoBehaviour
    {
        public enum MonitorContent
        {
            SubjectStatus,
            ProjectAscension,
            ContainmentFailure,
            UnknownSignal,
            PhaseOneInitiated
        }

        [Header("Display Settings")]
        [SerializeField] private MonitorContent content = MonitorContent.ProjectAscension;
        [SerializeField] private TextMesh worldTextMesh;
        [SerializeField] private Renderer screenMeshRenderer;
        [SerializeField] private Light screenGlowLight;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip terminalHumSFX;

        private void Start()
        {
            UpdateScreenText();
        }

        public void SetContent(MonitorContent newContent)
        {
            content = newContent;
            UpdateScreenText();
        }

        public void UpdateScreenText()
        {
            string displayText = content switch
            {
                MonitorContent.SubjectStatus => "=== SUBJECT STATUS ===\nDESIGNATION: AERON\nORIGIN: CLASSIFIED\nCELL STABILITY: CRITICAL\nSTATUS: SUSPENDED",
                MonitorContent.ProjectAscension => "=== PROJECT ASCENSION ===\nOBJECTIVE: DEICIDE\nTARGET ENTITIES: 07\nVITAL SYNTHESIS: ACTIVE\nWARNING: DIVINE RESIDUE DETECTED",
                MonitorContent.ContainmentFailure => "[CRITICAL ALERT]\nCONTAINMENT POD 01 BREACHED\nSUBJECT IS UNRESTRICTED\nALL PERSONNEL INITIATE LOCKDOWN",
                MonitorContent.UnknownSignal => ">>> UNKNOWN SIGNAL <<<\nFREQ: 0.000 Hz (INTERNAL)\nSOURCE: UNMAPPED\nMESSAGE: 'THEY ARE COMING'",
                MonitorContent.PhaseOneInitiated => "==========================\nPROJECT ASCENSION\nSUBJECT: AERON\nSTATUS: AWAKE\nUNKNOWN: PHASE ONE INITIATED\n==========================",
                _ => "SYSTEM NORMAL"
            };

            if (worldTextMesh != null)
            {
                worldTextMesh.text = displayText;
            }

            if (screenGlowLight != null)
            {
                screenGlowLight.color = content == MonitorContent.ContainmentFailure ? Color.red : new Color(0.2f, 0.8f, 1.0f);
            }
        }
    }
}
