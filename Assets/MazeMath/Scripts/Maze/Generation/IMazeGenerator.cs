namespace MazeMath.Maze.Generation
{
    public interface IMazeGenerator
    {
        MazeGenerationResult Generate(MazeGenerationRequest request);
    }
}
