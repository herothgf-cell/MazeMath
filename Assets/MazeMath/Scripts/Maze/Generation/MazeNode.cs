using System;
using System.Collections.Generic;
using MazeMath.Maze.Data;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeNode
    {
        private readonly List<string> _tags;

        public MazeNode(
            string nodeId,
            string templateId,
            int floor,
            RoomType type,
            bool isCriticalPath,
            IEnumerable<string>? tags = null)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                throw new ArgumentException("Node ID is required.", nameof(nodeId));
            if (string.IsNullOrWhiteSpace(templateId))
                throw new ArgumentException("Template ID is required.", nameof(templateId));
            if (floor < 1)
                throw new ArgumentOutOfRangeException(nameof(floor));

            NodeId = nodeId;
            TemplateId = templateId;
            Floor = floor;
            Type = type;
            IsCriticalPath = isCriticalPath;
            _tags = tags == null ? new List<string>() : new List<string>(tags);
        }

        public string NodeId { get; }
        public string TemplateId { get; }
        public int Floor { get; }
        public RoomType Type { get; }
        public bool IsCriticalPath { get; }
        public IReadOnlyList<string> Tags => _tags;
    }
}
