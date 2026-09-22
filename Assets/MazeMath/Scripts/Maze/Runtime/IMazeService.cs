using MazeMath.Maze;

namespace MazeMath.Maze.Runtime
{
    public interface IMazeService
    {
        MazeGraph CurrentGraph { get; }
        MazeRunState CurrentState { get; }

        MazeGraph StartChapter(string chapterId, int runSeed);
        void EnterNode(string nodeId);
        void SolveGate(string gateId);
        bool CanTraverse(string edgeId);
    }
}
