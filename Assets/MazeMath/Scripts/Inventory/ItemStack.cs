using System;

namespace MazeMath.Inventory
{
    [Serializable]
    public sealed class ItemStack
    {
        public string ItemId { get; }
        public int Count { get; private set; }

        public ItemStack(string itemId, int count)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                throw new ArgumentException("Item id must not be empty.", nameof(itemId));
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            ItemId = itemId;
            Count = count;
        }

        public void Add(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Count += amount;
        }

        public void Remove(int amount)
        {
            if (amount <= 0 || amount > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Count -= amount;
        }
    }
}
