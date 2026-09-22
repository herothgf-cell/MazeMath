using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MazeMath.Maze;

namespace MazeMath.Maze.Data
{
    [CreateAssetMenu(menuName = "MazeMath/Maze/Chapter")]
    public sealed class MazeChapterDefinition : ScriptableObject
    {
        public string chapterId = "chapter-01";
        public string displayName = "Chapter 1";
        public int floorCount = 1;
        public Vector2Int requiredRoomCountRange = new Vector2Int(6, 8);
        public Vector2Int optionalRoomCountRange = new Vector2Int(2, 3);
        public int maxBranchDepth = 1;
        public int requiredBacktrackCount = 1;
        public int maxConsecutiveDeadEnds = 1;
        public MazeDifficultyProfile difficulty;
        public List<RoomTemplateDefinition> allowedRooms = new List<RoomTemplateDefinition>();
        public List<string> requiredRoomTags = new List<string>();
        public int baseSeedOffset;

        public IReadOnlyList<string> ValidateAuthoring()
        {
            var errors = new List<string>();

            if (floorCount < 1)
            {
                errors.Add("FloorCount must be at least 1.");
            }

            if (requiredRoomCountRange.x < 1 ||
                requiredRoomCountRange.x > requiredRoomCountRange.y)
            {
                errors.Add("Required room range is invalid.");
            }

            if (optionalRoomCountRange.x < 0 ||
                optionalRoomCountRange.x > optionalRoomCountRange.y)
            {
                errors.Add("Optional room range is invalid.");
            }

            if (maxBranchDepth < 0)
            {
                errors.Add("MaxBranchDepth must not be negative.");
            }

            if (difficulty == null)
            {
                errors.Add("Difficulty profile is required.");
            }

            if (!allowedRooms.Any(room => room != null && room.type == RoomType.Start))
            {
                errors.Add("A Start room template is required.");
            }

            if (!allowedRooms.Any(room => room != null && room.type == RoomType.Boss))
            {
                errors.Add("A Boss room template is required.");
            }

            var availableCriticalSlots = System.Math.Max(0, requiredRoomCountRange.x - 2);
            if (requiredRoomTags != null &&
                requiredRoomTags.Count > availableCriticalSlots)
            {
                errors.Add("Required room tags exceed the minimum critical-path capacity.");
            }

            if (requiredRoomTags != null)
            {
                foreach (var tag in requiredRoomTags)
                {
                    if (string.IsNullOrWhiteSpace(tag))
                    {
                        errors.Add("Required room tag must not be empty.");
                        continue;
                    }

                    if (!allowedRooms.Any(room =>
                        room != null &&
                        room.canBeCriticalPath &&
                        room.tags != null &&
                        room.tags.Contains(tag)))
                    {
                        errors.Add("Missing critical room template for required tag: " + tag);
                    }
                }
            }

            return errors;
        }
    }
}
