using UnityEngine;

namespace MazeMath.Input
{
    public interface IGameInput
    {
        Vector2 Move { get; }
        bool JumpPressed { get; }
        bool InteractPressed { get; }
        bool MapPressed { get; }
        bool InventoryPressed { get; }
    }
}
