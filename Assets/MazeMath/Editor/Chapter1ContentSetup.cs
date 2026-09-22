using System.Collections.Generic;
using System.IO;
using MazeMath.Content;
using MazeMath.Crafting;
using MazeMath.Enchant;
using MazeMath.Equipment;
using MazeMath.Inventory;
using MazeMath.Maze;
using MazeMath.Maze.Data;
using MazeMath.Questions.Data;
using UnityEditor;
using UnityEngine;

namespace MazeMath.Editor
{
    public static class Chapter1ContentSetup
    {
        public const string Root = "Assets/MazeMath/ScriptableObjects/Chapter01";
        public const string CatalogPath = Root + "/Chapter01Catalog.asset";

        public static Chapter1ContentCatalog CreateOrUpdate()
        {
            EnsureFolders();

            var items = CreateItems();
            var equipment = CreateEquipment();
            var recipes = CreateRecipes();
            var enchants = CreateEnchants();
            LinkEquipmentEnchants(equipment, enchants);
            var questions = CreateQuestions();
            var chapter = CreateChapter();

            var catalog = GetOrCreate<Chapter1ContentCatalog>(CatalogPath);
            catalog.chapter = chapter;
            Replace(catalog.items, items);
            Replace(catalog.equipment, equipment);
            Replace(catalog.recipes, recipes);
            Replace(catalog.enchants, enchants);
            Replace(catalog.questionTemplates, questions);
            EditorUtility.SetDirty(catalog);

            AssetDatabase.SaveAssets();
            return catalog;
        }

        private static List<ItemDefinition> CreateItems()
        {
            return new List<ItemDefinition>
            {
                Item("Item_Scrap", ContentIds.Scrap, ItemCategory.Material, 99),
                Item("Item_IronPlate", ContentIds.IronPlate, ItemCategory.Material, 99),
                Item("Item_Gear", ContentIds.Gear, ItemCategory.Material, 99),
                Item("Item_EnergyCrystal", ContentIds.EnergyCrystal, ItemCategory.Material, 99),
                Item("Item_RareCore", ContentIds.RareCore, ItemCategory.Material, 99),
                Item("Item_MiningArm", ContentIds.MiningArmItem, ItemCategory.Equipment, 1),
                Item("Item_PowerWrench", ContentIds.PowerWrenchItem, ItemCategory.Equipment, 1),
                Item("Item_JumpBooster", ContentIds.JumpBoosterItem, ItemCategory.Equipment, 1),
                Item("Item_EnergyShield", ContentIds.EnergyShieldItem, ItemCategory.Equipment, 1),
                Item("Item_ExplorerSensor", ContentIds.ExplorerSensorItem, ItemCategory.Equipment, 1)
            };
        }

        private static List<EquipmentDefinition> CreateEquipment()
        {
            return new List<EquipmentDefinition>
            {
                Equipment(
                    "Equipment_MiningArm",
                    RobotEquipmentIds.MiningArm,
                    ContentIds.MiningArmItem,
                    EquipmentSlot.ArmTool,
                    RobotEquipmentIds.BreakCrackedWall,
                    ContentIds.MiningRecipe),
                Equipment(
                    "Equipment_PowerWrench",
                    RobotEquipmentIds.PowerWrench,
                    ContentIds.PowerWrenchItem,
                    EquipmentSlot.UtilityTool,
                    RobotEquipmentIds.RepairBasicMachine,
                    ContentIds.WrenchRecipe),
                Equipment(
                    "Equipment_JumpBooster",
                    RobotEquipmentIds.JumpBooster,
                    ContentIds.JumpBoosterItem,
                    EquipmentSlot.Mobility,
                    RobotEquipmentIds.JumpHighPlatform,
                    ContentIds.JumpRecipe),
                Equipment(
                    "Equipment_EnergyShield",
                    RobotEquipmentIds.EnergyShield,
                    ContentIds.EnergyShieldItem,
                    EquipmentSlot.Defense,
                    RobotEquipmentIds.ShieldHazard,
                    ContentIds.ShieldRecipe),
                Equipment(
                    "Equipment_ExplorerSensor",
                    RobotEquipmentIds.ExplorerSensor,
                    ContentIds.ExplorerSensorItem,
                    EquipmentSlot.Sensor,
                    RobotEquipmentIds.SensorInteraction,
                    ContentIds.SensorRecipe)
            };
        }

