using System;
using System.Collections.Generic;
using System.Linq;
using MazeMath.Content;
using MazeMath.Crafting;
using MazeMath.Enchant;
using MazeMath.Equipment;
using MazeMath.Inventory;
using MazeMath.Maze;
using MazeMath.Maze.Generation;
using MazeMath.Maze.Runtime;
using MazeMath.Questions.Data;
using MazeMath.Questions.Generation;
using MazeMath.Questions.Generation.Arithmetic;
using MazeMath.Questions.Generation.Pattern;
using MazeMath.Questions.Runtime;
using MazeMath.Puzzles.Types;
using MazeMath.Rewards;
using UnityEngine;

namespace MazeMath.Demo
{
    public sealed class Chapter1VerticalSliceController : MonoBehaviour
    {
        [SerializeField] private Chapter1ContentCatalog catalog;

        private InventoryService inventory;
        private EquipmentService equipment;
        private CraftingService crafting;
        private KnowledgeXpService xp;
        private EnchantService enchants;
        private RewardService rewards;
        private MazeRuntimeService maze;
        private QuestionSession questionSession;
        private QuestionRewardBinding questionRewardBinding;
        private Chapter1DemoHud hud;
        private MazeWorldDebugRenderer world;
        private List<string> criticalPath;
        private int currentPathIndex;
        private int questionIndex;

        public Chapter1ContentCatalog Catalog
        {
            get => catalog;
            set => catalog = value;
        }

        public string CurrentNodeId =>
            maze != null && maze.CurrentState != null ? maze.CurrentState.currentNodeId : null;

        private void Start()
        {
            if (catalog == null || catalog.chapter == null)
            {
                Debug.LogError("Chapter1ContentCatalog is not assigned. Run MazeMath/Setup Project.");
                enabled = false;
                return;
            }

            InitializeServices();
            InitializeMaze();
            InitializeViews();
            RefreshStatus();
        }

        private void OnDestroy()
        {
            questionRewardBinding?.Dispose();
        }

        private void InitializeServices()
        {
            inventory = new InventoryService(catalog.items, 20);
            inventory.TryAdd(ContentIds.Scrap, 1);
            inventory.TryAdd(ContentIds.IronPlate, 1);
            inventory.TryAdd(ContentIds.Gear, 2);
            inventory.TryAdd(ContentIds.EnergyCrystal, 1);

            equipment = new EquipmentService(inventory, catalog.equipment);
            xp = new KnowledgeXpService();
            enchants = new EnchantService(xp, equipment, catalog.enchants);

            crafting = new CraftingService(
                inventory,
                catalog.recipes,
                catalog.recipes.Select(recipe => recipe.recipeId));

            rewards = new RewardService(inventory, xp);
            questionSession = new QuestionSession();
            questionRewardBinding = new QuestionRewardBinding(questionSession, rewards);
            questionSession.Completed += OnQuestionCompleted;
        }

        private void InitializeMaze()
        {
            maze = new MazeRuntimeService(
                new MazeGenerator(),
                new MazeValidator(),
                new CatalogChapterProvider(catalog.chapter),
                equipment);

            var seed = MazeSeedService.Create(
                "vertical-slice",
                catalog.chapter.chapterId,
                0,
                catalog.chapter.baseSeedOffset);

            var graph = maze.StartChapter(catalog.chapter.chapterId, seed);
            var start = graph.Nodes.Values.Single(node => node.Type == RoomType.Start);
            var boss = graph.Nodes.Values.Single(node => node.Type == RoomType.Boss);
            criticalPath = graph.FindPath(start.NodeId, boss.NodeId).ToList();

            ConfigureMiningGate(graph);
            currentPathIndex = 0;
        }

        private void InitializeViews()
        {
            hud = gameObject.GetComponent<Chapter1DemoHud>();
            if (hud == null) hud = gameObject.AddComponent<Chapter1DemoHud>();
            hud.EnsureBuilt();

            hud.PreviousRequested += MovePrevious;
            hud.NextRequested += MoveNext;
            hud.QuestionRequested += StartQuestion;
            hud.CraftMiningRequested += CraftMiningArm;
            hud.PatternSensorRequested += ShowSensorPattern;
            hud.EnchantSensorRequested += EnchantSensor;
            hud.PuzzleRequested += RunEnvironmentPuzzleDemo;

            world = gameObject.GetComponent<MazeWorldDebugRenderer>();
            if (world == null) world = gameObject.AddComponent<MazeWorldDebugRenderer>();
            world.Render(maze.CurrentGraph);
            world.MovePlayer(CurrentNodeId);

            hud.SetMessage(
                "문제를 풀어 Scrap을 얻고 Mining Arm을 만든 뒤 빨간 Gate를 통과해 보세요.");
        }

