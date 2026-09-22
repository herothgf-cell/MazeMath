using System;
using System.Collections.Generic;
using System.Linq;
using MazeMath.Equipment;

namespace MazeMath.Enchant
{
    public sealed class EnchantService
    {
        private readonly KnowledgeXpService xp;
        private readonly IEquipmentService equipment;
        private readonly Dictionary<string, EnchantDefinition> definitions;
        private readonly HashSet<string> unlocked = new HashSet<string>();
        private readonly Dictionary<string, string> activeByEquipment =
            new Dictionary<string, string>();

        public EnchantService(
            KnowledgeXpService xp,
            IEquipmentService equipment,
            IEnumerable<EnchantDefinition> definitions)
        {
            this.xp = xp ?? throw new ArgumentNullException(nameof(xp));
            this.equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));

            this.definitions = definitions
                .Where(definition =>
                    definition != null &&
                    !string.IsNullOrWhiteSpace(definition.enchantId))
                .ToDictionary(definition => definition.enchantId, definition => definition);
        }

        public bool Unlock(string enchantId)
        {
            if (unlocked.Contains(enchantId))
                return true;

            if (!definitions.TryGetValue(enchantId, out var definition))
                return false;

            var cost = Math.Max(0, definition.knowledgeXpCost);
            if (!xp.Spend(cost))
                return false;

            unlocked.Add(enchantId);
            return true;
        }

        public bool IsUnlocked(string enchantId)
        {
            return unlocked.Contains(enchantId);
        }

        public bool EquipEnchant(string equipmentId, string enchantId)
        {
            if (!unlocked.Contains(enchantId) ||
                !definitions.TryGetValue(enchantId, out var enchant))
                return false;

            var equipmentDefinition = equipment.GetDefinition(equipmentId);
            if (equipmentDefinition == null ||
                equipmentDefinition.slot != enchant.compatibleSlot)
                return false;

            activeByEquipment[equipmentId] = enchantId;
            return true;
        }

        public string GetActiveEnchant(string equipmentId)
        {
            return activeByEquipment.TryGetValue(equipmentId, out var enchantId)
                ? enchantId
                : null;
        }

        public bool HasModifier(string modifierId)
        {
            if (string.IsNullOrWhiteSpace(modifierId))
                return false;

            foreach (var enchantId in activeByEquipment.Values)
            {
                if (definitions.TryGetValue(enchantId, out var definition) &&
                    definition.abilityModifierId == modifierId)
                    return true;
            }

            return false;
        }

        public IReadOnlyCollection<string> GetUnlocked()
        {
            return unlocked.ToArray();
        }
    }
}
