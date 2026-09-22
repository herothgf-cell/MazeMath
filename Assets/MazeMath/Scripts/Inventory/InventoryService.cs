using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeMath.Inventory
{
    public sealed class InventoryService : IInventoryService
    {
        private readonly Dictionary<string, ItemDefinition> definitions;
        private readonly List<ItemStack> stacks = new List<ItemStack>();
        private readonly int slotCapacity;

        public InventoryService(
            IEnumerable<ItemDefinition> definitions,
            int slotCapacity)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            if (slotCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));
            }

            this.definitions = definitions
                .Where(definition => definition != null)
                .ToDictionary(definition => definition.itemId, definition => definition);

            this.slotCapacity = slotCapacity;
        }

        public int GetCount(string itemId)
        {
            return stacks
                .Where(stack => stack.ItemId == itemId)
                .Sum(stack => stack.Count);
        }

        public bool Has(string itemId, int count)
        {
            return count >= 0 && GetCount(itemId) >= count;
        }

        public bool TryAdd(string itemId, int count)
        {
            if (count <= 0 || !definitions.TryGetValue(itemId, out var definition))
            {
                return false;
            }

            var maxStack = Math.Max(1, definition.maxStack);
            var existingFree = stacks
                .Where(stack => stack.ItemId == itemId)
                .Sum(stack => Math.Max(0, maxStack - stack.Count));
            var freeSlots = slotCapacity - stacks.Count;
            var totalCapacity = existingFree + freeSlots * maxStack;

            if (totalCapacity < count)
            {
                return false;
            }

            var remaining = count;

            foreach (var stack in stacks.Where(stack => stack.ItemId == itemId))
            {
                if (remaining == 0)
                {
                    break;
                }

                var canAdd = Math.Min(remaining, maxStack - stack.Count);
                if (canAdd <= 0)
                {
                    continue;
                }

                stack.Add(canAdd);
                remaining -= canAdd;
            }

            while (remaining > 0)
            {
                var amount = Math.Min(remaining, maxStack);
                stacks.Add(new ItemStack(itemId, amount));
                remaining -= amount;
            }

            return true;
        }

        public bool TryRemove(string itemId, int count)
        {
            if (count <= 0 || GetCount(itemId) < count)
            {
                return false;
            }

            var remaining = count;
            for (var index = stacks.Count - 1; index >= 0 && remaining > 0; index--)
            {
                var stack = stacks[index];
                if (stack.ItemId != itemId)
                {
                    continue;
                }

                var remove = Math.Min(remaining, stack.Count);
                stack.Remove(remove);
                remaining -= remove;

                if (stack.Count == 0)
                {
                    stacks.RemoveAt(index);
                }
            }

            return true;
        }

        public IReadOnlyList<ItemStack> GetItems()
        {
            return stacks;
        }
    }
}
