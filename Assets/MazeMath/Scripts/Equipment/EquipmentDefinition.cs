using System.Collections.Generic;
using UnityEngine;

namespace MazeMath.Equipment
{
    public enum EquipmentSlot
    {
        ArmTool,
        UtilityTool,
        Mobility,
        Defense,
        Sensor
    }

    [CreateAssetMenu(menuName = "MazeMath/Equipment/Definition")]
    public sealed class EquipmentDefinition : ScriptableObject
    {
        public string equipmentId;
        public string itemId;
        public string displayNameKey;
        public EquipmentSlot slot;
        public int tier = 1;
        public Sprite icon;
        public List<string> abilityIds = new List<string>();
        public List<string> availableEnchantIds = new List<string>();
        public string upgradeRecipeId;
    }
}
