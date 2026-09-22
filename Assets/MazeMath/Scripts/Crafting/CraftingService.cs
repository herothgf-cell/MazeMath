using System;
using System.Collections.Generic;
using System.Linq;
using MazeMath.Inventory;

namespace MazeMath.Crafting
{
    public sealed class CraftingService : ICraftingService
    {
        private readonly IInventoryService inventory;
        private readonly Dictionary<string, RecipeDefinition> recipes;
        private readonly HashSet<string> unlockedRecipeIds;

        public CraftingService(
            IInventoryService inventory,
            IEnumerable<RecipeDefinition> recipes,
            IEnumerable<string> unlockedRecipeIds)
        {
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));

            if (recipes == null)
            {
                throw new ArgumentNullException(nameof(recipes));
            }

            this.recipes = recipes
                .Where(recipe => recipe != null && !string.IsNullOrWhiteSpace(recipe.recipeId))
                .ToDictionary(recipe => recipe.recipeId, recipe => recipe);

            this.unlockedRecipeIds = unlockedRecipeIds != null
                ? new HashSet<string>(unlockedRecipeIds)
                : new HashSet<string>();
        }

        public bool CanCraft(string recipeId)
        {
            if (!recipes.TryGetValue(recipeId, out var recipe) ||
                !unlockedRecipeIds.Contains(recipeId))
            {
                return false;
            }

            return HasAllIngredients(recipe);
        }

        public CraftResult Craft(string recipeId)
        {
            if (!recipes.TryGetValue(recipeId, out var recipe))
            {
                return CraftResult.UnknownRecipe;
            }

            if (recipe.type == RecipeType.Pattern)
            {
                return CraftResult.InvalidPattern;
            }

            return CraftInternal(recipeId, recipe);
        }

        public CraftResult CraftPattern(
            string recipeId,
            System.Collections.Generic.IReadOnlyList<string> gridItemIds)
        {
            if (!recipes.TryGetValue(recipeId, out var recipe))
            {
                return CraftResult.UnknownRecipe;
            }

            if (recipe.type != RecipeType.Pattern ||
                !RecipeMatcher.Matches(recipe, gridItemIds))
            {
                return CraftResult.InvalidPattern;
            }

            return CraftInternal(recipeId, recipe);
        }

        private CraftResult CraftInternal(string recipeId, RecipeDefinition recipe)
        {
            if (!unlockedRecipeIds.Contains(recipeId))
            {
                return CraftResult.RecipeLocked;
            }

            if (!HasAllIngredients(recipe))
            {
                return CraftResult.MissingMaterial;
            }

            var removed = new List<IngredientRequirement>();

            foreach (var ingredient in recipe.ingredients)
            {
                if (!inventory.TryRemove(ingredient.itemId, ingredient.count))
                {
                    Rollback(removed);
                    return CraftResult.MissingMaterial;
                }

                removed.Add(new IngredientRequirement
                {
                    itemId = ingredient.itemId,
                    count = ingredient.count
                });
            }

            if (!inventory.TryAdd(recipe.resultItemId, recipe.resultCount))
            {
                Rollback(removed);
                return CraftResult.InventoryFull;
            }

            return CraftResult.Success;
        }

        public void UnlockRecipe(string recipeId)
        {
            if (recipes.ContainsKey(recipeId))
            {
                unlockedRecipeIds.Add(recipeId);
            }
        }

        public bool IsUnlocked(string recipeId)
        {
            return unlockedRecipeIds.Contains(recipeId);
        }

        private bool HasAllIngredients(RecipeDefinition recipe)
        {
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredient == null ||
                    ingredient.count <= 0 ||
                    !inventory.Has(ingredient.itemId, ingredient.count))
                {
                    return false;
                }
            }

            return recipe.resultCount > 0 &&
                   !string.IsNullOrWhiteSpace(recipe.resultItemId);
        }

        private void Rollback(IEnumerable<IngredientRequirement> removed)
        {
            foreach (var ingredient in removed)
            {
                if (!inventory.TryAdd(ingredient.itemId, ingredient.count))
                {
                    throw new InvalidOperationException(
                        "Crafting rollback failed for item: " + ingredient.itemId);
                }
            }
        }
    }
}
