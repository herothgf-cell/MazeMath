using System;
using System.Collections.Generic;
using System.Linq;
using MazeMath.Maze;
using MazeMath.Maze.Runtime;

namespace MazeMath.Maze.Map
{
    public enum MazeRevealState
    {
        Unknown = 0,
        Seen = 1,
        Visited = 2,
        Cleared = 3
    }

    public interface IMazeMapService
    {
        void MarkSeen(string nodeId);
        void MarkVisited(string nodeId);
        void MarkCleared(string nodeId);
        MazeRevealState GetState(string nodeId);
        IReadOnlyCollection<string> GetRevealedNodes(int floor);
    }

    public sealed class MazeMapService : IMazeMapService
    {
        private readonly MazeGraph graph;
        private readonly MazeRunState state;

        public MazeMapService(MazeGraph graph, MazeRunState state)
        {
            this.graph = graph ?? throw new ArgumentNullException(nameof(graph));
            this.state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public void MarkSeen(string nodeId)
        {
            EnsureNode(nodeId);
            if (GetState(nodeId) == MazeRevealState.Unknown)
            {
                AddUnique(state.seenNodeIds, nodeId);
            }
        }

        public void MarkVisited(string nodeId)
        {
            EnsureNode(nodeId);
            AddUnique(state.seenNodeIds, nodeId);
            AddUnique(state.visitedNodeIds, nodeId);
        }

        public void MarkCleared(string nodeId)
        {
            MarkVisited(nodeId);
            AddUnique(state.clearedNodeIds, nodeId);
        }

        public MazeRevealState GetState(string nodeId)
        {
            EnsureNode(nodeId);

            if (state.clearedNodeIds.Contains(nodeId))
            {
                return MazeRevealState.Cleared;
            }

            if (state.visitedNodeIds.Contains(nodeId))
            {
                return MazeRevealState.Visited;
            }

            if (state.seenNodeIds.Contains(nodeId))
            {
                return MazeRevealState.Seen;
            }

            return MazeRevealState.Unknown;
        }

        public IReadOnlyCollection<string> GetRevealedNodes(int floor)
        {
            return graph.Nodes.Values
                .Where(node => node.Floor == floor && GetState(node.NodeId) != MazeRevealState.Unknown)
                .Select(node => node.NodeId)
                .ToArray();
        }

        private void EnsureNode(string nodeId)
        {
            if (!graph.Nodes.ContainsKey(nodeId))
            {
                throw new InvalidOperationException("Unknown maze node: " + nodeId);
            }
        }

        private static void AddUnique(List<string> list, string value)
        {
            if (!list.Contains(value))
            {
                list.Add(value);
            }
        }
    }
}
