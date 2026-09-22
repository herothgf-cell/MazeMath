using System.Collections.Generic;
using MazeMath.Crafting;
using MazeMath.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Crafting
{
    public sealed class PatternCraftingTests
    {
        [Test]
        public void ExactPattern_Matches()
        {
            var recipe = CreatePatternRecipe();
            var grid = new[]
            {
                "gear", null, "gear",
                null, "crystal", null,
                "scrap", null, "scrap"
            };

            Assert.IsTrue(RecipeMatcher.Matches(recipe, grid));
        }

        [Test]
        public void SwappedPattern_DoesNotMatch()
        {
            var recipe = CreatePatternRecipe();
            var grid = new[]
            {
                "scrap", null, "gear",
                null, "crystal", null,
                "gear", null, "scrap"
            };

            Assert.IsFalse(RecipeMatcher.Matches(recipe, grid));
        }

        [Test]
        public void InvalidPattern_DoesNotConsumeMaterials()
        {
            var items = CreateItems();
            var inventory = new InventoryService(items, 12);
            inventory.TryAdd("gear", 2);
            inventory.TryAdd("crystal", 1);
            inventory.TryAdd("scrap", 2);

            var recipe = CreatePatternRecipe();
            var service = new CraftingService(
                inventory,
                new[] { recipe },
                new[] { recipe.recipeId });

            var invalid = new string[9];
            var result = service.CraftPattern(recipe.recipeId, invalid);

            Assert.AreEqual(CraftResult.InvalidPattern, result);
            Assert.AreEqual(2, inventory.GetCount("gear"));
            Assert.AreEqual(1, inventory.GetCount("crystal"));
            Assert.AreEqual(2, inventory.GetCount("scrap"));
        }

        [Test]
        public void CorrectPattern_ConsumesMaterialsAndCreatesResult()
        {
            var items = CreateItems();
            var inventory = new InventoryService(items, 12);
            inventory.TryAdd("gear", 2);
            inventory.TryAdd("crystal", 1);
            inventory.TryAdd("scrap", 2);

            var recipe = CreatePatternRecipe();
            var service = new CraftingService(
                inventory,
                new[] { recipe },
                new[] { recipe.recipeId });

            var grid = new[]
            {
                "gear", null, "gear",
                null, "crystal", null,
                "scrap", null, "scrap"
            };

            Assert.AreEqual(CraftResult.Success, service.CraftPattern(recipe.recipeId, grid));
            Assert.AreEqual(1, inventory.GetCount("sensor"));
        }

        private static RecipeDefinition CreatePatternRecipe()
        {
            var recipe = ScriptableObject.CreateInstance<RecipeDefinition>();
            recipe.recipeId = "sensor-pattern";
            recipe.resultItemId = "sensor";
            recipe.resultCount = 1;
            recipe.type = RecipeType.Pattern;
            recipe.patternItemIds = new List<string>
            {
                "gear", "", "gear",
                "", "crystal", "",
                "scrap", "", "scrap"
            };
            recipe.ingredients.Add(new IngredientRequirement { itemId = "gear", count = 2 });
            recipe.ingredients.Add(new IngredientRequirement { itemId = "crystal", count = 1 });
            recipe.ingredients.Add(new IngredientRequirement { itemId = "scrap", count = 2 });
            return recipe;
        }

        private static ItemDefinition[] CreateItems()
        {
            return new[]
            {
                Item("gear", 99, ItemCategory.Material),
                Item("crystal", 99, ItemCategory.Material),
                Item("scrap", 99, ItemCategory.Material),
                Item("sensor", 1, ItemCategory.Equipment)
            };
        }

        private static ItemDefinition Item(string id, int maxStack, ItemCategory category)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = id;
            item.maxStack = maxStack;
            item.category = category;
            return item;
        }
    }
}
