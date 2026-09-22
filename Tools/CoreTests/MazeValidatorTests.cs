using System.Linq;
using MazeMath.Maze.Data;
using MazeMath.Maze.Generation;
using MazeMath.Maze.Validation;
using NUnit.Framework;

namespace MazeMath.CoreTests;

public sealed class MazeValidatorTests
{
    [Test]
    public void Validate_GeneratedChapterOne_IsValid()
    {
        var settings = MazeGenerationSettings.Chapter1Defaults();
        var graph = new MazeGenerator()
            .Generate(new MazeGenerationRequest("chapter-1", 123u, settings))
            .Graph;

        var result = new MazeValidator().Validate(graph, settings);

        Assert.That(result.IsValid, Is.True, string.Join(", ", result.Issues.Select(i => i.Code)));
    }

    [Test]
    public void Validate_DisconnectedBoss_ReportsNoPath()
    {
        var graph = new MazeGraph("start", "boss");
        graph.AddNode(new MazeNode("start", "start", 1, RoomType.Start, true));
        graph.AddNode(new MazeNode("boss", "boss", 1, RoomType.Boss, true));

        var result = new MazeValidator().Validate(
            graph,
            MazeGenerationSettings.Chapter1Defaults());

        Assert.That(result.ErrorCodes, Does.Contain(MazeValidationErrorCode.NoPathToBoss));
    }

    [Test]
    public void Validate_LadderSkippingFloor_ReportsInvalidFloorTransition()
    {
        var graph = new MazeGraph("start", "boss");
        graph.AddNode(new MazeNode("start", "start", 1, RoomType.Start, true));
        graph.AddNode(new MazeNode("checkpoint", "checkpoint", 3, RoomType.Checkpoint, true));
        graph.AddNode(new MazeNode("boss", "boss", 3, RoomType.Boss, true));
        graph.AddEdge(new MazeEdge("e1", "start", "checkpoint", EdgeType.Ladder, null, true));
        graph.AddEdge(new MazeEdge("e2", "checkpoint", "boss", EdgeType.Open, null, true));

        var settings = new MazeGenerationSettings(3, 3, 0, 3, 1);
        var result = new MazeValidator().Validate(graph, settings);

        Assert.That(result.ErrorCodes, Does.Contain(MazeValidationErrorCode.InvalidFloorTransition));
    }

    [Test]
    public void Validate_MissingCheckpointBeforeBoss_ReportsIssue()
    {
        var graph = new MazeGraph("start", "boss");
        graph.AddNode(new MazeNode("start", "start", 1, RoomType.Start, true));
        graph.AddNode(new MazeNode("middle", "corridor", 1, RoomType.Corridor, true));
        graph.AddNode(new MazeNode("boss", "boss", 1, RoomType.Boss, true));
        graph.AddEdge(new MazeEdge("e1", "start", "middle", EdgeType.Open, null, true));
        graph.AddEdge(new MazeEdge("e2", "middle", "boss", EdgeType.Open, null, true));

        var result = new MazeValidator().Validate(
            graph,
            new MazeGenerationSettings(3, 3, 0, 1, 1, requireCheckpointBeforeBoss: true));

        Assert.That(result.ErrorCodes, Does.Contain(MazeValidationErrorCode.MissingCheckpointBeforeBoss));
    }

    [Test]
    public void Validate_RequirementBehindOwnGate_ReportsSelfLock()
    {
        var graph = new MazeGraph("start", "boss");
        graph.AddNode(new MazeNode("start", "start", 1, RoomType.Start, true));
        graph.AddNode(new MazeNode("key-room", "reward", 1, RoomType.Reward, true));
        graph.AddNode(new MazeNode("checkpoint", "checkpoint", 1, RoomType.Checkpoint, true));
        graph.AddNode(new MazeNode("boss", "boss", 1, RoomType.Boss, true));

        graph.AddEdge(new MazeEdge(
            "locked", "start", "key-room", EdgeType.Locked, "gate-a", true,
            requirementId: "key-a"));
        graph.AddEdge(new MazeEdge("e2", "key-room", "checkpoint", EdgeType.Open, null, true));
        graph.AddEdge(new MazeEdge("e3", "checkpoint", "boss", EdgeType.Open, null, true));
        graph.AddRequirement(new MazeRequirementPlacement("key-a", "key-room"));

        var result = new MazeValidator().Validate(
            graph,
            new MazeGenerationSettings(4, 4, 0, 1, 1));

        Assert.That(result.ErrorCodes, Does.Contain(MazeValidationErrorCode.RequirementBehindOwnGate));
    }

    [Test]
    public void Validate_TwoRequirementsDependingOnEachOther_ReportsCycle()
    {
        var graph = new MazeGraph("start", "boss");
        graph.AddNode(new MazeNode("start", "start", 1, RoomType.Start, true));
        graph.AddNode(new MazeNode("a-room", "reward", 1, RoomType.Reward, true));
        graph.AddNode(new MazeNode("b-room", "reward", 1, RoomType.Reward, true));
        graph.AddNode(new MazeNode("checkpoint", "checkpoint", 1, RoomType.Checkpoint, true));
        graph.AddNode(new MazeNode("boss", "boss", 1, RoomType.Boss, true));

        graph.AddEdge(new MazeEdge(
            "gate-a", "start", "a-room", EdgeType.Locked, "gate-a", true,
            requirementId: "key-b"));
        graph.AddEdge(new MazeEdge(
            "gate-b", "start", "b-room", EdgeType.Locked, "gate-b", true,
            requirementId: "key-a"));
        graph.AddEdge(new MazeEdge("a-next", "a-room", "checkpoint", EdgeType.Open, null, true));
        graph.AddEdge(new MazeEdge("b-next", "b-room", "checkpoint", EdgeType.Open, null, true));
        graph.AddEdge(new MazeEdge("boss-edge", "checkpoint", "boss", EdgeType.Open, null, true));

        graph.AddRequirement(new MazeRequirementPlacement("key-a", "a-room"));
        graph.AddRequirement(new MazeRequirementPlacement("key-b", "b-room"));

        var result = new MazeValidator().Validate(
            graph,
            new MazeGenerationSettings(5, 5, 0, 1, 1));

        Assert.That(result.ErrorCodes, Does.Contain(MazeValidationErrorCode.GateDependencyCycle));
    }

    [Test]
    public void Validate_RequiredBacktrackOverLimit_ReportsExcessiveBacktracking()
    {
        var graph = new MazeGraph("start", "boss", requiredBacktrackCount: 2);
        graph.AddNode(new MazeNode("start", "start", 1, RoomType.Start, true));
        graph.AddNode(new MazeNode("checkpoint", "checkpoint", 1, RoomType.Checkpoint, true));
        graph.AddNode(new MazeNode("boss", "boss", 1, RoomType.Boss, true));
        graph.AddEdge(new MazeEdge("e1", "start", "checkpoint", EdgeType.Open, null, true));
        graph.AddEdge(new MazeEdge("e2", "checkpoint", "boss", EdgeType.Open, null, true));

        var result = new MazeValidator().Validate(
            graph,
            new MazeGenerationSettings(3, 3, 0, 1, maxRequiredBacktracks: 1));

        Assert.That(result.ErrorCodes, Does.Contain(MazeValidationErrorCode.ExcessiveBacktracking));
    }
}
