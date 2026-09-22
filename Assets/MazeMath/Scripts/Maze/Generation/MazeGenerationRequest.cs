using System;
using MazeMath.Maze.Data;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeGenerationRequest
    {
        public MazeGenerationRequest(
            string chapterId,
            uint seed,
            MazeGenerationSettings settings)
        {
            if (string.IsNullOrWhiteSpace(chapterId))
                throw new ArgumentException("Chapter ID is required.", nameof(chapterId));

            ChapterId = chapterId;
            Seed = seed;
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string ChapterId { get; }
        public uint Seed { get; }
        public MazeGenerationSettings Settings { get; }
    }
}
