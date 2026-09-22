using System.Linq;
using MazeMath.Maze;
using MazeMath.Maze.Data;
using MazeMath.Maze.Generation;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Maze
{
    public sealed class MazeGeneratorTests
    {
        [Test]
        public void Generate_CreatesGuaranteedCriticalPath()
        {
            var chapter = CreateChapter(requiredRooms: 6, optionalRooms: 0);
            var generator = new MazeGenerator();

            var graph = generator.Generate(chapter, 1234);

            Assert.AreEqual(1, graph.Nodes.Values.Count(node => node.Type == RoomType.Start));
            Assert.AreEqual(1, graph.Nodes.Values.Count(node => node.Type == RoomType.Boss));
            Assert.AreEqual(6, graph.Nodes.Values.Count(node => node.IsCriticalPath));

            var start = graph.Nodes.Values.Single(node => node.Type == RoomType.Start);
            var boss = graph.Nodes.Values.Single(node => node.Type == RoomType.Boss);
            Assert.IsNotEmpty(graph.FindPath(start.NodeId, boss.NodeId));
        }

        [Test]
        public void Generate_AddsRequestedOptionalBranches()
        {
            var chapter = CreateChapter(requiredRooms: 6, optionalRooms: 3);
            var generator = new MazeGenerator();

            var graph = generator.Generate(chapter, 9876);

            Assert.AreEqual(3, graph.Nodes.Values.Count(node => !node.IsCriticalPath));
        }


        [Test]
        public void Generate_PlacesRequiredRoomTagsOnCriticalPathInOrder()
        {
            var chapter = CreateChapter(requiredRooms: 7, optionalRooms: 0);
            chapter.requiredRoomTags.Add("workshop");
            chapter.requiredRoomTags.Add("question");

            foreach (var room in chapter.allowedRooms)
            {
                if (room.type == RoomType.Corridor)
                {
                    room.tags.Add("workshop");
                }

                if (room.type == RoomType.Question)
                {
                    room.tags.Add("question");
                }
            }

            var graph = new MazeGenerator().Generate(chapter, 42);
            var start = graph.Nodes.Values.Single(node => node.Type == RoomType.Start);
            var boss = graph.Nodes.Values.Single(node => node.Type == RoomType.Boss);
            var path = graph.FindPath(start.NodeId, boss.NodeId);

            Assert.AreEqual(
                RoomType.Corridor,
                graph.Nodes[path[1]].Type);
            Assert.AreEqual(
                RoomType.Question,
                graph.Nodes[path[2]].Type);
        }

        [Test]
        public void SameSeed_CreatesSameTopology()
        {
            var chapter = CreateChapter(requiredRooms: 7, optionalRooms: 3);
            var generator = new MazeGenerator();

            var first = generator.Generate(chapter, 55);
            var second = generator.Generate(chapter, 55);

            CollectionAssert.AreEqual(
                first.Nodes.Keys.OrderBy(id => id).ToArray(),
                second.Nodes.Keys.OrderBy(id => id).ToArray());

            CollectionAssert.AreEqual(
                first.Edges.Values
                    .OrderBy(edge => edge.EdgeId)
                    .Select(edge => $"{edge.FromNodeId}>{edge.ToNodeId}:{edge.Type}")
                    .ToArray(),
                second.Edges.Values
                    .OrderBy(edge => edge.EdgeId)
                    .Select(edge => $"{edge.FromNodeId}>{edge.ToNodeId}:{edge.Type}")
                    .ToArray());
        }

        private static MazeChapterDefinition CreateChapter(int requiredRooms, int optionalRooms)
        {
            var chapter = ScriptableObject.CreateInstance<MazeChapterDefinition>();
            chapter.floorCount = 1;
            chapter.requiredRoomCountRange = new Vector2Int(requiredRooms, requiredRooms);
            chapter.optionalRoomCountRange = new Vector2Int(optionalRooms, optionalRooms);
            chapter.maxBranchDepth = 1;
            chapter.difficulty = ScriptableObject.CreateInstance<MazeDifficultyProfile>();

            chapter.allowedRooms.Add(CreateRoom("start", RoomType.Start, true, false));
            chapter.allowedRooms.Add(CreateRoom("corridor", RoomType.Corridor, true, false));
            chapter.allowedRooms.Add(CreateRoom("question", RoomType.Question, true, false));
            chapter.allowedRooms.Add(CreateRoom("puzzle", RoomType.Puzzle, true, false));
            chapter.allowedRooms.Add(CreateRoom("reward", RoomType.Reward, false, true));
            chapter.allowedRooms.Add(CreateRoom("boss", RoomType.Boss, true, false));

            return chapter;
        }

        private static RoomTemplateDefinition CreateRoom(
            string id,
            RoomType type,
            bool critical,
            bool optional)
        {
            var room = ScriptableObject.CreateInstance<RoomTemplateDefinition>();
            room.templateId = id;
            room.type = type;
            room.canBeCriticalPath = critical;
            room.canBeOptional = optional;
            room.floorMin = 0;
            room.floorMax = 2;
            return room;
        }
    }
}
