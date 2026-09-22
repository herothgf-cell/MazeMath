using MazeMath.Maze;
using NUnit.Framework;

namespace MazeMath.Tests.Maze
{
    public sealed class MazeGraphTests
    {
        [Test]
        public void FindPath_ReturnsStartToBoss_WhenConnected()
        {
            var graph = new MazeGraph();
            graph.AddNode(new MazeNode("start", 0, RoomType.Start));
            graph.AddNode(new MazeNode("hall", 0, RoomType.Corridor));
            graph.AddNode(new MazeNode("boss", 0, RoomType.Boss));

            graph.AddEdge(new MazeEdge("e1", "start", "hall", EdgeType.Open, true));
            graph.AddEdge(new MazeEdge("e2", "hall", "boss", EdgeType.Open, true));

            CollectionAssert.AreEqual(
                new[] { "start", "hall", "boss" },
                graph.FindPath("start", "boss"));
        }

        [Test]
        public void FindPath_ReturnsEmpty_WhenDisconnected()
        {
            var graph = new MazeGraph();
            graph.AddNode(new MazeNode("start", 0, RoomType.Start));
            graph.AddNode(new MazeNode("boss", 1, RoomType.Boss));

            CollectionAssert.IsEmpty(graph.FindPath("start", "boss"));
        }

        [Test]
        public void AddEdge_RejectsMissingEndpoint()
        {
            var graph = new MazeGraph();
            graph.AddNode(new MazeNode("start", 0, RoomType.Start));

            Assert.Throws<System.InvalidOperationException>(() =>
                graph.AddEdge(new MazeEdge("bad", "start", "missing", EdgeType.Open, true)));
        }
    }
}
