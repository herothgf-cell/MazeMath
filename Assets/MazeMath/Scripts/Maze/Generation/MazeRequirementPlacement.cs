using System;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeRequirementPlacement
    {
        public MazeRequirementPlacement(string requirementId, string nodeId)
        {
            if (string.IsNullOrWhiteSpace(requirementId))
                throw new ArgumentException("Requirement ID is required.", nameof(requirementId));
            if (string.IsNullOrWhiteSpace(nodeId))
                throw new ArgumentException("Node ID is required.", nameof(nodeId));

            RequirementId = requirementId;
            NodeId = nodeId;
        }

        public string RequirementId { get; }
        public string NodeId { get; }
    }
}
