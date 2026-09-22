using MazeMath.Core.Determinism;
using MazeMath.Maze.Generation;
using NUnit.Framework;

namespace MazeMath.CoreTests;

public sealed class DeterminismTests
{
    [TestCase("", 2166136261u)]
    [TestCase("MazeMath", 1353196792u)]
    public void Fnv1A32_ReturnsGoldenValue(string value, uint expected)
    {
        Assert.That(StableHash.Fnv1A32(value), Is.EqualTo(expected));
    }

    [Test]
    public void DeterministicRandom_SameSeed_ReturnsSameSequence()
    {
        var a = new DeterministicRandom(123456789u);
        var b = new DeterministicRandom(123456789u);

        for (var i = 0; i < 100; i++)
            Assert.That(a.NextUInt(), Is.EqualTo(b.NextUInt()));
    }

    [Test]
    public void DeterministicRandom_NextInt_StaysInsideRequestedRange()
    {
        var rng = new DeterministicRandom(42u);

        for (var i = 0; i < 1000; i++)
        {
            var value = rng.NextInt(3, 8);
            Assert.That(value, Is.GreaterThanOrEqualTo(3).And.LessThan(8));
        }
    }

    [Test]
    public void DeterministicRandom_ZeroSeed_IsStillUsable()
    {
        var rng = new DeterministicRandom(0u);

        Assert.That(rng.NextUInt(), Is.Not.EqualTo(0u));
    }

    [Test]
    public void MazeSeedService_IsStableAndAttemptSensitive()
    {
        var service = new MazeSeedService();

        var first = service.CreateSeed("default", "chapter-1", 0, 100);
        var same = service.CreateSeed("default", "chapter-1", 0, 100);
        var retry = service.CreateSeed("default", "chapter-1", 1, 100);

        Assert.That(same, Is.EqualTo(first));
        Assert.That(retry, Is.Not.EqualTo(first));
    }
}
