using System.Collections;
using UnityEngine;

namespace LastGod.ThirdPerson.Player
{
    public class ThirdPersonPlayerAnimator : MonoBehaviour
    {
        [Header("Rig Nodes (Optional Humanoid Links)")]
        [SerializeField] private Transform torso;
        [SerializeField] private Transform head;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightLeg;
        [SerializeField] private Transform leftLeg;

        [Header("Visual Mesh & Materials")]
        [SerializeField] private Renderer characterRenderer;

        private Animator _animator;
        private Vector3 _baseTorsoPos;
        private Quaternion _baseRightArmRot;
        private Quaternion _baseLeftArmRot;
        private Quaternion _baseRightLegRot;
        private Quaternion _baseLeftLegRot;

        private float _walkCycle;
        private bool _isAttacking;

        private void Awake()
        {
            _animator = GetComponent<Animator>();

            if (rightArm != null) _baseRightArmRot = rightArm.localRotation;
            if (leftArm != null) _baseLeftArmRot = leftArm.localRotation;
            if (rightLeg != null) _baseRightLegRot = rightLeg.localRotation;
            if (leftLeg != null) _baseLeftLegRot = leftLeg.localRotation;
            if (torso != null) _baseTorsoPos = torso.localPosition;
        }

        public void SetMovementState(float forwardSpeed, float lateralSpeed, bool isGrounded, bool isCrouching, bool isSprinting)
        {
            if (_animator != null && _animator.isHuman)
            {
                _animator.SetFloat("Speed", forwardSpeed);
                _animator.SetBool("IsGrounded", isGrounded);
                _animator.SetBool("IsCrouching", isCrouching);
                _animator.SetBool("IsSprinting", isSprinting);
            }

            // Procedural walk/idle limb movement
            if (!_isAttacking)
            {
                float speed = Mathf.Sqrt(forwardSpeed * forwardSpeed + lateralSpeed * lateralSpeed);
                if (speed > 0.1f)
                {
                    _walkCycle += Time.deltaTime * speed * (isSprinting ? 12f : 8f);
                    float legSwing = Mathf.Sin(_walkCycle) * 35f;
                    float armSwing = -legSwing * 0.7f;

                    if (rightLeg != null) rightLeg.localRotation = _baseRightLegRot * Quaternion.Euler(legSwing, 0f, 0f);
                    if (leftLeg != null) leftLeg.localRotation = _baseLeftLegRot * Quaternion.Euler(-legSwing, 0f, 0f);
                    if (rightArm != null) rightArm.localRotation = _baseRightArmRot * Quaternion.Euler(armSwing, 0f, 0f);
                    if (leftArm != null) leftArm.localRotation = _baseLeftArmRot * Quaternion.Euler(-armSwing, 0f, 0f);
                }
                else
                {
                    // Subtle unnatural stasis breathing
                    float breathe = Mathf.Sin(Time.time * 2.5f) * 0.02f;
                    if (torso != null) torso.localPosition = _baseTorsoPos + Vector3.up * breathe;
                    if (rightArm != null) rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, _baseRightArmRot, Time.deltaTime * 6f);
                    if (leftArm != null) leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, _baseLeftArmRot, Time.deltaTime * 6f);
                    if (rightLeg != null) rightLeg.localRotation = Quaternion.Slerp(rightLeg.localRotation, _baseRightLegRot, Time.deltaTime * 6f);
                    if (leftLeg != null) leftLeg.localRotation = Quaternion.Slerp(leftLeg.localRotation, _baseLeftLegRot, Time.deltaTime * 6f);
                }
            }
        }

        public void PlayLightAttack(int comboStep)
        {
            StopAllCoroutines();
            StartCoroutine(PerformComboStep(comboStep));
        }

        public void PlayHeavyAttack()
        {
            StopAllCoroutines();
            StartCoroutine(PerformHeavyStrike());
        }

        public void PlayDodge()
        {
            StopAllCoroutines();
            StartCoroutine(PerformDodgeRoll());
        }

        private IEnumerator PerformComboStep(int step)
        {
            _isAttacking = true;
            float duration = 0.22f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = t / duration;
                float punchFactor = Mathf.Sin(progress * Mathf.PI);

                if (step == 1) // Fast straight punch
                {
                    if (rightArm != null)
                        rightArm.localRotation = _baseRightArmRot * Quaternion.Euler(-80f * punchFactor, 25f * punchFactor, 0f);
                    if (torso != null)
                        torso.localRotation = Quaternion.Euler(0f, 20f * punchFactor, 0f);
                }
                else if (step == 2) // Precision elbow strike
                {
                    if (leftArm != null)
                        leftArm.localRotation = _baseLeftArmRot * Quaternion.Euler(-75f * punchFactor, -35f * punchFactor, 20f * punchFactor);
                    if (torso != null)
                        torso.localRotation = Quaternion.Euler(0f, -25f * punchFactor, 0f);
                }
                else // Brutal roundhouse kick
                {
                    if (rightLeg != null)
                        rightLeg.localRotation = _baseRightLegRot * Quaternion.Euler(-70f * punchFactor, 45f * punchFactor, -30f * punchFactor);
                    if (torso != null)
                        torso.localRotation = Quaternion.Euler(0f, 40f * punchFactor, -10f * punchFactor);
                }

                yield return null;
            }

            _isAttacking = false;
        }

        private IEnumerator PerformHeavyStrike()
        {
            _isAttacking = true;
            float duration = 0.42f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = t / duration;
                float windup = Mathf.Sin(progress * Mathf.PI);

                if (rightArm != null)
                    rightArm.localRotation = _baseRightArmRot * Quaternion.Euler(-110f * windup, 30f * windup, 0f);
                if (torso != null)
                    torso.localRotation = Quaternion.Euler(-15f * windup, 35f * windup, 0f);

                yield return null;
            }

            _isAttacking = false;
        }

        private IEnumerator PerformDodgeRoll()
        {
            _isAttacking = true;
            float duration = 0.35f;
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = t / duration;
                float tuck = Mathf.Sin(progress * Mathf.PI);

                if (torso != null)
                {
                    torso.localPosition = _baseTorsoPos - Vector3.up * (0.5f * tuck);
                    torso.localRotation = Quaternion.Euler(360f * progress, 0f, 0f);
                }

                yield return null;
            }

            if (torso != null)
            {
                torso.localPosition = _baseTorsoPos;
                torso.localRotation = Quaternion.identity;
            }
            _isAttacking = false;
        }
    }
}
