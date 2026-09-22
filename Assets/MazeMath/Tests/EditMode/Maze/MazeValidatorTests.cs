using System.Linq;
using MazeMath.Maze;
using MazeMath.Maze.Data;
using MazeMath.Maze.Generation;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Maze
{
    public sealed class MazeValidatorTests
    {
        [Test]
        public void Validate_FindsMissingPathToBoss()
        {
            var chapter = CreateChapter(3, 0);
            var graph = new MazeGraph();
            graph.AddNode(new MazeNode("start", 0, RoomType.Start));
            graph.AddNode(new MazeNode("boss", 0, RoomType.Boss));

            var result = new MazeValidator().Validate(graph, chapter);

            CollectionAssert.Contains(result.Errors, MazeValidationError.NoPathToBoss);
        }

        [Test]
        public void Validate_RejectsOpenEdgeAcrossFloors()
        {
            var chapter = CreateChapter(3, 0);
            chapter.floorCount = 2;

            var graph = new MazeGraph();
            graph.AddNode(new MazeNode("start", 0, RoomType.Start));
            graph.AddNode(new MazeNode("boss", 1, RoomType.Boss));
            graph.AddEdge(new MazeEdge("bad-floor", "start", "boss", EdgeType.Open, true));

            var result = new MazeValidator().Validate(graph, chapter);

            CollectionAssert.Contains(result.Errors, MazeValidationError.InvalidFloorTransition);
        }

        [Test]
        public void Validate_FindsUnreachableOptionalRoom()
        {
            var chapter = CreateChapter(3, 1);
            var graph = new MazeGraph();

            var start = new MazeNode("start", 0, RoomType.Start) { IsCriticalPath = true };
            var boss = new MazeNode("boss", 0, RoomType.Boss) { IsCriticalPath = true };
            var optional = new MazeNode("optional", 0, RoomType.Reward) { IsCriticalPath = false };

            graph.AddNode(start);
            graph.AddNode(boss);
            graph.AddNode(optional);
            graph.AddEdge(new MazeEdge("main", "start", "boss", EdgeType.Open, true));

            var result = new MazeValidator().Validate(graph, chapter);

            CollectionAssert.Contains(result.Errors, MazeValidationError.UnreachableOptionalRoom);
        }

        [Test]
        public void ThousandGeneratedSeeds_HaveNoCriticalValidationErrors()
        {
            var chapter = CreateChapter(7, 3);
            var generator = new MazeGenerator();
            var validator = new MazeValidator();

            for (var seed = 0; seed < 1000; seed++)
            {
                var graph = generator.Generate(chapter, seed);
                var result = validator.Validate(graph, chapter);

                Assert.IsFalse(
                    result.HasCriticalErrors,
                    $"seed={seed}: {string.Join(",", result.Errors.Select(error => error.ToString()))}");
            }
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
