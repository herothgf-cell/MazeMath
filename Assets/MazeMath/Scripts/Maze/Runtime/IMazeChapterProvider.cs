using MazeMath.Maze.Data;

namespace MazeMath.Maze.Runtime
{
    public interface IMazeChapterProvider
    {
        MazeChapterDefinition Get(string chapterId);
    }
}
