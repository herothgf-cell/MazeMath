using System.Collections.Generic;
using MazeMath.Maze.Data;
using MazeMath.Maze.Generation;
using MazeMath.Maze.Validation;
using NUnit.Framework;

namespace MazeMath.CoreTests;

public sealed class MazeGenerationCoordinatorTests
{
    [Test]
    public void GenerateValidated_AfterTenInvalidAttempts_ReturnsSafeLayout()
    {
        var coordinator = new MazeGenerationCoordinator(
            new AlwaysInvalidGenerator(),
            new AlwaysInvalidValidator(),
            new MazeSeedService(),
            new SafeMazeFactory());

        var result = coordinator.GenerateValidated(
            profileId: "default",
            chapterId: "chapter-1",
            baseSeedOffset: 10,
            settings: MazeGenerationSettings.Chapter1Defaults());

        Assert.That(result.UsedSafeLayout, Is.True);
        Assert.That(result.AttemptCount, Is.EqualTo(10));

        var validation = new MazeValidator().Validate(
            result.Graph,
            MazeGenerationSettings.Chapter1Defaults());

        Assert.That(validation.IsValid, Is.True);
    }

    [Test]
    public void GenerateValidated_ValidFirstAttempt_DoesNotUseFallback()
    {
        var coordinator = new MazeGenerationCoordinator(
            new MazeGenerator(),
            new MazeValidator(),
            new MazeSeedService(),
            new SafeMazeFactory());

        var result = coordinator.GenerateValidated(
            "default",
            "chapter-1",
            1,
            MazeGenerationSettings.Chapter1Defaults());

        Assert.That(result.UsedSafeLayout, Is.False);
        Assert.That(result.AttemptCount, Is.EqualTo(1));
    }

    [Test]
    public void GenerateValidated_OneThousandSeeds_AllReturnValidGraph()
    {
        var validator = new MazeValidator();
        var coordinator = new MazeGenerationCoordinator(
            new MazeGenerator(),
            validator,
            new MazeSeedService(),
            new SafeMazeFactory());
        var settings = MazeGenerationSettings.Chapter1Defaults();

        for (var i = 0; i < 1000; i++)
        {
            var result = coordinator.GenerateValidated(
                "default",
                "chapter-1",
                i,
                settings);

            var validation = validator.Validate(result.Graph, settings);

            Assert.That(
                validation.IsValid,
                Is.True,
                $"seed offset {i}: {string.Join(", ", validation.ErrorCodes)}");
        }
    }

    private sealed class AlwaysInvalidGenerator : IMazeGenerator
    {
        public MazeGenerationResult Generate(MazeGenerationRequest request)
        {
            var graph = new MazeGraph("start", "boss");
            graph.AddNode(new MazeNode("start", "start", 1, RoomType.Start, true));
            graph.AddNode(new MazeNode("boss", "boss", 1, RoomType.Boss, true));
            return new MazeGenerationResult(graph, request.Seed);
        }
    }

    private sealed class AlwaysInvalidValidator : IMazeValidator
    {
        public MazeValidationResult Validate(
            MazeGraph graph,
            MazeGenerationSettings settings)
        {
            return new MazeValidationResult(new[]
            {
                new MazeValidationIssue(
                    MazeValidationErrorCode.NoPathToBoss,
                    "forced invalid graph")
            });
        }
    }
}
