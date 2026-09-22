using MazeMath.Input;
using UnityEngine;

namespace MazeMath.UI.Mobile
{
    public sealed class MobileControlBridge : MonoBehaviour
    {
        [SerializeField] private GameInputService inputService;

        public void Bind(GameInputService service)
        {
            inputService = service;
        }

        public void MoveLeftDown()
        {
            inputService?.SetMobileMove(Vector2.left);
        }

        public void MoveRightDown()
        {
            inputService?.SetMobileMove(Vector2.right);
        }

        public void MoveUpDown()
        {
            inputService?.SetMobileMove(Vector2.up);
        }

        public void MoveDownDown()
        {
            inputService?.SetMobileMove(Vector2.down);
        }

        public void MoveStop()
        {
            inputService?.SetMobileMove(Vector2.zero);
        }

        public void PressJump()
        {
            inputService?.PressMobileJump();
        }

        public void PressInteract()
        {
            inputService?.PressMobileInteract();
        }

        public void PressMap()
        {
            inputService?.PressMobileMap();
        }

        public void PressInventory()
        {
            inputService?.PressMobileInventory();
        }
    }
}
