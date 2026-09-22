using MazeMath.Enchant;
using MazeMath.Equipment;
using MazeMath.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Adventure
{
    public sealed class LegacyEnchantRegressionTests
    {
        [Test] public void UnequippingToolDisablesLegacyEnchantModifier()
        {
            var item=ScriptableObject.CreateInstance<ItemDefinition>();
            var tool=ScriptableObject.CreateInstance<EquipmentDefinition>();
            var enchant=ScriptableObject.CreateInstance<EnchantDefinition>();
            try
            {
                item.itemId="sensor"; item.maxStack=1; item.category=ItemCategory.Equipment;
                tool.equipmentId="sensor.eq"; tool.itemId=item.itemId; tool.slot=EquipmentSlot.Sensor;
                enchant.enchantId="memory"; enchant.compatibleSlot=EquipmentSlot.Sensor;
                enchant.knowledgeXpCost=10; enchant.abilityModifierId="memory.effect";
                var inventory=new InventoryService(new[]{item},12); inventory.TryAdd(item.itemId,1);
                var equipment=new EquipmentService(inventory,new[]{tool}); equipment.Equip(tool.equipmentId);
                var xp=new KnowledgeXpService(); xp.Add(10);
                var service=new EnchantService(xp,equipment,new[]{enchant}); service.Unlock(enchant.enchantId);
                service.EquipEnchant(tool.equipmentId,enchant.enchantId);
                Assert.IsTrue(service.HasModifier("memory.effect"));
                equipment.Unequip(EquipmentSlot.Sensor);
                Assert.IsFalse(service.HasModifier("memory.effect"));
            }
            finally { Object.DestroyImmediate(item); Object.DestroyImmediate(tool); Object.DestroyImmediate(enchant); }
        }
    }
}
