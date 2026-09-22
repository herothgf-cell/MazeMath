using System;
using System.Linq;
using MazeMath.Maze.Data;
using MazeMath.Maze.Generation;
using NUnit.Framework;

namespace MazeMath.CoreTests;

public sealed class MazeGraphTests
{
    [Test]
    public void FindPath_ReturnsStartToBossPath()
    {
        var graph = new MazeGraph("start", "boss");
        graph.AddNode(new MazeNode("start", "start-room", 1, RoomType.Start, true));
        graph.AddNode(new MazeNode("middle", "corridor", 1, RoomType.Corridor, true));
        graph.AddNode(new MazeNode("boss", "boss-room", 1, RoomType.Boss, true));

        graph.AddEdge(new MazeEdge("e1", "start", "middle", EdgeType.Open, null, true));
        graph.AddEdge(new MazeEdge("e2", "middle", "boss", EdgeType.Open, null, true));

        Assert.That(graph.FindPath("start", "boss"),
            Is.EqualTo(new[] { "start", "middle", "boss" }));
    }

    [Test]
    public void AddNode_DuplicateId_Throws()
    {
        var graph = new MazeGraph("start", "boss");
        graph.AddNode(new MazeNode("start", "start-room", 1, RoomType.Start, true));

        Assert.Throws<InvalidOperationException>(() =>
            graph.AddNode(new MazeNode("start", "other", 1, RoomType.Corridor, false)));
    }

    [Test]
    public void AddEdge_MissingEndpoint_Throws()
    {
        var graph = new MazeGraph("start", "boss");
        graph.AddNode(new MazeNode("start", "start-room", 1, RoomType.Start, true));

        Assert.Throws<InvalidOperationException>(() =>
            graph.AddEdge(new MazeEdge("e1", "start", "missing", EdgeType.Open, null, true)));
    }

    [Test]
    public void FindPath_UnreachableTarget_ReturnsEmpty()
    {
        var graph = new MazeGraph("start", "boss");
        graph.AddNode(new MazeNode("start", "start-room", 1, RoomType.Start, true));
        graph.AddNode(new MazeNode("boss", "boss-room", 1, RoomType.Boss, true));

        Assert.That(graph.FindPath("start", "boss"), Is.Empty);
    }

    [Test]
    public void BidirectionalEdge_CanBeTraversedBothWays()
    {
        var graph = new MazeGraph("a", "b");
        graph.AddNode(new MazeNode("a", "a-room", 1, RoomType.Start, true));
        graph.AddNode(new MazeNode("b", "b-room", 1, RoomType.Boss, true));
        graph.AddEdge(new MazeEdge("e1", "a", "b", EdgeType.Open, null, true));

        Assert.That(graph.FindPath("a", "b"), Is.EqualTo(new[] { "a", "b" }));
        Assert.That(graph.FindPath("b", "a"), Is.EqualTo(new[] { "b", "a" }));
        Assert.That(graph.GetOutgoingEdges("b").Single().EdgeId, Is.EqualTo("e1"));
    }
}
