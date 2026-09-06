using System.Collections;
using UnityEngine;

namespace LastGod.ThirdPerson.Environment
{
    public class LabDoor : MonoBehaviour
    {
        public enum DoorType
        {
            Automatic,
            Locked,
            Security,
            Emergency
        }

        [Header("Door Config")]
        [SerializeField] private DoorType doorType = DoorType.Automatic;
        [SerializeField] private Transform leftPanel;
        [SerializeField] private Transform rightPanel;
        [SerializeField] private float slideDistance = 1.8f;
        [SerializeField] private float openSpeed = 3.5f;

        [Header("Status Indicator")]
        [SerializeField] private Light statusLight;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip doorSlideSFX;
        [SerializeField] private AudioClip doorLockedSFX;

        private bool _isOpen;
        private bool _isLocked;
        private Vector3 _leftClosedPos;
        private Vector3 _rightClosedPos;

        public bool IsOpen => _isOpen;
        public bool IsLocked => _isLocked;

        private void Awake()
        {
            if (leftPanel != null) _leftClosedPos = leftPanel.localPosition;
            if (rightPanel != null) _rightClosedPos = rightPanel.localPosition;
            _isLocked = (doorType == DoorType.Locked || doorType == DoorType.Security);
            UpdateStatusLight();
        }

        public void UnlockAndOpen()
        {
            _isLocked = false;
            UpdateStatusLight();
            Open();
        }

        public void Open()
        {
            if (_isLocked || _isOpen) return;
            _isOpen = true;

            if (audioSource != null && doorSlideSFX != null)
            {
                audioSource.PlayOneShot(doorSlideSFX);
            }

            StopAllCoroutines();
            StartCoroutine(SlideRoutine(true));
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;

            if (audioSource != null && doorSlideSFX != null)
            {
                audioSource.PlayOneShot(doorSlideSFX);
            }

            StopAllCoroutines();
            StartCoroutine(SlideRoutine(false));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.GetComponent<AI.GuardAI>() != null)
            {
                if (doorType == DoorType.Automatic && !_isLocked)
                {
                    Open();
                }
                else if (_isLocked && other.CompareTag("Player"))
                {
                    if (audioSource != null && doorLockedSFX != null)
                    {
                        audioSource.PlayOneShot(doorLockedSFX);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (doorType == DoorType.Automatic && _isOpen)
            {
                Close();
            }
        }

        private void UpdateStatusLight()
        {
            if (statusLight != null)
            {
                statusLight.color = _isLocked ? Color.red : Color.cyan;
            }
        }

        private IEnumerator SlideRoutine(bool open)
        {
            Vector3 targetLeft = open ? _leftClosedPos - Vector3.right * slideDistance : _leftClosedPos;
            Vector3 targetRight = open ? _rightClosedPos + Vector3.right * slideDistance : _rightClosedPos;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * openSpeed;
                if (leftPanel != null) leftPanel.localPosition = Vector3.Lerp(leftPanel.localPosition, targetLeft, t);
                if (rightPanel != null) rightPanel.localPosition = Vector3.Lerp(rightPanel.localPosition, targetRight, t);
                yield return null;
            }
        }
    }
}