        private void ConfigureMiningGate(MazeGraph graph)
        {
            if (criticalPath == null || criticalPath.Count < 4)
                return;

            var from = criticalPath[2];
            var to = criticalPath[3];
            var edge = FindEdge(graph, from, to);
            if (edge == null)
                return;

            edge.Type = EdgeType.EquipmentLocked;
            edge.RequirementId = RobotEquipmentIds.BreakCrackedWall;
            edge.GateId = null;
        }

        private void MoveNext()
        {
            if (criticalPath == null || currentPathIndex >= criticalPath.Count - 1)
            {
                hud.SetMessage("보스 방까지 도착했습니다!");
                return;
            }

            var from = criticalPath[currentPathIndex];
            var to = criticalPath[currentPathIndex + 1];
            var edge = FindEdge(maze.CurrentGraph, from, to);

            if (edge == null || !maze.CanTraverse(edge.EdgeId))
            {
                hud.SetMessage("길이 막혀 있습니다. Mining Arm을 제작하고 장착해 보세요.");
                return;
            }

            currentPathIndex++;
            maze.EnterNode(to);
            world.MovePlayer(to);
            HandleEnteredRoom(maze.CurrentGraph.Nodes[to]);
            RefreshStatus();
        }

        private void MovePrevious()
        {
            if (criticalPath == null || currentPathIndex <= 0)
                return;

            currentPathIndex--;
            var nodeId = criticalPath[currentPathIndex];
            maze.EnterNode(nodeId);
            world.MovePlayer(nodeId);
            hud.SetMessage("이전 방으로 돌아왔습니다.");
            RefreshStatus();
        }

        private void HandleEnteredRoom(MazeNode node)
        {
            switch (node.Type)
            {
                case RoomType.Question:
                    hud.SetMessage("문제 방입니다. 문제를 해결해 재료와 XP를 얻어보세요.");
                    StartQuestion();
                    break;
                case RoomType.Puzzle:
                    hud.SetMessage("환경 퍼즐 방입니다. '환경 퍼즐' 버튼으로 퍼즐 로직을 체험할 수 있습니다.");
                    break;
                case RoomType.Workshop:
                    hud.SetMessage("작업대입니다. 장비 제작과 3×3 조합을 시도해 보세요.");
                    break;
                case RoomType.Reward:
                    rewards.Grant(
                        "room:" + node.NodeId,
                        new LearningReward(5, new[] { new ItemReward(ContentIds.Gear, 1) }));
                    hud.SetMessage("보상 상자: Gear +1, Knowledge XP +5");
                    break;
                case RoomType.Boss:
                    hud.SetMessage("보스 방 도착! 이후 보스 퍼즐 단계와 연결할 수 있습니다.");
                    break;
                default:
                    hud.SetMessage(node.Type + " 방에 들어왔습니다.");
                    break;
            }
        }

        private void StartQuestion()
        {
            if (catalog.questionTemplates == null || catalog.questionTemplates.Count == 0)
                return;

            var template = catalog.questionTemplates[questionIndex % catalog.questionTemplates.Count];
            var seed = MazeSeedService.StableHash(
                maze.CurrentState.runSeed + ":" + CurrentNodeId + ":" + questionIndex);

            var generator = ResolveGenerator(template.generatorId);
            var question = generator.Generate(template, seed);

            if (questionIndex % 2 == 1 &&
                question.Type != QuestionType.MissingNumber)
            {
                ChoiceDistractorBuilder.AddChoices(question, 4);
            }

            questionIndex++;
            hud.ShowQuestion(question, questionSession);
            hud.SetMessage(
                question.Type == QuestionType.MultipleChoice
                    ? "객관식 문제입니다."
                    : "숫자를 직접 입력해 보세요.");
        }

        private void CraftMiningArm()
        {
            var result = crafting.Craft(ContentIds.MiningRecipe);
            if (result == CraftResult.Success)
            {
                equipment.Equip(RobotEquipmentIds.MiningArm);
                hud.SetMessage("Mining Arm 제작 완료! 균열벽 Gate를 통과할 수 있습니다.");
            }
            else
            {
                hud.SetMessage("Mining Arm 제작: " + result + " / 문제를 풀어 Scrap을 모아보세요.");
            }

            RefreshStatus();
        }

        private void ShowSensorPattern()
        {
            hud.ShowPatternCrafting(
                crafting,
                ContentIds.SensorPatternRecipe,
                new[] { ContentIds.Gear, ContentIds.EnergyCrystal, ContentIds.Scrap },
                OnSensorPatternResult);

            hud.SetMessage("3×3 패턴을 맞춰 Explorer Sensor를 조립해 보세요.");
        }

