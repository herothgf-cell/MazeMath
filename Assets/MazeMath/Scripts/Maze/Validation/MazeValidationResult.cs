using System.Collections.Generic;
using System.Linq;

namespace MazeMath.Maze.Validation
{
    public sealed class MazeValidationResult
    {
        private readonly List<MazeValidationIssue> _issues;

        public MazeValidationResult(IEnumerable<MazeValidationIssue> issues)
        {
            _issues = new List<MazeValidationIssue>(issues);
        }

        public IReadOnlyList<MazeValidationIssue> Issues => _issues;
        public IReadOnlyCollection<MazeValidationErrorCode> ErrorCodes =>
            _issues.Select(issue => issue.Code).Distinct().ToArray();
        public bool IsValid => _issues.Count == 0;
    }
}
