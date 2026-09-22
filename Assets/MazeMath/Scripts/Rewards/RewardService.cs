using System;
using System.Collections.Generic;
using MazeMath.Enchant;
using MazeMath.Inventory;

namespace MazeMath.Rewards
{
    public sealed class RewardService : IRewardService
    {
        private sealed class PendingEntry
        {
            public string SourceId;
            public LearningReward Reward;
        }

        private readonly IInventoryService inventory;
        private readonly KnowledgeXpService xp;
        private readonly HashSet<string> processedSources = new HashSet<string>();
        private readonly HashSet<string> pendingSources = new HashSet<string>();
        private readonly List<PendingEntry> pending = new List<PendingEntry>();

        public int PendingCount => pending.Count;

        public RewardService(IInventoryService inventory, KnowledgeXpService xp)
        {
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            this.xp = xp ?? throw new ArgumentNullException(nameof(xp));
        }

        public RewardGrantResult Grant(string sourceId, LearningReward reward)
        {
            if (string.IsNullOrWhiteSpace(sourceId) || reward == null)
                return RewardGrantResult.Invalid;

            if (processedSources.Contains(sourceId) || pendingSources.Contains(sourceId))
                return RewardGrantResult.Duplicate;

            if (TryGrantNow(reward))
            {
                processedSources.Add(sourceId);
                return RewardGrantResult.Granted;
            }

            pending.Add(new PendingEntry { SourceId = sourceId, Reward = reward });
            pendingSources.Add(sourceId);
            return RewardGrantResult.Pending;
        }

        public int FlushPending()
        {
            var granted = 0;
            for (var i = pending.Count - 1; i >= 0; i--)
            {
                var entry = pending[i];
                if (!TryGrantNow(entry.Reward))
                    continue;

                pending.RemoveAt(i);
                pendingSources.Remove(entry.SourceId);
                processedSources.Add(entry.SourceId);
                granted++;
            }

            return granted;
        }

        private bool TryGrantNow(LearningReward reward)
        {
            var added = new List<ItemReward>();

            foreach (var item in reward.Items)
            {
                if (!inventory.TryAdd(item.ItemId, item.Count))
                {
                    RollbackItems(added);
                    return false;
                }

                added.Add(item);
            }

            xp.Add(reward.KnowledgeXp);
            return true;
        }

        private void RollbackItems(List<ItemReward> added)
        {
            for (var i = added.Count - 1; i >= 0; i--)
            {
                var item = added[i];
                if (!inventory.TryRemove(item.ItemId, item.Count))
                    throw new InvalidOperationException("Reward rollback failed for " + item.ItemId);
            }
        }
    }
}
