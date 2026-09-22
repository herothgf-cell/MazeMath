using System.Collections.Generic;
using MazeMath.Enchant;
using MazeMath.Inventory;
using MazeMath.Rewards;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Rewards
{
    public sealed class RewardServiceTests
    {
        [Test]
        public void DuplicateSource_GrantsOnlyOnce()
        {
            var scrap = Item("material.scrap", 99);
            var inventory = new InventoryService(new[] { scrap }, 12);
            var xp = new KnowledgeXpService();
            var service = new RewardService(inventory, xp);

            var reward = new LearningReward(8, new[]
            {
                new ItemReward("material.scrap", 1)
            });

            Assert.AreEqual(RewardGrantResult.Granted, service.Grant("question:q1", reward));
            Assert.AreEqual(RewardGrantResult.Duplicate, service.Grant("question:q1", reward));
            Assert.AreEqual(8, xp.Balance);
            Assert.AreEqual(1, inventory.GetCount("material.scrap"));
        }

        [Test]
        public void FullInventory_QueuesRewardWithoutLosingXpOrItems()
        {
            var filler = Item("filler", 1);
            var scrap = Item("material.scrap", 99);
            var inventory = new InventoryService(new[] { filler, scrap }, 1);
            inventory.TryAdd("filler", 1);

            var xp = new KnowledgeXpService();
            var service = new RewardService(inventory, xp);
            var reward = new LearningReward(10, new[]
            {
                new ItemReward("material.scrap", 2)
            });

            Assert.AreEqual(RewardGrantResult.Pending, service.Grant("puzzle:p1", reward));
            Assert.AreEqual(0, xp.Balance);
            Assert.AreEqual(1, service.PendingCount);

            inventory.TryRemove("filler", 1);
            Assert.AreEqual(1, service.FlushPending());

            Assert.AreEqual(10, xp.Balance);
            Assert.AreEqual(2, inventory.GetCount("material.scrap"));
            Assert.AreEqual(0, service.PendingCount);
        }

        private static ItemDefinition Item(string id, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = id;
            item.maxStack = maxStack;
            item.category = ItemCategory.Material;
            return item;
        }
    }
}
