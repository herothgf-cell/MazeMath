using MazeMath.Maze;
using MazeMath.Maze.Hint;
using NUnit.Framework;

namespace MazeMath.Tests.Maze
{
    public sealed class MazeHintServiceTests
    {
        [Test]
        public void NoProgressBeforeThreshold_DoesNotTriggerHint()
        {
            var hints = new MazeHintService(90);
            hints.RegisterProgress(0);

            Assert.AreEqual(MazeHintLevel.None, hints.Evaluate(89, 0, 0));
        }

        [Test]
        public void NoProgressAtThreshold_TriggersLevelOneOnlyOnce()
        {
            var hints = new MazeHintService(90);
            hints.RegisterProgress(0);

            Assert.AreEqual(MazeHintLevel.Observe, hints.Evaluate(90, 0, 0));
            Assert.AreEqual(MazeHintLevel.Observe, hints.Evaluate(91, 0, 0));
        }

        [Test]
        public void RepeatedRoomEntries_TriggerLevelOne()
        {
            var hints = new MazeHintService(90);
            hints.RegisterProgress(0);

            Assert.AreEqual(MazeHintLevel.Observe, hints.Evaluate(10, 3, 0));
        }

        [Test]
        public void StrongerHint_LevelThreeReturnsOnlyNextNode()
        {
            var graph = new MazeGraph();
            graph.AddNode(new MazeNode("a", 0, RoomType.Start));
            graph.AddNode(new MazeNode("b", 0, RoomType.Corridor));
            graph.AddNode(new MazeNode("c", 0, RoomType.Boss));
            graph.AddEdge(new MazeEdge("ab", "a", "b", EdgeType.Open, true));
            graph.AddEdge(new MazeEdge("bc", "b", "c", EdgeType.Open, true));

            var hints = new MazeHintService(90);
            hints.RegisterProgress(0);
            hints.Evaluate(90, 0, 0);
            hints.RequestStrongerHint();
            hints.RequestStrongerHint();

            Assert.AreEqual(MazeHintLevel.Direction, hints.CurrentLevel);
            Assert.AreEqual("b", hints.GetNextNode(graph, "a", "c"));
        }
    }
}
