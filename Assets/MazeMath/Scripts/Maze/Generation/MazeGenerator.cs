using System;
using System.Collections.Generic;
using System.Linq;
using MazeMath.Maze.Data;
using MazeMath.Maze;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeGenerator
    {
        public MazeGraph Generate(MazeChapterDefinition chapter, int seed)
        {
            if (chapter == null)
            {
                throw new ArgumentNullException(nameof(chapter));
            }

            var authoringErrors = chapter.ValidateAuthoring();
            if (authoringErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    "Maze chapter authoring is invalid: " + string.Join(" | ", authoringErrors));
            }

            var random = new DeterministicRandom(seed);
            var graph = new MazeGraph();

            var criticalCount = random.NextInt(
                chapter.requiredRoomCountRange.x,
                chapter.requiredRoomCountRange.y + 1);

            var startTemplate = chapter.allowedRooms.First(room => room.type == RoomType.Start);
            var bossTemplate = chapter.allowedRooms.First(room => room.type == RoomType.Boss);
            var criticalCandidates = chapter.allowedRooms
                .Where(room =>
                    room != null &&
                    room.canBeCriticalPath &&
                    room.type != RoomType.Start &&
                    room.type != RoomType.Boss)
                .ToList();

            if (criticalCount > 2 && criticalCandidates.Count == 0)
            {
                throw new InvalidOperationException(
                    "At least one non-start/non-boss critical room template is required.");
            }

            var criticalNodes = new List<MazeNode>(criticalCount);

            var start = CreateNode(
                "start",
                startTemplate,
                0,
                isCriticalPath: true);
            graph.AddNode(start);
            criticalNodes.Add(start);

            for (var index = 1; index < criticalCount - 1; index++)
            {
                var floor = CalculateFloor(index, criticalCount, chapter.floorCount);
                var candidates = criticalCandidates
                    .Where(room => room.SupportsFloor(floor))
                    .ToList();

                if (candidates.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"No critical room template supports floor {floor}.");
                }

                var template = PickWeighted(candidates, random);
                var node = CreateNode(
                    $"critical-{index:00}",
                    template,
                    floor,
                    isCriticalPath: true);

                graph.AddNode(node);
                criticalNodes.Add(node);
            }

            var bossFloor = CalculateFloor(criticalCount - 1, criticalCount, chapter.floorCount);
            if (!bossTemplate.SupportsFloor(bossFloor))
            {
                throw new InvalidOperationException(
                    $"Boss room template does not support floor {bossFloor}.");
            }

            var boss = CreateNode(
                "boss",
                bossTemplate,
                bossFloor,
                isCriticalPath: true);
            graph.AddNode(boss);
            criticalNodes.Add(boss);

            for (var index = 0; index < criticalNodes.Count - 1; index++)
            {
                var from = criticalNodes[index];
                var to = criticalNodes[index + 1];
                var edgeType = from.Floor == to.Floor ? EdgeType.Open : EdgeType.Ladder;

                graph.AddEdge(new MazeEdge(
                    $"critical-edge-{index:00}",
                    from.NodeId,
                    to.NodeId,
                    edgeType,
                    isBidirectional: true));
            }

            AddOptionalBranches(graph, chapter, criticalNodes, random);
            return graph;
        }

        private static void AddOptionalBranches(
            MazeGraph graph,
            MazeChapterDefinition chapter,
            IReadOnlyList<MazeNode> criticalNodes,
            DeterministicRandom random)
        {
            var optionalCount = random.NextInt(
                chapter.optionalRoomCountRange.x,
                chapter.optionalRoomCountRange.y + 1);

            if (optionalCount == 0)
            {
                return;
            }

            var optionalCandidates = chapter.allowedRooms
                .Where(room =>
                    room != null &&
                    room.canBeOptional &&
                    room.type != RoomType.Start &&
                    room.type != RoomType.Boss)
                .ToList();

            if (optionalCandidates.Count == 0)
            {
                throw new InvalidOperationException(
                    "Optional rooms are requested but no optional room template is available.");
            }

            for (var index = 0; index < optionalCount; index++)
            {
                var parent = criticalNodes[random.NextInt(0, criticalNodes.Count - 1)];
                var candidates = optionalCandidates
                    .Where(room => room.SupportsFloor(parent.Floor))
                    .ToList();

                if (candidates.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"No optional room template supports floor {parent.Floor}.");
                }

                var template = PickWeighted(candidates, random);
                var node = CreateNode(
                    $"optional-{index:00}",
                    template,
                    parent.Floor,
                    isCriticalPath: false);

                graph.AddNode(node);
                graph.AddEdge(new MazeEdge(
                    $"optional-edge-{index:00}",
                    parent.NodeId,
                    node.NodeId,
                    EdgeType.Open,
                    isBidirectional: true));
            }
        }

        private static MazeNode CreateNode(
            string nodeId,
            RoomTemplateDefinition template,
            int floor,
            bool isCriticalPath)
        {
            return new MazeNode(nodeId, floor, template.type)
            {
                IsCriticalPath = isCriticalPath,
                TemplateId = template.templateId
            };
        }

        private static int CalculateFloor(int index, int totalCriticalRooms, int floorCount)
        {
            if (floorCount <= 1)
            {
                return 0;
            }

            var floor = index * floorCount / totalCriticalRooms;
            return Math.Min(floorCount - 1, floor);
        }

        private static RoomTemplateDefinition PickWeighted(
            IReadOnlyList<RoomTemplateDefinition> candidates,
            DeterministicRandom random)
        {
            var totalWeight = 0;
            for (var i = 0; i < candidates.Count; i++)
            {
                totalWeight += Math.Max(1, candidates[i].weight);
            }

            var roll = random.NextInt(0, totalWeight);
            for (var i = 0; i < candidates.Count; i++)
            {
                roll -= Math.Max(1, candidates[i].weight);
                if (roll < 0)
                {
                    return candidates[i];
                }
            }

            return candidates[candidates.Count - 1];
        }
    }
}
