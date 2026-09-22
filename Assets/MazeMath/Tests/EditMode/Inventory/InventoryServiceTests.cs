using System.Collections.Generic;
using MazeMath.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Inventory
{
    public sealed class InventoryServiceTests
    {
        [Test]
        public void Add_MergesStacksUpToMaxStack()
        {
            var scrap = CreateItem("scrap", 99);
            var inventory = new InventoryService(new[] { scrap }, 12);

            Assert.IsTrue(inventory.TryAdd("scrap", 80));
            Assert.IsTrue(inventory.TryAdd("scrap", 30));

            Assert.AreEqual(110, inventory.GetCount("scrap"));
            Assert.AreEqual(2, inventory.GetItems().Count);
            Assert.AreEqual(99, inventory.GetItems()[0].Count);
            Assert.AreEqual(11, inventory.GetItems()[1].Count);
        }

        [Test]
        public void Remove_MoreThanOwned_IsAtomic()
        {
            var scrap = CreateItem("scrap", 99);
            var inventory = new InventoryService(new[] { scrap }, 12);
            inventory.TryAdd("scrap", 10);

            Assert.IsFalse(inventory.TryRemove("scrap", 11));
            Assert.AreEqual(10, inventory.GetCount("scrap"));
        }

        [Test]
        public void Add_WhenCapacityInsufficient_DoesNotPartiallyAdd()
        {
            var scrap = CreateItem("scrap", 10);
            var gear = CreateItem("gear", 10);
            var inventory = new InventoryService(new[] { scrap, gear }, 1);

            Assert.IsTrue(inventory.TryAdd("scrap", 10));
            Assert.IsFalse(inventory.TryAdd("gear", 1));

            Assert.AreEqual(10, inventory.GetCount("scrap"));
            Assert.AreEqual(0, inventory.GetCount("gear"));
        }

        private static ItemDefinition CreateItem(string id, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = id;
            item.maxStack = maxStack;
            item.category = ItemCategory.Material;
            return item;
        }
    }
}
