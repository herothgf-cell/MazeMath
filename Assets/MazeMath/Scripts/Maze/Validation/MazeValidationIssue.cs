using System;

namespace MazeMath.Maze.Validation
{
    public sealed class MazeValidationIssue
    {
        public MazeValidationIssue(MazeValidationErrorCode code, string message)
        {
            Code = code;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public MazeValidationErrorCode Code { get; }
        public string Message { get; }
    }
}
