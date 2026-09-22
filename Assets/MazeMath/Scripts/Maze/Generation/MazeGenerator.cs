using System;
using System.Linq;
using MazeMath.Core.Determinism;
using MazeMath.Maze.Data;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeGenerator : IMazeGenerator
    {
        private static readonly RoomType[] IntermediateRoomTypes =
        {
            RoomType.Corridor,
            RoomType.Junction,
            RoomType.Question,
            RoomType.Puzzle,
            RoomType.Workshop
        };

        private static readonly RoomType[] OptionalRoomTypes =
        {
            RoomType.Reward,
            RoomType.Secret,
            RoomType.Question
        };

        public MazeGenerationResult Generate(MazeGenerationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var settings = request.Settings;
            var rng = new DeterministicRandom(request.Seed);
            var roomCount = rng.NextInt(
                settings.MinCriticalPathRooms,
                settings.MaxCriticalPathRooms + 1);

            var startId = "c00-start";
            var bossId = $"c{roomCount - 1:00}-boss";
            var graph = new MazeGraph(startId, bossId);

            for (var i = 0; i < roomCount; i++)
            {
                var type = ResolveRoomType(i, roomCount, rng);
                var floor = ResolveFloor(i, roomCount, settings.FloorCount);
                var nodeId = ResolveNodeId(i, roomCount, type);

                graph.AddNode(new MazeNode(
                    nodeId,
                    TemplateIdFor(type),
                    floor,
                    type,
                    isCriticalPath: true));
            }

            for (var i = 0; i < roomCount - 1; i++)
            {
                var fromId = NodeIdAt(graph, i);
                var toId = NodeIdAt(graph, i + 1);
                var from = graph.GetNode(fromId);
                var to = graph.GetNode(toId);
                var edgeType = from.Floor == to.Floor ? EdgeType.Open : EdgeType.Ladder;

                graph.AddEdge(new MazeEdge(
                    $"e{i:00}",
                    fromId,
                    toId,
                    edgeType,
                    gateId: null,
                    isBidirectional: true));
            }

            AddOptionalBranches(graph, settings, rng);

            return new MazeGenerationResult(graph, request.Seed);
        }

        private static void AddOptionalBranches(
            MazeGraph graph,
            MazeGenerationSettings settings,
            DeterministicRandom rng)
        {
            if (settings.OptionalRoomCount == 0)
                return;

            var criticalCandidates = graph.Nodes
                .Where(node =>
                    node.IsCriticalPath &&
                    node.Type != RoomType.Start &&
                    node.Type != RoomType.Boss &&
                    node.Type != RoomType.Checkpoint)
                .OrderBy(node => node.NodeId)
                .ToArray();

            if (criticalCandidates.Length == 0)
                throw new InvalidOperationException("No critical node is available for optional branches.");

            string? previousDepthOneOptionalId = null;

            for (var i = 0; i < settings.OptionalRoomCount; i++)
            {
                var attachToPreviousOptional =
                    settings.MaxOptionalBranchDepth == 2 &&
                    previousDepthOneOptionalId != null &&
                    i % 2 == 1;

                string parentNodeId;
                if (attachToPreviousOptional)
                {
                    parentNodeId = previousDepthOneOptionalId!;
                }
                else
                {
                    parentNodeId = criticalCandidates[
                        rng.NextInt(0, criticalCandidates.Length)].NodeId;
                }

                var type = OptionalRoomTypes[
                    rng.NextInt(0, OptionalRoomTypes.Length)];
                var nodeId = $"o{i:00}-{type.ToString().ToLowerInvariant()}";
                var floor = graph.GetNode(parentNodeId).Floor;

                graph.AddNode(new MazeNode(
                    nodeId,
                    TemplateIdFor(type),
                    floor,
                    type,
                    isCriticalPath: false));

                graph.AddEdge(new MazeEdge(
                    $"oe{i:00}",
                    parentNodeId,
                    nodeId,
                    EdgeType.Open,
                    gateId: null,
                    isBidirectional: true));

                previousDepthOneOptionalId =
                    attachToPreviousOptional ? null : nodeId;
            }
        }

        private static RoomType ResolveRoomType(
            int index,
            int roomCount,
            DeterministicRandom rng)
        {
            if (index == 0)
                return RoomType.Start;
            if (index == roomCount - 1)
                return RoomType.Boss;
            if (index == roomCount - 2)
                return RoomType.Checkpoint;

            return IntermediateRoomTypes[
                rng.NextInt(0, IntermediateRoomTypes.Length)];
        }

        private static int ResolveFloor(int index, int roomCount, int floorCount)
        {
            var floor = 1 + (index * floorCount / roomCount);
            return Math.Min(floor, floorCount);
        }

        private static string ResolveNodeId(int index, int roomCount, RoomType type)
        {
            if (index == 0)
                return "c00-start";
            if (index == roomCount - 1)
                return $"c{index:00}-boss";

            return $"c{index:00}-{type.ToString().ToLowerInvariant()}";
        }

        private static string TemplateIdFor(RoomType type)
        {
            return $"{type.ToString().ToLowerInvariant()}-room";
        }

        private static string NodeIdAt(MazeGraph graph, int index)
        {
            foreach (var node in graph.Nodes)
            {
                if (node.NodeId.StartsWith($"c{index:00}-", StringComparison.Ordinal))
                    return node.NodeId;
            }

            throw new InvalidOperationException($"Critical node at index {index} was not created.");
        }
    }
}
