using System.Collections.Generic;

namespace MazeMath.Maze.Generation
{
    public enum MazeValidationError
    {
        NoPathToBoss,
        MissingRequiredRoom,
        GateDependencyCycle,
        RequirementBehindOwnGate,
        ExcessiveBacktracking,
        InvalidFloorTransition,
        UnreachableOptionalRoom,
        ConsecutiveSamePuzzleType,
        MissingCheckpointBeforeBoss
    }

    public sealed class MazeValidationResult
    {
        public List<MazeValidationError> Errors { get; } = new List<MazeValidationError>();
        public bool HasCriticalErrors => Errors.Count > 0;

        public void Add(MazeValidationError error)
        {
            if (!Errors.Contains(error))
            {
                Errors.Add(error);
            }
        }
    }
}
