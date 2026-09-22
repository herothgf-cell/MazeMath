using System.Collections.Generic;
using MazeMath.Equipment;
using MazeMath.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Equipment
{
    public sealed class EquipmentServiceTests
    {
        [Test]
        public void Equip_ReplacesExistingItemInSameSlot()
        {
            var sensor1Item = Item("sensor1");
            var sensor2Item = Item("sensor2");
            var inventory = new InventoryService(new[] { sensor1Item, sensor2Item }, 12);
            inventory.TryAdd("sensor1", 1);
            inventory.TryAdd("sensor2", 1);

            var first = Equipment("sensor-a", "sensor1", EquipmentSlot.Sensor, "ability.sensor.interaction");
            var second = Equipment("sensor-b", "sensor2", EquipmentSlot.Sensor, "ability.sensor.hidden_wall");
            var service = new EquipmentService(inventory, new[] { first, second });

            Assert.IsTrue(service.Equip("sensor-a"));
            Assert.IsTrue(service.HasAbility("ability.sensor.interaction"));

            Assert.IsTrue(service.Equip("sensor-b"));
            Assert.AreEqual("sensor-b", service.GetEquipped(EquipmentSlot.Sensor));
            Assert.IsFalse(service.HasAbility("ability.sensor.interaction"));
            Assert.IsTrue(service.HasAbility("ability.sensor.hidden_wall"));
        }

        [Test]
        public void Equip_RequiresOwnedInventoryItem()
        {
            var item = Item("mining-arm");
            var inventory = new InventoryService(new[] { item }, 12);
            var definition = Equipment(
                "mining-arm-t1",
                "mining-arm",
                EquipmentSlot.ArmTool,
                "ability.break.cracked_wall");

            var service = new EquipmentService(inventory, new[] { definition });

            Assert.IsFalse(service.Equip("mining-arm-t1"));
        }

        private static ItemDefinition Item(string id)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = id;
            item.maxStack = 1;
            item.category = ItemCategory.Equipment;
            return item;
        }

        private static EquipmentDefinition Equipment(
            string id,
            string itemId,
            EquipmentSlot slot,
            string ability)
        {
            var definition = ScriptableObject.CreateInstance<EquipmentDefinition>();
            definition.equipmentId = id;
            definition.itemId = itemId;
            definition.slot = slot;
            definition.tier = 1;
            definition.abilityIds = new List<string> { ability };
            return definition;
        }
    }
}
