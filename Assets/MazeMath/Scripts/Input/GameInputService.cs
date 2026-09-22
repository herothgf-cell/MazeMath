using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeMath.Input
{
    public sealed class GameInputService : MonoBehaviour, IGameInput
    {
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction interactAction;
        private InputAction mapAction;
        private InputAction inventoryAction;

        private Vector2 mobileMove;
        private bool mobileJump;
        private bool mobileInteract;
        private bool mobileMap;
        private bool mobileInventory;

        public Vector2 Move
        {
            get
            {
                var keyboardMove = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
                return mobileMove.sqrMagnitude > 0f ? mobileMove : keyboardMove;
            }
        }

        public bool JumpPressed => mobileJump || (jumpAction != null && jumpAction.WasPressedThisFrame());
        public bool InteractPressed => mobileInteract || (interactAction != null && interactAction.WasPressedThisFrame());
        public bool MapPressed => mobileMap || (mapAction != null && mapAction.WasPressedThisFrame());
        public bool InventoryPressed => mobileInventory || (inventoryAction != null && inventoryAction.WasPressedThisFrame());

        private void Awake()
        {
            BuildActions();
        }

        private void OnEnable()
        {
            moveAction?.Enable();
            jumpAction?.Enable();
            interactAction?.Enable();
            mapAction?.Enable();
            inventoryAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            jumpAction?.Disable();
            interactAction?.Disable();
            mapAction?.Disable();
            inventoryAction?.Disable();
        }

        private void LateUpdate()
        {
            mobileJump = false;
            mobileInteract = false;
            mobileMap = false;
            mobileInventory = false;
        }

        private void OnDestroy()
        {
            moveAction?.Dispose();
            jumpAction?.Dispose();
            interactAction?.Dispose();
            mapAction?.Dispose();
            inventoryAction?.Dispose();
        }

        public void SetMobileMove(Vector2 value)
        {
            mobileMove = Vector2.ClampMagnitude(value, 1f);
        }

        public void PressMobileJump()
        {
            mobileJump = true;
        }

        public void PressMobileInteract()
        {
            mobileInteract = true;
        }

        public void PressMobileMap()
        {
            mobileMap = true;
        }

        public void PressMobileInventory()
        {
            mobileInventory = true;
        }

        private void BuildActions()
        {
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");

            jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            interactAction = new InputAction("Interact", InputActionType.Button, "<Keyboard>/e");
            mapAction = new InputAction("Map", InputActionType.Button, "<Keyboard>/m");
            inventoryAction = new InputAction("Inventory", InputActionType.Button, "<Keyboard>/b");
        }
    }
}