        private static List<RecipeDefinition> CreateRecipes()
        {
            var recipes = new List<RecipeDefinition>
            {
                Guided(
                    "Recipe_MiningArm",
                    ContentIds.MiningRecipe,
                    ContentIds.MiningArmItem,
                    Ingredient(ContentIds.Scrap, 2),
                    Ingredient(ContentIds.IronPlate, 1)),
                Guided(
                    "Recipe_PowerWrench",
                    ContentIds.WrenchRecipe,
                    ContentIds.PowerWrenchItem,
                    Ingredient(ContentIds.Scrap, 2),
                    Ingredient(ContentIds.Gear, 1),
                    Ingredient(ContentIds.EnergyCrystal, 1)),
                Guided(
                    "Recipe_JumpBooster",
                    ContentIds.JumpRecipe,
                    ContentIds.JumpBoosterItem,
                    Ingredient(ContentIds.Scrap, 2),
                    Ingredient(ContentIds.Gear, 2),
                    Ingredient(ContentIds.EnergyCrystal, 1)),
                Guided(
                    "Recipe_EnergyShield",
                    ContentIds.ShieldRecipe,
                    ContentIds.EnergyShieldItem,
                    Ingredient(ContentIds.IronPlate, 2),
                    Ingredient(ContentIds.EnergyCrystal, 2)),
                Guided(
                    "Recipe_ExplorerSensor",
                    ContentIds.SensorRecipe,
                    ContentIds.ExplorerSensorItem,
                    Ingredient(ContentIds.Gear, 2),
                    Ingredient(ContentIds.EnergyCrystal, 1),
                    Ingredient(ContentIds.Scrap, 2))
            };

            recipes.Add(Pattern(
                "Recipe_ExplorerSensor_Pattern",
                ContentIds.SensorPatternRecipe,
                ContentIds.ExplorerSensorItem,
                new[]
                {
                    ContentIds.Gear, "", ContentIds.Gear,
                    "", ContentIds.EnergyCrystal, "",
                    ContentIds.Scrap, "", ContentIds.Scrap
                },
                Ingredient(ContentIds.Gear, 2),
                Ingredient(ContentIds.EnergyCrystal, 1),
                Ingredient(ContentIds.Scrap, 2)));

            recipes.Add(Pattern(
                "Recipe_MiningArm_Pattern",
                ContentIds.MiningPatternRecipe,
                ContentIds.MiningArmItem,
                new[]
                {
                    ContentIds.IronPlate, ContentIds.IronPlate, "",
                    ContentIds.Scrap, ContentIds.Gear, "",
                    "", ContentIds.Scrap, ""
                },
                Ingredient(ContentIds.IronPlate, 2),
                Ingredient(ContentIds.Scrap, 2),
                Ingredient(ContentIds.Gear, 1)));

            return recipes;
        }

        private static List<EnchantDefinition> CreateEnchants()
        {
            return new List<EnchantDefinition>
            {
                Enchant("Enchant_MiningEcho", RobotEnchantIds.MiningEcho, EquipmentSlot.ArmTool, 10, "modifier.mining.echo"),
                Enchant("Enchant_MiningLucky", RobotEnchantIds.MiningLucky, EquipmentSlot.ArmTool, 10, "modifier.mining.lucky"),
                Enchant("Enchant_WrenchQuickFix", RobotEnchantIds.WrenchQuickFix, EquipmentSlot.UtilityTool, 10, "modifier.wrench.quick-fix"),
                Enchant("Enchant_WrenchCircuitSense", RobotEnchantIds.WrenchCircuitSense, EquipmentSlot.UtilityTool, 10, "modifier.wrench.circuit-sense"),
                Enchant("Enchant_JumpSoftLanding", RobotEnchantIds.JumpSoftLanding, EquipmentSlot.Mobility, 10, "modifier.jump.soft-landing"),
                Enchant("Enchant_JumpRouteScan", RobotEnchantIds.JumpRouteScan, EquipmentSlot.Mobility, 10, "modifier.jump.route-scan"),
                Enchant("Enchant_ShieldRecharge", RobotEnchantIds.ShieldRecharge, EquipmentSlot.Defense, 10, "modifier.shield.recharge"),
                Enchant("Enchant_ShieldStableField", RobotEnchantIds.ShieldStableField, EquipmentSlot.Defense, 10, "modifier.shield.stable-field"),
                Enchant("Enchant_SensorMemory", RobotEnchantIds.SensorMemory, EquipmentSlot.Sensor, 10, "modifier.sensor.memory"),
                Enchant("Enchant_SensorGuide", RobotEnchantIds.SensorGuide, EquipmentSlot.Sensor, 10, "modifier.sensor.guide")
            };
        }

