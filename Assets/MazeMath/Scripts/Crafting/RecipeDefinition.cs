using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeMath.Crafting
{
    public enum RecipeType
    {
        Guided,
        Pattern
    }

    [Serializable]
    public sealed class IngredientRequirement
    {
        public string itemId;
        public int count;
    }

    [CreateAssetMenu(menuName = "MazeMath/Crafting/Recipe")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        public string recipeId;
        public string resultItemId;
        public int resultCount = 1;
        public RecipeType type = RecipeType.Guided;
        public List<IngredientRequirement> ingredients = new List<IngredientRequirement>();
        public string requiredWorkshopTag;
        public string unlockConditionId;
    }
}
