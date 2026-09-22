using System;
using System.Collections.Generic;
using System.Linq;
using MazeMath.Maze.Data;
using MazeMath.Maze.Generation;

namespace MazeMath.Maze.Validation
{
    public sealed class MazeValidator : IMazeValidator
    {
        public MazeValidationResult Validate(
            MazeGraph graph,
            MazeGenerationSettings settings)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var issues = new List<MazeValidationIssue>();
            var hasStart = graph.Nodes.Any(node => node.NodeId == graph.StartNodeId);
            var hasBoss = graph.Nodes.Any(node => node.NodeId == graph.BossNodeId);

            if (!hasStart)
                Add(issues, MazeValidationErrorCode.MissingStart, "Start node is missing.");
            if (!hasBoss)
                Add(issues, MazeValidationErrorCode.MissingBoss, "Boss node is missing.");

            if (hasStart && hasBoss)
            {
                ValidateCriticalPath(graph, settings, issues);
                ValidateRequirementDependencies(graph, issues);
            }

            ValidateFloorTransitions(graph, issues);

            if (graph.RequiredBacktrackCount > settings.MaxRequiredBacktracks)
            {
                Add(
                    issues,
                    MazeValidationErrorCode.ExcessiveBacktracking,
                    $"Required backtracks {graph.RequiredBacktrackCount} exceed limit {settings.MaxRequiredBacktracks}.");
            }

            return new MazeValidationResult(issues);
        }

        private static void ValidateCriticalPath(
            MazeGraph graph,
            MazeGenerationSettings settings,
            ICollection<MazeValidationIssue> issues)
        {
            var path = graph.FindPath(graph.StartNodeId, graph.BossNodeId);
            if (path.Count == 0)
            {
                Add(issues, MazeValidationErrorCode.NoPathToBoss, "Boss is unreachable from Start.");
                return;
            }

            if (!settings.RequireCheckpointBeforeBoss || path.Count < 2)
                return;

            var previousNode = graph.GetNode(path[path.Count - 2]);
            if (previousNode.Type != RoomType.Checkpoint)
            {
                Add(
                    issues,
                    MazeValidationErrorCode.MissingCheckpointBeforeBoss,
                    "The structural Start-to-Boss path must enter a Checkpoint immediately before Boss.");
            }
        }

        private static void ValidateFloorTransitions(
            MazeGraph graph,
            ICollection<MazeValidationIssue> issues)
        {
            foreach (var edge in graph.Edges)
            {
                var from = graph.GetNode(edge.FromNodeId);
                var to = graph.GetNode(edge.ToNodeId);
                var delta = Math.Abs(from.Floor - to.Floor);

                if (delta == 0)
                    continue;

                var isTransition =
                    edge.Type == EdgeType.Ladder ||
                    edge.Type == EdgeType.Elevator;

                var ladderSkipsFloor =
                    edge.Type == EdgeType.Ladder &&
                    delta != 1;

                if (!isTransition || ladderSkipsFloor)
                {
                    Add(
                        issues,
                        MazeValidationErrorCode.InvalidFloorTransition,
                        $"Edge '{edge.EdgeId}' has invalid floor transition {from.Floor}->{to.Floor}.");
                }
            }
        }

        private static void ValidateRequirementDependencies(
            MazeGraph graph,
            ICollection<MazeValidationIssue> issues)
        {
            var dependencies = new Dictionary<string, HashSet<string>>();

            foreach (var placement in graph.Requirements)
            {
                var requiredOnBestPath = FindGateRequirementsOnBestPath(
                    graph,
                    graph.StartNodeId,
                    placement.NodeId);

                if (!dependencies.TryGetValue(placement.RequirementId, out var set))
                {
                    set = new HashSet<string>();
                    dependencies.Add(placement.RequirementId, set);
                }

                foreach (var requiredId in requiredOnBestPath)
                    set.Add(requiredId);

                if (set.Contains(placement.RequirementId))
                {
                    Add(
                        issues,
                        MazeValidationErrorCode.RequirementBehindOwnGate,
                        $"Requirement '{placement.RequirementId}' is behind a gate that requires itself.");
                }
            }

            if (ContainsDependencyCycle(dependencies))
            {
                Add(
                    issues,
                    MazeValidationErrorCode.GateDependencyCycle,
                    "Gate requirements contain a dependency cycle.");
            }
        }

