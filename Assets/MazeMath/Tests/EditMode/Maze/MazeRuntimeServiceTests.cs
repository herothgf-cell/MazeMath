using System.Collections.Generic;
using MazeMath.Maze;
using MazeMath.Maze.Data;
using MazeMath.Maze.Generation;
using MazeMath.Maze.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Maze
{
    public sealed class MazeRuntimeServiceTests
    {
        [Test]
        public void StartChapter_InitializesStartNodeAndVisitedState()
        {
            var chapter = CreateChapter();
            var service = CreateService(chapter);

            var graph = service.StartChapter(chapter.chapterId, 123);

            Assert.AreSame(graph, service.CurrentGraph);
            Assert.AreEqual("start", service.CurrentState.currentNodeId);
            Assert.AreEqual(0, service.CurrentState.currentFloor);
            CollectionAssert.Contains(service.CurrentState.visitedNodeIds, "start");
        }

        [Test]
        public void LockedEdge_OpensAfterGateSolved()
        {
            var chapter = CreateChapter();
            var service = CreateService(chapter);
            var graph = service.StartChapter(chapter.chapterId, 456);

            var startEdge = FindFirstCriticalEdge(graph);
            startEdge.GateId = "gate-a";

            Assert.IsFalse(service.CanTraverse(startEdge.EdgeId));

            service.SolveGate("gate-a");

            Assert.IsTrue(service.CanTraverse(startEdge.EdgeId));
            CollectionAssert.Contains(service.CurrentState.solvedGateIds, "gate-a");
        }

        [Test]
        public void EnterNode_UpdatesFloorAndVisitOnlyOnce()
        {
            var chapter = CreateChapter();
            chapter.floorCount = 2;
            var service = CreateService(chapter);
            var graph = service.StartChapter(chapter.chapterId, 789);

            MazeNode upper = null;
            foreach (var node in graph.Nodes.Values)
            {
                if (node.Floor == 1)
                {
                    upper = node;
                    break;
                }
            }

            Assert.IsNotNull(upper);

            service.EnterNode(upper.NodeId);
            service.EnterNode(upper.NodeId);

            Assert.AreEqual(1, service.CurrentState.currentFloor);
            Assert.AreEqual(
                1,
                service.CurrentState.visitedNodeIds.FindAll(id => id == upper.NodeId).Count);
        }

        private static MazeRuntimeService CreateService(MazeChapterDefinition chapter)
        {
            return new MazeRuntimeService(
                new MazeGenerator(),
                new MazeValidator(),
                new SingleChapterProvider(chapter));
        }

        private static MazeEdge FindFirstCriticalEdge(MazeGraph graph)
        {
            foreach (var edge in graph.Edges.Values)
            {
                if (edge.EdgeId.StartsWith("critical-edge"))
                {
                    return edge;
                }
            }

            Assert.Fail("Critical edge not found.");
            return null;
        }

        private static MazeChapterDefinition CreateChapter()
        {
            var chapter = ScriptableObject.CreateInstance<MazeChapterDefinition>();
            chapter.chapterId = "chapter-runtime-test";
            chapter.floorCount = 1;
            chapter.requiredRoomCountRange = new Vector2Int(4, 4);
            chapter.optionalRoomCountRange = Vector2Int.zero;
            chapter.difficulty = ScriptableObject.CreateInstance<MazeDifficultyProfile>();

            chapter.allowedRooms.Add(CreateRoom("start", RoomType.Start));
            chapter.allowedRooms.Add(CreateRoom("corridor", RoomType.Corridor));
            chapter.allowedRooms.Add(CreateRoom("boss", RoomType.Boss));
            return chapter;
        }

        private static RoomTemplateDefinition CreateRoom(string id, RoomType type)
        {
            var room = ScriptableObject.CreateInstance<RoomTemplateDefinition>();
            room.templateId = id;
            room.type = type;
            room.canBeCriticalPath = true;
            room.canBeOptional = false;
            room.floorMin = 0;
            room.floorMax = 2;
            return room;
        }

        private sealed class SingleChapterProvider : IMazeChapterProvider
        {
            private readonly MazeChapterDefinition chapter;

            public SingleChapterProvider(MazeChapterDefinition chapter)
            {
                this.chapter = chapter;
            }

            public MazeChapterDefinition Get(string chapterId)
            {
                return chapterId == chapter.chapterId ? chapter : null;
            }
        }
    }
}
