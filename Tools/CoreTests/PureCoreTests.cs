using MazeMath.Maze;
using MazeMath.Maze.Generation;
using NUnit.Framework;

namespace MazeMath.CoreTests
{
    public sealed class PureCoreTests
    {
        [Test]
        public void GraphFindPath_ReturnsConnectedRoute()
        {
            var graph = new MazeGraph();
            graph.AddNode(new MazeNode("start", 0, RoomType.Start));
            graph.AddNode(new MazeNode("middle", 0, RoomType.Corridor));
            graph.AddNode(new MazeNode("boss", 0, RoomType.Boss));
            graph.AddEdge(new MazeEdge("e1", "start", "middle", EdgeType.Open, true));
            graph.AddEdge(new MazeEdge("e2", "middle", "boss", EdgeType.Open, true));

            CollectionAssert.AreEqual(
                new[] { "start", "middle", "boss" },
                graph.FindPath("start", "boss"));
        }

        [Test]
        public void DeterministicRandom_SameSeedProducesSameSequence()
        {
            var a = new DeterministicRandom(12345);
            var b = new DeterministicRandom(12345);

            for (var i = 0; i < 100; i++)
            {
                Assert.That(a.NextInt(0, 10000), Is.EqualTo(b.NextInt(0, 10000)));
            }
        }

        [Test]
        public void StableSeed_SameInputsProduceSameValue()
        {
            var a = MazeSeedService.Create("profile", "chapter-01", 2, 17);
            var b = MazeSeedService.Create("profile", "chapter-01", 2, 17);
            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void StableSeed_AttemptChangesValue()
        {
            var a = MazeSeedService.Create("profile", "chapter-01", 1, 17);
            var b = MazeSeedService.Create("profile", "chapter-01", 2, 17);
            Assert.That(a, Is.Not.EqualTo(b));
        }
    }
}
