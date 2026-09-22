using MazeMath.Equipment;
using UnityEngine;

namespace MazeMath.Enchant
{
    [CreateAssetMenu(menuName = "MazeMath/Enchant/Definition")]
    public sealed class EnchantDefinition : ScriptableObject
    {
        public string enchantId;
        public string displayNameKey;
        public string descriptionKey;
        public EquipmentSlot compatibleSlot;
        public int level = 1;
        public int knowledgeXpCost = 10;
        public string abilityModifierId;
    }
}