        private static IReadOnlyCollection<string> FindGateRequirementsOnBestPath(
            MazeGraph graph,
            string startNodeId,
            string targetNodeId)
        {
            var nodes = graph.Nodes.Select(node => node.NodeId).ToArray();
            var costs = new Dictionary<string, PathCost>();
            var previous = new Dictionary<string, PathStep>();
            var visited = new HashSet<string>();

            foreach (var nodeId in nodes)
                costs[nodeId] = PathCost.Infinity;

            if (!costs.ContainsKey(startNodeId) || !costs.ContainsKey(targetNodeId))
                return Array.Empty<string>();

            costs[startNodeId] = new PathCost(0, 0);

            while (visited.Count < nodes.Length)
            {
                var current = SelectCheapestUnvisited(costs, visited);
                if (current == null || costs[current].IsInfinity)
                    break;

                if (current == targetNodeId)
                    break;

                visited.Add(current);

                foreach (var edge in graph.GetOutgoingEdges(current))
                {
                    var next = ResolveNeighbor(current, edge);
                    if (next == null || visited.Contains(next))
                        continue;

                    var nextCost = costs[current].Add(
                        edge.RequirementId == null ? 0 : 1,
                        1);

                    if (nextCost.CompareTo(costs[next]) >= 0)
                        continue;

                    costs[next] = nextCost;
                    previous[next] = new PathStep(current, edge);
                }
            }

            if (targetNodeId != startNodeId && !previous.ContainsKey(targetNodeId))
                return Array.Empty<string>();

            var requirements = new HashSet<string>();
            var cursor = targetNodeId;

            while (cursor != startNodeId)
            {
                var step = previous[cursor];
                if (!string.IsNullOrWhiteSpace(step.Edge.RequirementId))
                    requirements.Add(step.Edge.RequirementId!);

                cursor = step.PreviousNodeId;
            }

            return requirements;
        }

        private static string? SelectCheapestUnvisited(
            IReadOnlyDictionary<string, PathCost> costs,
            ISet<string> visited)
        {
            string? selected = null;
            var selectedCost = PathCost.Infinity;

            foreach (var pair in costs)
            {
                if (visited.Contains(pair.Key))
                    continue;

                if (selected == null || pair.Value.CompareTo(selectedCost) < 0)
                {
                    selected = pair.Key;
                    selectedCost = pair.Value;
                }
            }

            return selected;
        }

        private static string? ResolveNeighbor(string currentNodeId, MazeEdge edge)
        {
            if (edge.FromNodeId == currentNodeId)
                return edge.ToNodeId;

            if (edge.IsBidirectional && edge.ToNodeId == currentNodeId)
                return edge.FromNodeId;

            return null;
        }

        private static bool ContainsDependencyCycle(
            IReadOnlyDictionary<string, HashSet<string>> dependencies)
        {
            var states = new Dictionary<string, int>();

            foreach (var node in dependencies.Keys)
            {
                if (Visit(node, dependencies, states))
                    return true;
            }

            return false;
        }

        private static bool Visit(
            string node,
            IReadOnlyDictionary<string, HashSet<string>> dependencies,
            IDictionary<string, int> states)
        {
            if (states.TryGetValue(node, out var state))
            {
                if (state == 1)
                    return true;
                if (state == 2)
                    return false;
            }

            states[node] = 1;

            if (dependencies.TryGetValue(node, out var nextNodes))
            {
                foreach (var next in nextNodes)
                {
                    if (next == node)
                        continue;

                    if (Visit(next, dependencies, states))
                        return true;
                }
            }

            states[node] = 2;
            return false;
        }

        private static void Add(
            ICollection<MazeValidationIssue> issues,
            MazeValidationErrorCode code,
            string message)
        {
            if (issues.Any(issue => issue.Code == code))
                return;

            issues.Add(new MazeValidationIssue(code, message));
        }

        private readonly struct PathStep
        {
            public PathStep(string previousNodeId, MazeEdge edge)
            {
                PreviousNodeId = previousNodeId;
                Edge = edge;
            }

            public string PreviousNodeId { get; }
            public MazeEdge Edge { get; }
        }

        private readonly struct PathCost : IComparable<PathCost>
        {
            public static readonly PathCost Infinity =
                new PathCost(int.MaxValue, int.MaxValue);

            public PathCost(int gates, int steps)
            {
                Gates = gates;
                Steps = steps;
            }

            public int Gates { get; }
            public int Steps { get; }
            public bool IsInfinity => Gates == int.MaxValue;

            public PathCost Add(int gates, int steps)
            {
                if (IsInfinity)
                    return Infinity;

                return new PathCost(Gates + gates, Steps + steps);
            }

            public int CompareTo(PathCost other)
            {
                var gateComparison = Gates.CompareTo(other.Gates);
                return gateComparison != 0
                    ? gateComparison
                    : Steps.CompareTo(other.Steps);
            }
        }
    }
}
