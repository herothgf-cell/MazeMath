using System.Collections.Generic;
using MazeMath.Crafting;
using MazeMath.Inventory;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Crafting
{
    public sealed class CraftingServiceTests
    {
        [Test]
        public void Craft_MissingMaterial_DoesNotChangeInventory()
        {
            var scrap = CreateItem("scrap", 99);
            var wrench = CreateItem("wrench", 1, ItemCategory.Equipment);
            var inventory = new InventoryService(new[] { scrap, wrench }, 12);
            inventory.TryAdd("scrap", 1);

            var recipe = CreateRecipe("wrench-recipe", "wrench", 1, ("scrap", 2));
            var service = new CraftingService(
                inventory,
                new[] { recipe },
                new[] { recipe.recipeId });

            var result = service.Craft(recipe.recipeId);

            Assert.AreEqual(CraftResult.MissingMaterial, result);
            Assert.AreEqual(1, inventory.GetCount("scrap"));
            Assert.AreEqual(0, inventory.GetCount("wrench"));
        }

        [Test]
        public void Craft_Success_ConsumesIngredientsAndAddsResultOnce()
        {
            var scrap = CreateItem("scrap", 99);
            var gear = CreateItem("gear", 99);
            var wrench = CreateItem("wrench", 1, ItemCategory.Equipment);
            var inventory = new InventoryService(new[] { scrap, gear, wrench }, 12);
            inventory.TryAdd("scrap", 2);
            inventory.TryAdd("gear", 1);

            var recipe = CreateRecipe(
                "wrench-recipe",
                "wrench",
                1,
                ("scrap", 2),
                ("gear", 1));

            var service = new CraftingService(
                inventory,
                new[] { recipe },
                new[] { recipe.recipeId });

            Assert.AreEqual(CraftResult.Success, service.Craft(recipe.recipeId));
            Assert.AreEqual(0, inventory.GetCount("scrap"));
            Assert.AreEqual(0, inventory.GetCount("gear"));
            Assert.AreEqual(1, inventory.GetCount("wrench"));

            Assert.AreEqual(CraftResult.MissingMaterial, service.Craft(recipe.recipeId));
            Assert.AreEqual(1, inventory.GetCount("wrench"));
        }

        [Test]
        public void Craft_WhenResultCannotFit_RollsBackIngredients()
        {
            var scrap = CreateItem("scrap", 10);
            var wrench = CreateItem("wrench", 1, ItemCategory.Equipment);
            var inventory = new InventoryService(new[] { scrap, wrench }, 1);
            inventory.TryAdd("scrap", 10);

            var recipe = CreateRecipe("wrench-recipe", "wrench", 1, ("scrap", 1));
            var service = new CraftingService(
                inventory,
                new[] { recipe },
                new[] { recipe.recipeId });

            var result = service.Craft(recipe.recipeId);

            Assert.AreEqual(CraftResult.InventoryFull, result);
            Assert.AreEqual(10, inventory.GetCount("scrap"));
            Assert.AreEqual(0, inventory.GetCount("wrench"));
        }

        private static ItemDefinition CreateItem(
            string id,
            int maxStack,
            ItemCategory category = ItemCategory.Material)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = id;
            item.maxStack = maxStack;
            item.category = category;
            return item;
        }

        private static RecipeDefinition CreateRecipe(
            string id,
            string resultItemId,
            int resultCount,
            params (string itemId, int count)[] ingredients)
        {
            var recipe = ScriptableObject.CreateInstance<RecipeDefinition>();
            recipe.recipeId = id;
            recipe.resultItemId = resultItemId;
            recipe.resultCount = resultCount;

            foreach (var ingredient in ingredients)
            {
                recipe.ingredients.Add(new IngredientRequirement
                {
                    itemId = ingredient.itemId,
                    count = ingredient.count
                });
            }

            return recipe;
        }
    }
}
