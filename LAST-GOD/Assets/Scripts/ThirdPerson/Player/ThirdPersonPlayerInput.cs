using UnityEngine;
using Input = UnityEngine.Input;

namespace LastGod.ThirdPerson.Player
{
    public class ThirdPersonPlayerInput : MonoBehaviour
    {
        [Header("Sensitivity & Options")]
        public float mouseSensitivity = 2.2f;
        public bool invertY = false;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool SprintPressed { get; private set; }
        public bool CrouchPressed { get; private set; }
        public bool DodgeTriggered { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool LightAttackTriggered { get; private set; }
        public bool HeavyAttackTriggered { get; private set; }
        public bool BlockPressed { get; private set; }
        public bool LockOnTriggered { get; private set; }
        public bool SurgeTriggered { get; private set; }
        public bool InteractTriggered { get; private set; }
        public bool PauseTriggered { get; private set; }

        public bool InputLocked { get; set; }

        private void Update()
        {
            try
            {
                if (InputLocked)
                {
                    MoveInput = Vector2.zero;
                    LookInput = Vector2.zero;
                    SprintPressed = false;
                    CrouchPressed = false;
                    DodgeTriggered = false;
                    JumpTriggered = false;
                    LightAttackTriggered = false;
                    HeavyAttackTriggered = false;
                    BlockPressed = false;
                    LockOnTriggered = false;
                    SurgeTriggered = false;
                    InteractTriggered = false;
                    PauseTriggered = UnityEngine.Input.GetKeyDown(KeyCode.Escape);
                    return;
                }

                // WASD / Arrow Movement
                float h = UnityEngine.Input.GetAxisRaw("Horizontal");
                float v = UnityEngine.Input.GetAxisRaw("Vertical");
                MoveInput = new Vector2(h, v).normalized;

                // Mouse Look
                float mx = UnityEngine.Input.GetAxis("Mouse X") * mouseSensitivity;
                float my = UnityEngine.Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);
                LookInput = new Vector2(mx, my);

                // Modifiers & Actions
                SprintPressed = UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift);
                CrouchPressed = UnityEngine.Input.GetKey(KeyCode.C) || UnityEngine.Input.GetKey(KeyCode.LeftControl);

                // Space can serve as Dodge in combat, or Jump when moving forward
                DodgeTriggered = UnityEngine.Input.GetKeyDown(KeyCode.Space) || UnityEngine.Input.GetKeyDown(KeyCode.LeftAlt);
                JumpTriggered = UnityEngine.Input.GetKeyDown(KeyCode.Space);

                // Attacks
                LightAttackTriggered = UnityEngine.Input.GetMouseButtonDown(0);
                HeavyAttackTriggered = UnityEngine.Input.GetMouseButtonDown(1);
                BlockPressed = UnityEngine.Input.GetKey(KeyCode.F) || UnityEngine.Input.GetKey(KeyCode.K);

                // Lock-on & Abilities
                LockOnTriggered = UnityEngine.Input.GetMouseButtonDown(2) || UnityEngine.Input.GetKeyDown(KeyCode.Tab);
                SurgeTriggered = UnityEngine.Input.GetKeyDown(KeyCode.Q) || UnityEngine.Input.GetKeyDown(KeyCode.Alpha1);

                // Interaction & System
                InteractTriggered = UnityEngine.Input.GetKeyDown(KeyCode.E);
                PauseTriggered = UnityEngine.Input.GetKeyDown(KeyCode.Escape);
            }
            catch (System.InvalidOperationException)
            {
                // Fallback if legacy input is inactive
            }
        }
    }
}
