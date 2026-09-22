using System.Collections.Generic;

namespace MazeMath.Inventory
{
    public interface IInventoryService
    {
        int GetCount(string itemId);
        bool Has(string itemId, int count);
        bool TryAdd(string itemId, int count);
        bool TryRemove(string itemId, int count);
        IReadOnlyList<ItemStack> GetItems();
    }
}
