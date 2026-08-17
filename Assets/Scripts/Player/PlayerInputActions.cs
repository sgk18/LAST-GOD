// GENERATED — mirrors Assets/Settings/Input/PlayerInputActions.inputactions
// Namespace : LastGod.Input  (matches README Step 2)
// Do NOT edit by hand — regenerate via the .inputactions Inspector if you
// change bindings.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace LastGod.Input
{
    /// <summary>
    /// Strongly-typed wrapper around the PlayerInputActions .inputactions asset.
    /// Generated to match the binding layout in
    /// Assets/Settings/Input/PlayerInputActions.inputactions.
    /// </summary>
    public partial class PlayerInputActions : IInputActionCollection2, IDisposable
    {
        // ─── Asset reference ──────────────────────────────────────────────────
        public InputActionAsset asset { get; }

        // ─── Constructor ──────────────────────────────────────────────────────
        public PlayerInputActions()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInputActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""a1b2c3d4-e5f6-7890-abcd-ef1234567890"",
            ""actions"": [
                { ""name"": ""Move"",   ""type"": ""Value"",  ""id"": ""11111111-0000-0000-0000-000000000001"", ""expectedControlType"": ""Vector2"", ""processors"": """", ""interactions"": """", ""singletonActionBindings"": [] },
                { ""name"": ""Jump"",   ""type"": ""Button"", ""id"": ""11111111-0000-0000-0000-000000000002"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""singletonActionBindings"": [] },
                { ""name"": ""Attack"", ""type"": ""Button"", ""id"": ""11111111-0000-0000-0000-000000000003"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""singletonActionBindings"": [] },
                { ""name"": ""Dash"",   ""type"": ""Button"", ""id"": ""11111111-0000-0000-0000-000000000004"", ""expectedControlType"": ""Button"",  ""processors"": """", ""interactions"": """", ""singletonActionBindings"": [] }
            ],
            ""bindings"": [
                { ""name"": """",      ""id"": ""22222222-0000-0000-0000-000000000001"", ""path"": ""<Gamepad>/leftStick"",    ""interactions"": """", ""processors"": ""StickDeadzone"", ""groups"": ""Gamepad"",   ""action"": ""Move"",   ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": ""WASD"",  ""id"": ""22222222-0000-0000-0000-000000000002"", ""path"": ""2DVector"",               ""interactions"": """", ""processors"": """",              ""groups"": """",         ""action"": ""Move"",   ""isComposite"": true,  ""isPartOfComposite"": false },
                { ""name"": ""up"",    ""id"": ""22222222-0000-0000-0000-000000000003"", ""path"": ""<Keyboard>/w"",           ""interactions"": """", ""processors"": """",              ""groups"": ""Keyboard"", ""action"": ""Move"",   ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": ""down"",  ""id"": ""22222222-0000-0000-0000-000000000004"", ""path"": ""<Keyboard>/s"",           ""interactions"": """", ""processors"": """",              ""groups"": ""Keyboard"", ""action"": ""Move"",   ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": ""left"",  ""id"": ""22222222-0000-0000-0000-000000000005"", ""path"": ""<Keyboard>/a"",           ""interactions"": """", ""processors"": """",              ""groups"": ""Keyboard"", ""action"": ""Move"",   ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": ""right"", ""id"": ""22222222-0000-0000-0000-000000000006"", ""path"": ""<Keyboard>/d"",           ""interactions"": """", ""processors"": """",              ""groups"": ""Keyboard"", ""action"": ""Move"",   ""isComposite"": false, ""isPartOfComposite"": true },
                { ""name"": """",      ""id"": ""22222222-0000-0000-0000-000000000007"", ""path"": ""<Gamepad>/dpad"",         ""interactions"": """", ""processors"": """",              ""groups"": ""Gamepad"",  ""action"": ""Move"",   ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """",      ""id"": ""33333333-0000-0000-0000-000000000001"", ""path"": ""<Keyboard>/space"",       ""interactions"": """", ""processors"": """",              ""groups"": ""Keyboard"", ""action"": ""Jump"",   ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """",      ""id"": ""33333333-0000-0000-0000-000000000002"", ""path"": ""<Gamepad>/buttonSouth"",  ""interactions"": """", ""processors"": """",              ""groups"": ""Gamepad"",  ""action"": ""Jump"",   ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """",      ""id"": ""44444444-0000-0000-0000-000000000001"", ""path"": ""<Keyboard>/j"",           ""interactions"": """", ""processors"": """",              ""groups"": ""Keyboard"", ""action"": ""Attack"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """",      ""id"": ""44444444-0000-0000-0000-000000000002"", ""path"": ""<Gamepad>/buttonWest"",   ""interactions"": """", ""processors"": """",              ""groups"": ""Gamepad"",  ""action"": ""Attack"", ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """",      ""id"": ""55555555-0000-0000-0000-000000000001"", ""path"": ""<Keyboard>/leftShift"",   ""interactions"": """", ""processors"": """",              ""groups"": ""Keyboard"", ""action"": ""Dash"",   ""isComposite"": false, ""isPartOfComposite"": false },
                { ""name"": """",      ""id"": ""55555555-0000-0000-0000-000000000002"", ""path"": ""<Gamepad>/buttonEast"",   ""interactions"": """", ""processors"": """",              ""groups"": ""Gamepad"",  ""action"": ""Dash"",   ""isComposite"": false, ""isPartOfComposite"": false }
            ]
        }
    ],
    ""controlSchemes"": [
        { ""name"": ""Keyboard"", ""bindingGroup"": ""Keyboard"", ""devices"": [ { ""devicePath"": ""<Keyboard>"", ""isOptional"": false, ""isOR"": false } ] },
        { ""name"": ""Gamepad"",  ""bindingGroup"": ""Gamepad"",  ""devices"": [ { ""devicePath"": ""<Gamepad>"",  ""isOptional"": false, ""isOR"": false } ] }
    ]
}");
            // Cache map
            _Player = asset.FindActionMap("Player", throwIfNotFound: true);
            _Player_Move   = _Player.FindAction("Move",   throwIfNotFound: true);
            _Player_Jump   = _Player.FindAction("Jump",   throwIfNotFound: true);
            _Player_Attack = _Player.FindAction("Attack", throwIfNotFound: true);
            _Player_Dash   = _Player.FindAction("Dash",   throwIfNotFound: true);
        }

        ~PlayerInputActions() => Dispose();

        // ─── IInputActionCollection2 ──────────────────────────────────────────
        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action) => asset.Contains(action);

        public IEnumerator<InputAction> GetEnumerator() => asset.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Enable()  => asset.Enable();
        public void Disable() => asset.Disable();

        public IEnumerable<InputBinding> bindings => asset.bindings;

        public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
            => asset.FindAction(actionNameOrId, throwIfNotFound);

        public int FindBinding(InputBinding bindingMask, out InputAction action)
            => asset.FindBinding(bindingMask, out action);

        // ─── Player action map ────────────────────────────────────────────────
        private InputActionMap _Player;
        private InputAction    _Player_Move;
        private InputAction    _Player_Jump;
        private InputAction    _Player_Attack;
        private InputAction    _Player_Dash;

        private IPlayerActions _PlayerActionsCallbackInterface;

        /// <summary>Strongly-typed Player action map accessor.</summary>
        public PlayerActions Player => new PlayerActions(this);

        public struct PlayerActions
        {
            private readonly PlayerInputActions _wrapper;
            public PlayerActions(PlayerInputActions wrapper) { _wrapper = wrapper; }

            // Actions
            public InputAction Move   => _wrapper._Player_Move;
            public InputAction Jump   => _wrapper._Player_Jump;
            public InputAction Attack => _wrapper._Player_Attack;
            public InputAction Dash   => _wrapper._Player_Dash;

            public InputActionMap Get() => _wrapper._Player;

            public void Enable()  => Get().Enable();
            public void Disable() => Get().Disable();

            public bool enabled => Get().enabled;

            public void SetCallbacks(IPlayerActions instance)
            {
                if (_wrapper._PlayerActionsCallbackInterface != null)
                {
                    var prev = _wrapper._PlayerActionsCallbackInterface;
                    Move.started   -= prev.OnMove;
                    Move.performed -= prev.OnMove;
                    Move.canceled  -= prev.OnMove;
                    Jump.started   -= prev.OnJump;
                    Jump.performed -= prev.OnJump;
                    Jump.canceled  -= prev.OnJump;
                    Attack.started   -= prev.OnAttack;
                    Attack.performed -= prev.OnAttack;
                    Attack.canceled  -= prev.OnAttack;
                    Dash.started   -= prev.OnDash;
                    Dash.performed -= prev.OnDash;
                    Dash.canceled  -= prev.OnDash;
                }
                _wrapper._PlayerActionsCallbackInterface = instance;
                if (instance != null)
                {
                    Move.started   += instance.OnMove;
                    Move.performed += instance.OnMove;
                    Move.canceled  += instance.OnMove;
                    Jump.started   += instance.OnJump;
                    Jump.performed += instance.OnJump;
                    Jump.canceled  += instance.OnJump;
                    Attack.started   += instance.OnAttack;
                    Attack.performed += instance.OnAttack;
                    Attack.canceled  += instance.OnAttack;
                    Dash.started   += instance.OnDash;
                    Dash.performed += instance.OnDash;
                    Dash.canceled  += instance.OnDash;
                }
            }
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
            GC.SuppressFinalize(this);
        }
    }

    // ─── Callback interface ───────────────────────────────────────────────────
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnAttack(InputAction.CallbackContext context);
        void OnDash(InputAction.CallbackContext context);
    }
}
