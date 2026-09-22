using MazeMath.Maze.Generation;
using NUnit.Framework;

namespace MazeMath.Tests.Maze
{
    public sealed class MazeSeedTests
    {
        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            var a = new DeterministicRandom(12345);
            var b = new DeterministicRandom(12345);

            for (var i = 0; i < 100; i++)
            {
                Assert.AreEqual(a.NextInt(0, 1000), b.NextInt(0, 1000));
            }
        }

        [Test]
        public void NextInt_StaysInsideExclusiveUpperBound()
        {
            var random = new DeterministicRandom(77);

            for (var i = 0; i < 1000; i++)
            {
                var value = random.NextInt(3, 9);
                Assert.That(value, Is.GreaterThanOrEqualTo(3));
                Assert.That(value, Is.LessThan(9));
            }
        }

        [Test]
        public void SameSeedInputs_ReturnSameRunSeed()
        {
            var first = MazeSeedService.Create("player", "chapter-01", 0, 17);
            var second = MazeSeedService.Create("player", "chapter-01", 0, 17);

            Assert.AreEqual(first, second);
        }

        [Test]
        public void AttemptIndex_ChangesRunSeed()
        {
            var first = MazeSeedService.Create("player", "chapter-01", 0, 17);
            var second = MazeSeedService.Create("player", "chapter-01", 1, 17);

            Assert.AreNotEqual(first, second);
        }
    }
}
