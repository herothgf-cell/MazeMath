using MazeMath.Core.Determinism;

namespace MazeMath.Maze.Generation
{
    public sealed class MazeSeedService
    {
        public uint CreateSeed(
            string? profileId,
            string? chapterId,
            int attemptIndex,
            int baseSeedOffset)
        {
            var key = $"{profileId ?? string.Empty}|{chapterId ?? string.Empty}|{attemptIndex}|{baseSeedOffset}";
            return StableHash.Fnv1A32(key);
        }
    }
}
