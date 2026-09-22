using System;
using System.Collections.Generic;

namespace MazeMath.Maze
{
    public sealed class MazeGraph
    {
        private readonly Dictionary<string, MazeNode> nodes = new Dictionary<string, MazeNode>();
        private readonly Dictionary<string, MazeEdge> edges = new Dictionary<string, MazeEdge>();
        private readonly Dictionary<string, List<MazeEdge>> adjacency = new Dictionary<string, List<MazeEdge>>();

        public IReadOnlyDictionary<string, MazeNode> Nodes => nodes;
        public IReadOnlyDictionary<string, MazeEdge> Edges => edges;

        public void AddNode(MazeNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (!nodes.TryAdd(node.NodeId, node))
            {
                throw new InvalidOperationException("Duplicate maze node: " + node.NodeId);
            }

            adjacency[node.NodeId] = new List<MazeEdge>();
        }

        public void AddEdge(MazeEdge edge)
        {
            if (edge == null)
            {
                throw new ArgumentNullException(nameof(edge));
            }

            if (!nodes.ContainsKey(edge.FromNodeId) || !nodes.ContainsKey(edge.ToNodeId))
            {
                throw new InvalidOperationException(
                    $"Maze edge {edge.EdgeId} references a missing endpoint.");
            }

            if (!edges.TryAdd(edge.EdgeId, edge))
            {
                throw new InvalidOperationException("Duplicate maze edge: " + edge.EdgeId);
            }

            adjacency[edge.FromNodeId].Add(edge);
            if (edge.IsBidirectional)
            {
                adjacency[edge.ToNodeId].Add(edge);
            }
        }

        public IReadOnlyList<string> FindPath(string fromNodeId, string toNodeId)
        {
            if (!nodes.ContainsKey(fromNodeId) || !nodes.ContainsKey(toNodeId))
            {
                return Array.Empty<string>();
            }

            if (fromNodeId == toNodeId)
            {
                return new[] { fromNodeId };
            }

            var queue = new Queue<string>();
            var visited = new HashSet<string> { fromNodeId };
            var previous = new Dictionary<string, string>();

            queue.Enqueue(fromNodeId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var edge in adjacency[current])
                {
                    var next = GetTraversableNeighbor(current, edge);
                    if (next == null || !visited.Add(next))
                    {
                        continue;
                    }

                    previous[next] = current;
                    if (next == toNodeId)
                    {
                        return BuildPath(previous, fromNodeId, toNodeId);
                    }

                    queue.Enqueue(next);
                }
            }

            return Array.Empty<string>();
        }

        public IReadOnlyList<MazeEdge> GetEdges(string nodeId)
        {
            return adjacency.TryGetValue(nodeId, out var list)
                ? list
                : Array.Empty<MazeEdge>();
        }

        private static string GetTraversableNeighbor(string current, MazeEdge edge)
        {
            if (edge.FromNodeId == current)
            {
                return edge.ToNodeId;
            }

            if (edge.IsBidirectional && edge.ToNodeId == current)
            {
                return edge.FromNodeId;
            }

            return null;
        }

        private static IReadOnlyList<string> BuildPath(
            IReadOnlyDictionary<string, string> previous,
            string start,
            string target)
        {
            var path = new List<string> { target };
            var cursor = target;

            while (cursor != start)
            {
                cursor = previous[cursor];
                path.Add(cursor);
            }

            path.Reverse();
            return path;
        }
    }
}
