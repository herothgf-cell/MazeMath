namespace MazeMath.Crafting
{
    public enum CraftResult
    {
        Success,
        RecipeLocked,
        MissingMaterial,
        InventoryFull,
        UnknownRecipe,
        InvalidPattern
    }

    public interface ICraftingService
    {
        bool CanCraft(string recipeId);
        CraftResult Craft(string recipeId);
    }
}
