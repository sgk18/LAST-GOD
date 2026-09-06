using UnityEngine;

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
                PauseTriggered = Input.GetKeyDown(KeyCode.Escape);
                return;
            }

            // WASD / Arrow Movement
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            MoveInput = new Vector2(h, v).normalized;

            // Mouse Look
            float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
            float my = Input.GetAxis("Mouse Y") * mouseSensitivity * (invertY ? 1f : -1f);
            LookInput = new Vector2(mx, my);

            // Modifiers & Actions
            SprintPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            CrouchPressed = Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl);

            // Space can serve as Dodge in combat, or Jump when moving forward
            DodgeTriggered = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftAlt);
            JumpTriggered = Input.GetKeyDown(KeyCode.Space);

            // Attacks
            LightAttackTriggered = Input.GetMouseButtonDown(0);
            HeavyAttackTriggered = Input.GetMouseButtonDown(1);
            BlockPressed = Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.K);

            // Lock-on & Abilities
            LockOnTriggered = Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.Tab);
            SurgeTriggered = Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Alpha1);

            // Interaction & System
            InteractTriggered = Input.GetKeyDown(KeyCode.E);
            PauseTriggered = Input.GetKeyDown(KeyCode.Escape);
        }
    }
}
