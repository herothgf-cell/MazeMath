using System;

namespace MazeMath.Maze.Data
{
    public sealed class MazeGenerationSettings
    {
        public MazeGenerationSettings(
            int minCriticalPathRooms,
            int maxCriticalPathRooms,
            int optionalRoomCount,
            int floorCount,
            int maxRequiredBacktracks,
            int maxOptionalBranchDepth = 1,
            bool requireCheckpointBeforeBoss = true)
        {
            if (minCriticalPathRooms < 2)
                throw new ArgumentOutOfRangeException(nameof(minCriticalPathRooms));
            if (maxCriticalPathRooms < minCriticalPathRooms)
                throw new ArgumentOutOfRangeException(nameof(maxCriticalPathRooms));
            if (optionalRoomCount < 0)
                throw new ArgumentOutOfRangeException(nameof(optionalRoomCount));
            if (floorCount < 1 || floorCount > 3)
                throw new ArgumentOutOfRangeException(nameof(floorCount));
            if (maxRequiredBacktracks < 0)
                throw new ArgumentOutOfRangeException(nameof(maxRequiredBacktracks));
            if (maxOptionalBranchDepth < 1 || maxOptionalBranchDepth > 2)
                throw new ArgumentOutOfRangeException(nameof(maxOptionalBranchDepth));

            MinCriticalPathRooms = minCriticalPathRooms;
            MaxCriticalPathRooms = maxCriticalPathRooms;
            OptionalRoomCount = optionalRoomCount;
            FloorCount = floorCount;
            MaxRequiredBacktracks = maxRequiredBacktracks;
            MaxOptionalBranchDepth = maxOptionalBranchDepth;
            RequireCheckpointBeforeBoss = requireCheckpointBeforeBoss;
        }

        public int MinCriticalPathRooms { get; }
        public int MaxCriticalPathRooms { get; }
        public int OptionalRoomCount { get; }
        public int FloorCount { get; }
        public int MaxRequiredBacktracks { get; }
        public int MaxOptionalBranchDepth { get; }
        public bool RequireCheckpointBeforeBoss { get; }

        public static MazeGenerationSettings Chapter1Defaults()
        {
            return new MazeGenerationSettings(6, 8, 0, 1, 1);
        }

        public MazeGenerationSettings WithOptionalRoomCount(int count)
        {
            return new MazeGenerationSettings(
                MinCriticalPathRooms,
                MaxCriticalPathRooms,
                count,
                FloorCount,
                MaxRequiredBacktracks,
                MaxOptionalBranchDepth,
                RequireCheckpointBeforeBoss);
        }
    }
}
