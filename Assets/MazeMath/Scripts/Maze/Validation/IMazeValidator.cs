using MazeMath.Maze.Data;
using MazeMath.Maze.Generation;

namespace MazeMath.Maze.Validation
{
    public interface IMazeValidator
    {
        MazeValidationResult Validate(
            MazeGraph graph,
            MazeGenerationSettings settings);
    }
}
