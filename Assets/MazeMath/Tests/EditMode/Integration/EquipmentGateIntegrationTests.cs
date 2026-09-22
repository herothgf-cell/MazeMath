using MazeMath.Equipment;
using MazeMath.Inventory;
using MazeMath.Maze;
using MazeMath.Maze.Data;
using MazeMath.Maze.Generation;
using MazeMath.Maze.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Integration
{
    public sealed class EquipmentGateIntegrationTests
    {
        [Test]
        public void EquipmentLockedEdge_RequiresMatchingEquippedAbility()
        {
            var chapter = CreateChapter();
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.itemId = "mining-arm";
            item.maxStack = 1;
            item.category = ItemCategory.Equipment;

            var inventory = new InventoryService(new[] { item }, 12);
            inventory.TryAdd("mining-arm", 1);

            var equipmentDef = ScriptableObject.CreateInstance<EquipmentDefinition>();
            equipmentDef.equipmentId = "mining-arm-t1";
            equipmentDef.itemId = "mining-arm";
            equipmentDef.slot = EquipmentSlot.ArmTool;
            equipmentDef.abilityIds.Add(RobotEquipmentIds.BreakCrackedWall);

            var equipment = new EquipmentService(inventory, new[] { equipmentDef });
            var service = new MazeRuntimeService(
                new MazeGenerator(),
                new MazeValidator(),
                new Provider(chapter),
                equipment);

            var graph = service.StartChapter(chapter.chapterId, 1);
            var edge = FirstEdge(graph);
            edge.Type = EdgeType.EquipmentLocked;
            edge.RequirementId = RobotEquipmentIds.BreakCrackedWall;

            Assert.IsFalse(service.CanTraverse(edge.EdgeId));

            Assert.IsTrue(equipment.Equip("mining-arm-t1"));
            Assert.IsTrue(service.CanTraverse(edge.EdgeId));
        }

        private static MazeEdge FirstEdge(MazeGraph graph)
        {
            foreach (var edge in graph.Edges.Values) return edge;
            Assert.Fail("No edge generated.");
            return null;
        }

        private static MazeChapterDefinition CreateChapter()
        {
            var chapter = ScriptableObject.CreateInstance<MazeChapterDefinition>();
            chapter.chapterId = "gate-test";
            chapter.floorCount = 1;
            chapter.requiredRoomCountRange = new Vector2Int(3, 3);
            chapter.optionalRoomCountRange = Vector2Int.zero;
            chapter.difficulty = ScriptableObject.CreateInstance<MazeDifficultyProfile>();
            chapter.allowedRooms.Add(Room("start", RoomType.Start));
            chapter.allowedRooms.Add(Room("corridor", RoomType.Corridor));
            chapter.allowedRooms.Add(Room("boss", RoomType.Boss));
            return chapter;
        }

        private static RoomTemplateDefinition Room(string id, RoomType type)
        {
            var room = ScriptableObject.CreateInstance<RoomTemplateDefinition>();
            room.templateId = id;
            room.type = type;
            room.canBeCriticalPath = true;
            room.floorMin = 0;
            room.floorMax = 2;
            return room;
        }

        private sealed class Provider : IMazeChapterProvider
        {
            private readonly MazeChapterDefinition chapter;
            public Provider(MazeChapterDefinition chapter) { this.chapter = chapter; }
            public MazeChapterDefinition Get(string id) => id == chapter.chapterId ? chapter : null;
        }
    }
}