        private void OnSensorPatternResult(CraftResult result)
        {
            if (result == CraftResult.Success)
            {
                equipment.Equip(RobotEquipmentIds.ExplorerSensor);
                hud.HidePatternCrafting();
                hud.SetMessage("Explorer Sensor 제작/장착 완료!");
            }
            else
            {
                hud.SetMessage("3×3 조합 결과: " + result);
            }

            RefreshStatus();
        }

        private void EnchantSensor()
        {
            if (!inventory.Has(ContentIds.ExplorerSensorItem, 1))
            {
                hud.SetMessage("먼저 Explorer Sensor를 제작해 주세요.");
                return;
            }

            equipment.Equip(RobotEquipmentIds.ExplorerSensor);

            if (!enchants.IsUnlocked(RobotEnchantIds.SensorMemory) &&
                !enchants.Unlock(RobotEnchantIds.SensorMemory))
            {
                hud.SetMessage("Memory 인챈트에는 Knowledge XP 10이 필요합니다.");
                return;
            }

            if (enchants.EquipEnchant(
                RobotEquipmentIds.ExplorerSensor,
                RobotEnchantIds.SensorMemory))
            {
                hud.SetMessage("Explorer Sensor에 Memory I 인챈트를 장착했습니다.");
            }

            RefreshStatus();
        }

        private void RunEnvironmentPuzzleDemo()
        {
            var weight = new WeightBridgePuzzle("demo-weight", 8);
            weight.StartPuzzle();
            weight.PlaceWeight("3kg", 3);
            weight.PlaceWeight("5kg", 5);

            var text = weight.IsSolved()
                ? "무게 다리 데모: 3kg + 5kg = 8kg, 다리가 열렸습니다."
                : "무게 다리 데모가 아직 해결되지 않았습니다.";

            rewards.Grant(
                "puzzle:demo-weight",
                new LearningReward(10, new[] { new ItemReward(ContentIds.Scrap, 1) }));

            hud.SetMessage(text + " / Scrap +1, XP +10");
            RefreshStatus();
        }

        private void OnQuestionCompleted(QuestionInstance question)
        {
            hud.SetMessage("정답! Scrap +1과 Knowledge XP를 획득했습니다.");
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (hud == null || inventory == null || xp == null)
                return;

            var mining = equipment.GetEquipped(EquipmentSlot.ArmTool) ?? "-";
            var sensor = equipment.GetEquipped(EquipmentSlot.Sensor) ?? "-";
            var sensorEnchant = enchants.GetActiveEnchant(RobotEquipmentIds.ExplorerSensor) ?? "-";

            hud.SetStatus(
                $"CHAPTER 1\n" +
                $"현재 방: {CurrentNodeId}\n" +
                $"진행: {currentPathIndex + 1}/{(criticalPath != null ? criticalPath.Count : 0)}\n\n" +
                $"Scrap: {inventory.GetCount(ContentIds.Scrap)}\n" +
                $"Iron: {inventory.GetCount(ContentIds.IronPlate)}\n" +
                $"Gear: {inventory.GetCount(ContentIds.Gear)}\n" +
                $"Crystal: {inventory.GetCount(ContentIds.EnergyCrystal)}\n" +
                $"Knowledge XP: {xp.Balance}\n\n" +
                $"Arm: {mining}\n" +
                $"Sensor: {sensor}\n" +
                $"Enchant: {sensorEnchant}");
        }

        private static IQuestionGenerator ResolveGenerator(string generatorId)
        {
            switch (generatorId)
            {
                case "subtraction": return new SubtractionGenerator();
                case "multiplication": return new MultiplicationGenerator();
                case "division": return new DivisionGenerator();
                case "missing": return new MissingNumberGenerator();
                case "addition":
                default:
                    return new AdditionGenerator();
            }
        }

        private static MazeEdge FindEdge(MazeGraph graph, string from, string to)
        {
            foreach (var edge in graph.Edges.Values)
            {
                var direct = edge.FromNodeId == from && edge.ToNodeId == to;
                var reverse = edge.IsBidirectional &&
                              edge.FromNodeId == to &&
                              edge.ToNodeId == from;
                if (direct || reverse)
                    return edge;
            }

            return null;
        }

        private sealed class CatalogChapterProvider : IMazeChapterProvider
        {
            private readonly MazeMath.Maze.Data.MazeChapterDefinition chapter;

            public CatalogChapterProvider(MazeMath.Maze.Data.MazeChapterDefinition chapter)
            {
                this.chapter = chapter;
            }

            public MazeMath.Maze.Data.MazeChapterDefinition Get(string chapterId)
            {
                return chapter != null && chapter.chapterId == chapterId
                    ? chapter
                    : null;
            }
        }
    }
}
