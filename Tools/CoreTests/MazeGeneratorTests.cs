using System.Collections.Generic;
using System.Linq;
using MazeMath.Maze.Data;
using MazeMath.Maze.Generation;
using NUnit.Framework;

namespace MazeMath.CoreTests;

public sealed class MazeGeneratorTests
{
    [Test]
    public void Generate_CreatesPathWithinRequestedBounds()
    {
        var settings = new MazeGenerationSettings(
            minCriticalPathRooms: 6,
            maxCriticalPathRooms: 8,
            optionalRoomCount: 0,
            floorCount: 1,
            maxRequiredBacktracks: 1);

        var result = new MazeGenerator().Generate(
            new MazeGenerationRequest("chapter-1", 1234u, settings));

        var path = result.Graph.FindPath(
            result.Graph.StartNodeId,
            result.Graph.BossNodeId);

        Assert.That(path.Count, Is.InRange(6, 8));
        Assert.That(path.First(), Is.EqualTo(result.Graph.StartNodeId));
        Assert.That(path.Last(), Is.EqualTo(result.Graph.BossNodeId));
    }

    [Test]
    public void Generate_SameSeed_ReturnsIdenticalGraphSignature()
    {
        var settings = MazeGenerationSettings.Chapter1Defaults();
        var generator = new MazeGenerator();

        var a = generator.Generate(new MazeGenerationRequest("chapter-1", 777u, settings));
        var b = generator.Generate(new MazeGenerationRequest("chapter-1", 777u, settings));

        Assert.That(Signature(a.Graph), Is.EqualTo(Signature(b.Graph)));
    }

    [Test]
    public void Generate_DifferentSeeds_ProduceMoreThanOneLayoutAcrossSample()
    {
        var settings = MazeGenerationSettings.Chapter1Defaults();
        var generator = new MazeGenerator();
        var signatures = new HashSet<string>();

        for (uint seed = 1; seed <= 20; seed++)
            signatures.Add(Signature(generator.Generate(
                new MazeGenerationRequest("chapter-1", seed, settings)).Graph));

        Assert.That(signatures.Count, Is.GreaterThan(1));
    }

    [Test]
    public void Generate_OneFloor_AssignsEveryNodeToFloorOne()
    {
        var settings = new MazeGenerationSettings(6, 8, 0, 1, 1);
        var graph = new MazeGenerator()
            .Generate(new MazeGenerationRequest("chapter-1", 10u, settings))
            .Graph;

        Assert.That(graph.Nodes.All(n => n.Floor == 1), Is.True);
    }

    [Test]
    public void Generate_TwoFloors_CreatesFloorTransition()
    {
        var settings = new MazeGenerationSettings(8, 8, 0, 2, 1);
        var graph = new MazeGenerator()
            .Generate(new MazeGenerationRequest("chapter-2", 10u, settings))
            .Graph;

        Assert.That(graph.Nodes.Any(n => n.Floor == 1), Is.True);
        Assert.That(graph.Nodes.Any(n => n.Floor == 2), Is.True);
        Assert.That(graph.Edges.Any(e => e.Type == EdgeType.Ladder), Is.True);
    }

    private static string Signature(MazeGraph graph)
    {
        var nodes = graph.Nodes
            .OrderBy(n => n.NodeId)
            .Select(n => $"{n.NodeId}:{n.Type}:{n.Floor}:{n.IsCriticalPath}");

        var edges = graph.Edges
            .OrderBy(e => e.EdgeId)
            .Select(e => $"{e.EdgeId}:{e.FromNodeId}>{e.ToNodeId}:{e.Type}");

        return string.Join("|", nodes.Concat(edges));
    }
}
