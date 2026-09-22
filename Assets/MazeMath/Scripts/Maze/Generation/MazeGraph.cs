using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeGraph
    {
        private readonly Dictionary<string, MazeNode> _nodes = new Dictionary<string, MazeNode>();
        private readonly Dictionary<string, MazeEdge> _edges = new Dictionary<string, MazeEdge>();
        private readonly Dictionary<string, List<string>> _adjacency = new Dictionary<string, List<string>>();
        private readonly List<MazeRequirementPlacement> _requirements = new List<MazeRequirementPlacement>();

        public MazeGraph(
            string startNodeId,
            string bossNodeId,
            int requiredBacktrackCount = 0)
        {
            if (string.IsNullOrWhiteSpace(startNodeId))
                throw new ArgumentException("Start node ID is required.", nameof(startNodeId));
            if (string.IsNullOrWhiteSpace(bossNodeId))
                throw new ArgumentException("Boss node ID is required.", nameof(bossNodeId));
            if (requiredBacktrackCount < 0)
                throw new ArgumentOutOfRangeException(nameof(requiredBacktrackCount));

            StartNodeId = startNodeId;
            BossNodeId = bossNodeId;
            RequiredBacktrackCount = requiredBacktrackCount;
        }

        public string StartNodeId { get; }
        public string BossNodeId { get; }
        public int RequiredBacktrackCount { get; }
        public IReadOnlyCollection<MazeNode> Nodes => _nodes.Values;
        public IReadOnlyCollection<MazeEdge> Edges => _edges.Values;
        public IReadOnlyList<MazeRequirementPlacement> Requirements => _requirements;

        public void AddNode(MazeNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            if (_nodes.ContainsKey(node.NodeId))
                throw new InvalidOperationException($"Node '{node.NodeId}' already exists.");

            _nodes.Add(node.NodeId, node);
            _adjacency.Add(node.NodeId, new List<string>());
        }

        public void AddEdge(MazeEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (_edges.ContainsKey(edge.EdgeId))
                throw new InvalidOperationException($"Edge '{edge.EdgeId}' already exists.");
            if (!_nodes.ContainsKey(edge.FromNodeId) || !_nodes.ContainsKey(edge.ToNodeId))
                throw new InvalidOperationException("Both edge endpoints must exist before adding the edge.");

            _edges.Add(edge.EdgeId, edge);
            _adjacency[edge.FromNodeId].Add(edge.EdgeId);

            if (edge.IsBidirectional)
                _adjacency[edge.ToNodeId].Add(edge.EdgeId);
        }

        public void AddRequirement(MazeRequirementPlacement placement)
        {
            if (placement == null)
                throw new ArgumentNullException(nameof(placement));
            if (!_nodes.ContainsKey(placement.NodeId))
                throw new InvalidOperationException(
                    $"Requirement node '{placement.NodeId}' does not exist.");
            if (_requirements.Any(r =>
                r.RequirementId == placement.RequirementId &&
                r.NodeId == placement.NodeId))
            {
                throw new InvalidOperationException(
                    $"Requirement '{placement.RequirementId}' already exists at '{placement.NodeId}'.");
            }

            _requirements.Add(placement);
        }

        public MazeNode GetNode(string nodeId)
        {
            if (!_nodes.TryGetValue(nodeId, out var node))
                throw new KeyNotFoundException($"Node '{nodeId}' does not exist.");

            return node;
        }

        public IReadOnlyList<MazeEdge> GetOutgoingEdges(string nodeId)
        {
            if (!_adjacency.TryGetValue(nodeId, out var edgeIds))
                throw new KeyNotFoundException($"Node '{nodeId}' does not exist.");

            return edgeIds.Select(id => _edges[id]).ToArray();
        }

        public IReadOnlyList<string> FindPath(string fromNodeId, string toNodeId)
        {
            return FindPath(fromNodeId, toNodeId, blockedRequirementId: null);
        }

        public IReadOnlyList<string> FindPath(
            string fromNodeId,
            string toNodeId,
            string? blockedRequirementId)
        {
            if (!_nodes.ContainsKey(fromNodeId) || !_nodes.ContainsKey(toNodeId))
                return Array.Empty<string>();

            var queue = new Queue<string>();
            var visited = new HashSet<string>();
            var previous = new Dictionary<string, string>();

            queue.Enqueue(fromNodeId);
            visited.Add(fromNodeId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == toNodeId)
                    return ReconstructPath(previous, fromNodeId, toNodeId);

                foreach (var edgeId in _adjacency[current])
                {
                    var edge = _edges[edgeId];
                    if (blockedRequirementId != null &&
                        edge.RequirementId == blockedRequirementId)
                    {
                        continue;
                    }

                    var next = ResolveNeighbor(current, edge);
                    if (next == null || !visited.Add(next))
                        continue;

                    previous[next] = current;
                    queue.Enqueue(next);
                }
            }

            return Array.Empty<string>();
        }

        private static string? ResolveNeighbor(string currentNodeId, MazeEdge edge)
        {
            if (edge.FromNodeId == currentNodeId)
                return edge.ToNodeId;

            if (edge.IsBidirectional && edge.ToNodeId == currentNodeId)
                return edge.FromNodeId;

            return null;
        }

        private static IReadOnlyList<string> ReconstructPath(
            IReadOnlyDictionary<string, string> previous,
            string fromNodeId,
            string toNodeId)
        {
            var path = new List<string> { toNodeId };
            var current = toNodeId;

            while (current != fromNodeId)
            {
                current = previous[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }
    }
}
