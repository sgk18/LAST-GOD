using System;
using UnityEngine;
using LastGod.Core;

namespace LastGod.Player.Guard
{
    /// <summary>
    /// Spy Guard NPC - Undercover intelligence operative investigating Sector 4B Cryo-Stasis Bay.
    /// Represents the "one soul" sent inside to uncover unauthorized human experiments.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpyGuardNPC : MonoBehaviour
    {
        [Header("Identity and Cover")]
        [SerializeField] private string operativeCodename = "Agent Vane";
        [SerializeField] private string coverIdentity = "Sector 4 Security Patrol // ID: B-3";
        [SerializeField] private bool isCovertOperative = true;

        [Header("Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Investigation Notes")]
        [TextArea(2, 4)]
        [SerializeField] private string[] fieldIntelLogs = new string[]
        {
            "Specimen 7... heart rate 14 BPM at -28.4 C. The neural feedback loop is accelerating.",
            "I need to extract the stasis telemetry before the shift change.",
            "If Dr. Varis finds out I have accessed the bio-lock, this entire sector goes into lethal quarantine.",
            "Cover is holding. The other guards think I am on routine perimeter patrol."
        };

        [Header("Interaction and Awareness")]
        [SerializeField] private float detectionRadius = 3.5f;

        private int currentLogIndex = 0;
        private float logTimer = 0f;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void Update()
        {
            logTimer += Time.deltaTime;
            if (logTimer > 6.0f)
            {
                logTimer = 0f;
                currentLogIndex = (currentLogIndex + 1) % fieldIntelLogs.Length;
            }
        }

        public string GetCurrentIntelLog()
        {
            if (fieldIntelLogs == null || fieldIntelLogs.Length == 0)
                return "Scanning terminal data...";
            return fieldIntelLogs[currentLogIndex];
        }

        public string GetOperativeName() => operativeCodename;
        public string GetCoverTitle() => coverIdentity;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 0.95f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