        private static void LinkEquipmentEnchants(
            List<EquipmentDefinition> equipment,
            List<EnchantDefinition> enchants)
        {
            foreach (var definition in equipment)
            {
                definition.availableEnchantIds.Clear();
                foreach (var enchant in enchants)
                {
                    if (enchant.compatibleSlot == definition.slot)
                    {
                        definition.availableEnchantIds.Add(enchant.enchantId);
                    }
                }

                EditorUtility.SetDirty(definition);
            }
        }

        private static List<QuestionTemplateDefinition> CreateQuestions()
        {
            return new List<QuestionTemplateDefinition>
            {
                Question("Question_Addition", "question.addition", "addition", LearningAxis.Addition, 1, 20, 40),
                Question("Question_Subtraction", "question.subtraction", "subtraction", LearningAxis.Subtraction, 1, 30, 30),
                Question("Question_Multiplication", "question.multiplication", "multiplication", LearningAxis.Multiplication, 2, 10, 100),
                Question("Question_Division", "question.division", "division", LearningAxis.Division, 2, 10, 100),
                Question("Question_Missing", "question.missing", "missing", LearningAxis.NumberSense, 1, 15, 30, QuestionType.MissingNumber)
            };
        }

        private static MazeChapterDefinition CreateChapter()
        {
            var difficulty = GetOrCreate<MazeDifficultyProfile>(Root + "/MazeDifficulty_Chapter01.asset");
            difficulty.minCriticalPathRooms = 8;
            difficulty.maxCriticalPathRooms = 8;
            difficulty.maxFloorTransitions = 0;
            difficulty.maxSimultaneousObjectives = 1;
            difficulty.hintDelaySeconds = 90;
            difficulty.mapRevealRadius = 0;
            difficulty.allowHiddenRoom = false;
            difficulty.allowMultiFloorDependency = false;
            EditorUtility.SetDirty(difficulty);

            var rooms = new List<RoomTemplateDefinition>
            {
                Room("Room_Start", "start", RoomType.Start, true, false, 4),
                Room("Room_Corridor", "corridor", RoomType.Corridor, true, true, 4),
                Room("Room_Junction", "junction", RoomType.Junction, true, true, 2),
                Room("Room_Question", "question", RoomType.Question, true, true, 3),
                Room("Room_Puzzle", "puzzle", RoomType.Puzzle, true, true, 3),
                Room("Room_Reward", "reward", RoomType.Reward, false, true, 4),
                Room("Room_Workshop", "workshop", RoomType.Workshop, true, true, 2),
                Room("Room_Checkpoint", "checkpoint", RoomType.Checkpoint, true, false, 1),
                Room("Room_Boss", "boss", RoomType.Boss, true, false, 4)
            };

            var chapter = GetOrCreate<MazeChapterDefinition>(Root + "/MazeChapter_01.asset");
            chapter.chapterId = "chapter-01";
            chapter.displayName = "모모의 첫 번째 미로";
            chapter.floorCount = 1;
            chapter.requiredRoomCountRange = new Vector2Int(8, 8);
            chapter.optionalRoomCountRange = new Vector2Int(3, 3);
            chapter.maxBranchDepth = 1;
            chapter.requiredBacktrackCount = 1;
            chapter.maxConsecutiveDeadEnds = 1;
            chapter.difficulty = difficulty;
            chapter.allowedRooms.Clear();
            chapter.allowedRooms.AddRange(rooms);
            chapter.requiredRoomTags.Clear();
            chapter.requiredRoomTags.Add("workshop");
            chapter.requiredRoomTags.Add("question");
            chapter.requiredRoomTags.Add("corridor");
            chapter.requiredRoomTags.Add("puzzle");
            chapter.requiredRoomTags.Add("checkpoint");
            chapter.baseSeedOffset = 101;
            EditorUtility.SetDirty(chapter);
            return chapter;
        }

        private static ItemDefinition Item(
            string assetName,
            string itemId,
            ItemCategory category,
            int maxStack)
        {
            var item = GetOrCreate<ItemDefinition>(Root + "/" + assetName + ".asset");
            item.itemId = itemId;
            item.displayNameKey = itemId + ".name";
            item.descriptionKey = itemId + ".description";
            item.category = category;
            item.maxStack = maxStack;
            EditorUtility.SetDirty(item);
            return item;
        }

