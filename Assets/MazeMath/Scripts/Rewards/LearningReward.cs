using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeMath.Rewards
{
    [Serializable]
    public sealed class ItemReward
    {
        public string ItemId { get; }
        public int Count { get; }

        public ItemReward(string itemId, int count)
        {
            if (string.IsNullOrWhiteSpace(itemId))
                throw new ArgumentException("Item id is required.", nameof(itemId));
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            ItemId = itemId;
            Count = count;
        }
    }

    [Serializable]
    public sealed class LearningReward
    {
        public int KnowledgeXp { get; }
        public IReadOnlyList<ItemReward> Items { get; }

        public LearningReward(int knowledgeXp, IEnumerable<ItemReward> items = null)
        {
            if (knowledgeXp < 0)
                throw new ArgumentOutOfRangeException(nameof(knowledgeXp));

            KnowledgeXp = knowledgeXp;
            Items = items == null
                ? Array.Empty<ItemReward>()
                : items.Where(item => item != null).ToArray();
        }
    }
}
