using System.Collections.Generic;
using MazeMath.Crafting;
using MazeMath.Enchant;
using MazeMath.Equipment;
using MazeMath.Inventory;
using MazeMath.Maze.Data;
using MazeMath.Questions.Data;
using UnityEngine;

namespace MazeMath.Content
{
    [CreateAssetMenu(menuName = "MazeMath/Content/Chapter 1 Catalog")]
    public sealed class Chapter1ContentCatalog : ScriptableObject
    {
        public MazeChapterDefinition chapter;
        public List<ItemDefinition> items = new List<ItemDefinition>();
        public List<EquipmentDefinition> equipment = new List<EquipmentDefinition>();
        public List<RecipeDefinition> recipes = new List<RecipeDefinition>();
        public List<EnchantDefinition> enchants = new List<EnchantDefinition>();
        public List<QuestionTemplateDefinition> questionTemplates =
            new List<QuestionTemplateDefinition>();
    }
}
