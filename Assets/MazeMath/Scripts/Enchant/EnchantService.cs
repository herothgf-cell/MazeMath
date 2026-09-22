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
        private readonly Dictionary<string, string> activeByEquipment = new Dictionary<string, string>();

        public EnchantService(KnowledgeXpService xp, IEquipmentService equipment, IEnumerable<EnchantDefinition> definitions)
        {
            this.xp = xp ?? throw new ArgumentNullException(nameof(xp));
            this.equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            this.definitions = definitions.Where(d => d != null && !string.IsNullOrWhiteSpace(d.enchantId)).ToDictionary(d => d.enchantId, d => d);
        }
        public bool Unlock(string enchantId)
        {
            if (unlocked.Contains(enchantId)) return true;
            if (!definitions.TryGetValue(enchantId, out var definition)) return false;
            if (!xp.Spend(Math.Max(0, definition.knowledgeXpCost))) return false;
            unlocked.Add(enchantId); return true;
        }
        public bool IsUnlocked(string enchantId) { return unlocked.Contains(enchantId); }
        public bool EquipEnchant(string equipmentId, string enchantId)
        {
            if (!unlocked.Contains(enchantId) || !definitions.TryGetValue(enchantId, out var enchant)) return false;
            var definition = equipment.GetDefinition(equipmentId);
            if (definition == null || definition.slot != enchant.compatibleSlot) return false;
            activeByEquipment[equipmentId] = enchantId; return true;
        }
        public string GetActiveEnchant(string equipmentId)
        {
            return activeByEquipment.TryGetValue(equipmentId, out var id) ? id : null;
        }
        public bool HasModifier(string modifierId)
        {
            if (string.IsNullOrWhiteSpace(modifierId)) return false;
            foreach (var pair in activeByEquipment)
            {
                var tool = equipment.GetDefinition(pair.Key);
                if (tool == null || equipment.GetEquipped(tool.slot) != pair.Key) continue;
                if (definitions.TryGetValue(pair.Value, out var enchant) && enchant.abilityModifierId == modifierId) return true;
            }
            return false;
        }
        public IReadOnlyCollection<string> GetUnlocked() { return unlocked.ToArray(); }
    }
}
