using MazeMath.Maze;
using MazeMath.Maze.Map;
using MazeMath.Maze.Runtime;
using NUnit.Framework;

namespace MazeMath.Tests.Maze
{
    public sealed class MazeMapTests
    {
        [Test]
        public void RevealState_OnlyAdvances()
        {
            var graph = CreateGraph();
            var state = new MazeRunState();
            var map = new MazeMapService(graph, state);

            Assert.AreEqual(MazeRevealState.Unknown, map.GetState("room-a"));

            map.MarkSeen("room-a");
            Assert.AreEqual(MazeRevealState.Seen, map.GetState("room-a"));

            map.MarkVisited("room-a");
            Assert.AreEqual(MazeRevealState.Visited, map.GetState("room-a"));

            map.MarkSeen("room-a");
            Assert.AreEqual(MazeRevealState.Visited, map.GetState("room-a"));

            map.MarkCleared("room-a");
            Assert.AreEqual(MazeRevealState.Cleared, map.GetState("room-a"));
        }

        [Test]
        public void GetRevealedNodes_FiltersByFloor()
        {
            var graph = CreateGraph();
            var state = new MazeRunState();
            var map = new MazeMapService(graph, state);

            map.MarkVisited("room-a");
            map.MarkVisited("room-b");

            CollectionAssert.AreEquivalent(
                new[] { "room-a" },
                map.GetRevealedNodes(0));

            CollectionAssert.AreEquivalent(
                new[] { "room-b" },
                map.GetRevealedNodes(1));
        }

        private static MazeGraph CreateGraph()
        {
            var graph = new MazeGraph();
            graph.AddNode(new MazeNode("room-a", 0, RoomType.Corridor));
            graph.AddNode(new MazeNode("room-b", 1, RoomType.Corridor));
            return graph;
        }
    }
}
