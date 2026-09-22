namespace MazeMath.Maze.Validation
{
    public enum MazeValidationErrorCode
    {
        MissingStart,
        MissingBoss,
        NoPathToBoss,
        DanglingEdge,
        GateDependencyCycle,
        RequirementBehindOwnGate,
        ExcessiveBacktracking,
        InvalidFloorTransition,
        MissingCheckpointBeforeBoss
    }
}
