using UnityEngine;
using UnityEngine.Events;
using LastGod.Core;

namespace LastGod.Combat
{
    /// <summary>
    /// Interactive chest script that triggers Cainos chest open animation
    /// when the player approaches and interacts or touches the chest.
    /// </summary>
    public class InteractiveChest : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private ParticleSystem openParticles;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openSFX;

        [Header("State")]
        [SerializeField] private bool isOpen = false;
        [SerializeField] private bool openOnProximity = true;

        public UnityEvent OnOpened = new UnityEvent();

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isOpen) return;

            if (other.CompareTag("Player") && openOnProximity)
            {
                OpenChest();
            }
        }

        public void OpenChest()
        {
            if (isOpen) return;
            isOpen = true;

            if (animator != null)
            {
                animator.SetBool("IsOpened", true);
            }

            if (openParticles != null)
            {
                openParticles.Play();
            }

            if (audioSource != null && openSFX != null)
            {
                audioSource.PlayOneShot(openSFX);
            }

            if (CameraShake2D.Instance != null)
            {
                CameraShake2D.Instance.TriggerShake(0.15f, 0.05f);
            }

            OnOpened.Invoke();
        }
    }
}
