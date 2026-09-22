using System;
using System.Collections.Generic;
using System.Linq;
using MazeMath.Inventory;

namespace MazeMath.Equipment
{
    public sealed class EquipmentService : IEquipmentService
    {
        private readonly IInventoryService inventory;
        private readonly Dictionary<string, EquipmentDefinition> definitions;
        private readonly Dictionary<EquipmentSlot, string> equipped =
            new Dictionary<EquipmentSlot, string>();

        public EquipmentService(
            IInventoryService inventory,
            IEnumerable<EquipmentDefinition> definitions)
        {
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));

            this.definitions = definitions
                .Where(definition =>
                    definition != null &&
                    !string.IsNullOrWhiteSpace(definition.equipmentId))
                .ToDictionary(definition => definition.equipmentId, definition => definition);
        }

        public string GetEquipped(EquipmentSlot slot)
        {
            return equipped.TryGetValue(slot, out var equipmentId)
                ? equipmentId
                : null;
        }

        public bool Equip(string equipmentId)
        {
            if (!definitions.TryGetValue(equipmentId, out var definition))
                return false;

            if (string.IsNullOrWhiteSpace(definition.itemId) ||
                !inventory.Has(definition.itemId, 1))
                return false;

            equipped[definition.slot] = equipmentId;
            return true;
        }

        public bool Unequip(EquipmentSlot slot)
        {
            return equipped.Remove(slot);
        }

        public bool HasAbility(string abilityId)
        {
            if (string.IsNullOrWhiteSpace(abilityId))
                return false;

            foreach (var pair in equipped)
            {
                if (!definitions.TryGetValue(pair.Value, out var definition))
                    continue;

                if (definition.abilityIds != null &&
                    definition.abilityIds.Contains(abilityId))
                    return true;
            }

            return false;
        }

        public int GetEquipmentTier(string equipmentId)
        {
            return definitions.TryGetValue(equipmentId, out var definition)
                ? definition.tier
                : 0;
        }

        public EquipmentDefinition GetDefinition(string equipmentId)
        {
            definitions.TryGetValue(equipmentId, out var definition);
            return definition;
        }

        public IReadOnlyCollection<EquipmentDefinition> GetAllDefinitions()
        {
            return definitions.Values.ToArray();
        }
    }
}
