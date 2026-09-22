using System;

namespace MazeMath.Maze
{
    public enum RoomType
    {
        Start,
        Corridor,
        Junction,
        Puzzle,
        Question,
        Reward,
        Workshop,
        Checkpoint,
        Transition,
        Boss,
        Secret
    }

    public enum EdgeType
    {
        Open,
        Door,
        Ladder,
        Elevator,
        Locked,
        PuzzleLocked,
        EquipmentLocked,
        Hidden
    }

    [Serializable]
    public sealed class MazeNode
    {
        public string NodeId { get; }
        public int Floor { get; }
        public RoomType Type { get; }
        public bool IsCriticalPath { get; set; }
        public string TemplateId { get; set; }

        public MazeNode(string nodeId, int floor, RoomType type)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                throw new ArgumentException("Node id must not be empty.", nameof(nodeId));
            }

            NodeId = nodeId;
            Floor = floor;
            Type = type;
        }
    }

    [Serializable]
    public sealed class MazeEdge
    {
        public string EdgeId { get; }
        public string FromNodeId { get; }
        public string ToNodeId { get; }
        public EdgeType Type { get; }
        public bool IsBidirectional { get; }
        public string GateId { get; set; }

        public MazeEdge(
            string edgeId,
            string fromNodeId,
            string toNodeId,
            EdgeType type,
            bool isBidirectional)
        {
            if (string.IsNullOrWhiteSpace(edgeId))
            {
                throw new ArgumentException("Edge id must not be empty.", nameof(edgeId));
            }

            EdgeId = edgeId;
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
            Type = type;
            IsBidirectional = isBidirectional;
        }
    }
}
