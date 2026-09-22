using System;
using MazeMath.Maze.Data;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeEdge
    {
        public MazeEdge(
            string edgeId,
            string fromNodeId,
            string toNodeId,
            EdgeType type,
            string? gateId,
            bool isBidirectional,
            string? requirementId = null)
        {
            if (string.IsNullOrWhiteSpace(edgeId))
                throw new ArgumentException("Edge ID is required.", nameof(edgeId));
            if (string.IsNullOrWhiteSpace(fromNodeId))
                throw new ArgumentException("From node ID is required.", nameof(fromNodeId));
            if (string.IsNullOrWhiteSpace(toNodeId))
                throw new ArgumentException("To node ID is required.", nameof(toNodeId));

            EdgeId = edgeId;
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            Type = type;
            GateId = gateId;
            IsBidirectional = isBidirectional;
            RequirementId = requirementId;
        }

        public string EdgeId { get; }
        public string FromNodeId { get; }
        public string ToNodeId { get; }
        public EdgeType Type { get; }
        public string? GateId { get; }
        public bool IsBidirectional { get; }
        public string? RequirementId { get; }
    }
}
