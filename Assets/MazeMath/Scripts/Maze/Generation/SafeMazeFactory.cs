using System;
using MazeMath.Maze.Data;

namespace MazeMath.Maze.Generation
{
    public sealed class SafeMazeFactory
    {
        private static readonly RoomType[] SafeIntermediateTypes =
        {
            RoomType.Corridor,
            RoomType.Question,
            RoomType.Junction,
            RoomType.Puzzle,
            RoomType.Workshop
        };

        public MazeGraph CreateChapter1SafeLayout()
        {
            return CreateSafeLayout(MazeGenerationSettings.Chapter1Defaults());
        }

        public MazeGraph CreateSafeLayout(MazeGenerationSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var minimumForBossCheckpoint = settings.RequireCheckpointBeforeBoss ? 3 : 2;
            var roomCount = Math.Max(settings.MinCriticalPathRooms, minimumForBossCheckpoint);

            if (roomCount > settings.MaxCriticalPathRooms)
            {
                throw new InvalidOperationException(
                    "Maze settings cannot produce a safe layout inside the configured critical-path bounds.");
            }

            var startId = "c00-start";
            var bossId = $"c{roomCount - 1:00}-boss";
            var graph = new MazeGraph(startId, bossId);

            for (var i = 0; i < roomCount; i++)
            {
                var type = ResolveRoomType(i, roomCount, settings.RequireCheckpointBeforeBoss);
                var floor = ResolveFloor(i, roomCount, settings.FloorCount);
                var nodeId = ResolveNodeId(i, roomCount, type);

                graph.AddNode(new MazeNode(
                    nodeId,
                    $"{type.ToString().ToLowerInvariant()}-room",
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
                var edgeType = from.Floor == to.Floor
                    ? EdgeType.Open
                    : EdgeType.Ladder;

                graph.AddEdge(new MazeEdge(
                    $"e{i:00}",
                    fromId,
                    toId,
                    edgeType,
                    gateId: null,
                    isBidirectional: true));
            }

            AddOptionalRooms(graph, settings.OptionalRoomCount, roomCount);
            return graph;
        }

        private static void AddOptionalRooms(
            MazeGraph graph,
            int optionalRoomCount,
            int criticalRoomCount)
        {
            if (optionalRoomCount == 0)
                return;

            var firstEligibleIndex = criticalRoomCount > 3 ? 1 : 0;
            var lastEligibleIndex = criticalRoomCount > 3
                ? criticalRoomCount - 3
                : 0;
            var eligibleCount = lastEligibleIndex - firstEligibleIndex + 1;

            for (var i = 0; i < optionalRoomCount; i++)
            {
                var parentIndex = firstEligibleIndex + (i % eligibleCount);
                var parentId = NodeIdAt(graph, parentIndex);
                var nodeId = $"o{i:00}-reward";
                var floor = graph.GetNode(parentId).Floor;

                graph.AddNode(new MazeNode(
                    nodeId,
                    "reward-room",
                    floor,
                    RoomType.Reward,
                    isCriticalPath: false));

                graph.AddEdge(new MazeEdge(
                    $"oe{i:00}",
                    parentId,
                    nodeId,
                    EdgeType.Open,
                    gateId: null,
                    isBidirectional: true));
            }
        }

        private static RoomType ResolveRoomType(
            int index,
            int roomCount,
            bool requireCheckpointBeforeBoss)
        {
            if (index == 0)
                return RoomType.Start;
            if (index == roomCount - 1)
                return RoomType.Boss;
            if (requireCheckpointBeforeBoss && index == roomCount - 2)
                return RoomType.Checkpoint;

            return SafeIntermediateTypes[
                (index - 1) % SafeIntermediateTypes.Length];
        }

        private static int ResolveFloor(
            int index,
            int roomCount,
            int floorCount)
        {
            if (floorCount == 1 || roomCount == 1)
                return 1;

            return 1 + (index * (floorCount - 1) / (roomCount - 1));
        }

        private static string ResolveNodeId(
            int index,
            int roomCount,
            RoomType type)
        {
            if (index == 0)
                return "c00-start";
            if (index == roomCount - 1)
                return $"c{index:00}-boss";

            return $"c{index:00}-{type.ToString().ToLowerInvariant()}";
        }

        private static string NodeIdAt(MazeGraph graph, int index)
        {
            var prefix = $"c{index:00}-";

            foreach (var node in graph.Nodes)
            {
                if (node.NodeId.StartsWith(prefix, StringComparison.Ordinal))
                    return node.NodeId;
            }

            throw new InvalidOperationException(
                $"Critical node at index {index} was not created.");
        }
    }
}