        private static EquipmentDefinition Equipment(
            string assetName,
            string equipmentId,
            string itemId,
            EquipmentSlot slot,
            string abilityId,
            string upgradeRecipeId)
        {
            var equipment = GetOrCreate<EquipmentDefinition>(Root + "/" + assetName + ".asset");
            equipment.equipmentId = equipmentId;
            equipment.itemId = itemId;
            equipment.displayNameKey = equipmentId + ".name";
            equipment.slot = slot;
            equipment.tier = 1;
            equipment.abilityIds.Clear();
            equipment.abilityIds.Add(abilityId);
            equipment.upgradeRecipeId = upgradeRecipeId;
            EditorUtility.SetDirty(equipment);
            return equipment;
        }

        private static IngredientRequirement Ingredient(string itemId, int count)
        {
            return new IngredientRequirement { itemId = itemId, count = count };
        }

        private static RecipeDefinition Guided(
            string assetName,
            string recipeId,
            string resultItemId,
            params IngredientRequirement[] ingredients)
        {
            var recipe = GetOrCreate<RecipeDefinition>(Root + "/" + assetName + ".asset");
            recipe.recipeId = recipeId;
            recipe.resultItemId = resultItemId;
            recipe.resultCount = 1;
            recipe.type = RecipeType.Guided;
            recipe.ingredients.Clear();
            recipe.ingredients.AddRange(ingredients);
            recipe.patternItemIds.Clear();
            recipe.allowRotation = false;
            recipe.allowMirror = false;
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static RecipeDefinition Pattern(
            string assetName,
            string recipeId,
            string resultItemId,
            string[] pattern,
            params IngredientRequirement[] ingredients)
        {
            var recipe = GetOrCreate<RecipeDefinition>(Root + "/" + assetName + ".asset");
            recipe.recipeId = recipeId;
            recipe.resultItemId = resultItemId;
            recipe.resultCount = 1;
            recipe.type = RecipeType.Pattern;
            recipe.ingredients.Clear();
            recipe.ingredients.AddRange(ingredients);
            recipe.patternItemIds.Clear();
            recipe.patternItemIds.AddRange(pattern);
            recipe.allowRotation = false;
            recipe.allowMirror = false;
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        private static EnchantDefinition Enchant(
            string assetName,
            string enchantId,
            EquipmentSlot slot,
            int cost,
            string modifier)
        {
            var enchant = GetOrCreate<EnchantDefinition>(Root + "/" + assetName + ".asset");
            enchant.enchantId = enchantId;
            enchant.displayNameKey = enchantId + ".name";
            enchant.descriptionKey = enchantId + ".description";
            enchant.compatibleSlot = slot;
            enchant.level = 1;
            enchant.knowledgeXpCost = cost;
            enchant.abilityModifierId = modifier;
            EditorUtility.SetDirty(enchant);
            return enchant;
        }

        private static QuestionTemplateDefinition Question(
            string assetName,
            string templateId,
            string generatorId,
            LearningAxis axis,
            int min,
            int max,
            int maxAnswer,
            QuestionType type = QuestionType.NumericInput)
        {
            var question = GetOrCreate<QuestionTemplateDefinition>(Root + "/" + assetName + ".asset");
            question.templateId = templateId;
            question.generatorId = generatorId;
            question.type = type;
            question.primaryAxis = axis;
            question.difficulty = DifficultyBand.Normal;
            question.minOperand = min;
            question.maxOperand = max;
            question.maxAnswer = maxAnswer;
            question.maxInputLength = 3;
            EditorUtility.SetDirty(question);
            return question;
        }

        private static RoomTemplateDefinition Room(
            string assetName,
            string templateId,
            RoomType type,
            bool critical,
            bool optional,
            int weight)
        {
            var room = GetOrCreate<RoomTemplateDefinition>(Root + "/" + assetName + ".asset");
            room.templateId = templateId;
            room.type = type;
            room.canBeCriticalPath = critical;
            room.canBeOptional = optional;
            room.floorMin = 0;
            room.floorMax = 0;
            room.weight = weight;
            room.tags.Clear();
            room.tags.Add(templateId);
            EditorUtility.SetDirty(room);
            return room;
        }

        private static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void Replace<T>(List<T> target, List<T> values)
        {
            target.Clear();
            target.AddRange(values);
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/MazeMath/ScriptableObjects");
            Directory.CreateDirectory(Root);
            AssetDatabase.Refresh();
        }
    }
}
