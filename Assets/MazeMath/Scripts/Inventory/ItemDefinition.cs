using UnityEngine;

namespace MazeMath.Inventory
{
    public enum ItemCategory
    {
        Material,
        Equipment,
        Quest,
        RecipeClue
    }

    [CreateAssetMenu(menuName = "MazeMath/Items/Item")]
    public sealed class ItemDefinition : ScriptableObject
    {
        public string itemId;
        public string displayNameKey;
        public Sprite icon;
        public ItemCategory category = ItemCategory.Material;
        public int maxStack = 99;
        public string descriptionKey;
    }
}
