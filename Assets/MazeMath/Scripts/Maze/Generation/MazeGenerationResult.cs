using System;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeGenerationResult
    {
        public MazeGenerationResult(
            MazeGraph graph,
            uint seed,
            bool usedSafeLayout = false,
            int attemptCount = 1)
        {
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            Seed = seed;
            UsedSafeLayout = usedSafeLayout;
            AttemptCount = attemptCount;
        }

        public MazeGraph Graph { get; }
        public uint Seed { get; }
        public bool UsedSafeLayout { get; }
        public int AttemptCount { get; }
    }
}
