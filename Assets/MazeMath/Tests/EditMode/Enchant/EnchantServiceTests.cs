using System.Collections.Generic;
using MazeMath.Enchant;
using MazeMath.Equipment;
using MazeMath.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Enchant
{
    public sealed class EnchantServiceTests
    {
        [Test]
        public void Unlock_WithInsufficientXp_DoesNotSpend()
        {
            var fixture = CreateFixture(startXp: 5);
            var enchant = Enchant("memory", EquipmentSlot.Sensor, 10, "modifier.sensor.memory");
            var service = new EnchantService(fixture.xp, fixture.equipment, new[] { enchant });

            Assert.IsFalse(service.Unlock("memory"));
            Assert.AreEqual(5, fixture.xp.Balance);
            Assert.IsFalse(service.IsUnlocked("memory"));
        }

        [Test]
        public void UnlockedEnchant_CanBeReequippedForFree()
        {
            var fixture = CreateFixture(startXp: 30);
            var memory = Enchant("memory", EquipmentSlot.Sensor, 10, "modifier.sensor.memory");
            var guide = Enchant("guide", EquipmentSlot.Sensor, 10, "modifier.sensor.guide");
            var service = new EnchantService(
                fixture.xp,
                fixture.equipment,
                new[] { memory, guide });

            Assert.IsTrue(service.Unlock("memory"));
            Assert.IsTrue(service.Unlock("guide"));
            Assert.AreEqual(10, fixture.xp.Balance);

            Assert.IsTrue(service.EquipEnchant("sensor-eq", "memory"));
            Assert.IsTrue(service.EquipEnchant("sensor-eq", "guide"));
            Assert.IsTrue(service.EquipEnchant("sensor-eq", "memory"));

            Assert.AreEqual(10, fixture.xp.Balance);
            Assert.AreEqual("memory", service.GetActiveEnchant("sensor-eq"));
            Assert.IsTrue(service.HasModifier("modifier.sensor.memory"));
        }

        [Test]
        public void Enchant_MustMatchEquipmentSlot()
        {
            var fixture = CreateFixture(startXp: 20);
            var armEnchant = Enchant("echo", EquipmentSlot.ArmTool, 5, "modifier.arm.echo");
            var service = new EnchantService(fixture.xp, fixture.equipment, new[] { armEnchant });
            service.Unlock("echo");

            Assert.IsFalse(service.EquipEnchant("sensor-eq", "echo"));
        }

        private static (KnowledgeXpService xp, EquipmentService equipment) CreateFixture(int startXp)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = "sensor-item";
            item.maxStack = 1;
            item.category = ItemCategory.Equipment;

            var inventory = new InventoryService(new[] { item }, 12);
            inventory.TryAdd("sensor-item", 1);

            var equipmentDefinition = ScriptableObject.CreateInstance<EquipmentDefinition>();
            equipmentDefinition.equipmentId = "sensor-eq";
            equipmentDefinition.itemId = "sensor-item";
            equipmentDefinition.slot = EquipmentSlot.Sensor;
            equipmentDefinition.tier = 1;

            var equipment = new EquipmentService(inventory, new[] { equipmentDefinition });
            equipment.Equip("sensor-eq");

            var xp = new KnowledgeXpService();
            xp.Add(startXp);
            return (xp, equipment);
        }

        private static EnchantDefinition Enchant(
            string id,
            EquipmentSlot slot,
            int cost,
            string modifier)
        {
            var enchant = ScriptableObject.CreateInstance<EnchantDefinition>();
            enchant.enchantId = id;
            enchant.compatibleSlot = slot;
            enchant.knowledgeXpCost = cost;
            enchant.abilityModifierId = modifier;
            return enchant;
        }
    }
}
