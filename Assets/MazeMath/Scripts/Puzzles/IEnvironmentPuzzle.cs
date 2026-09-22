namespace MazeMath.Puzzles
{
    public enum PuzzleState
    {
        Locked,
        Ready,
        Active,
        Solved
    }

    public enum PuzzleDifficulty
    {
        Easy = 1,
        Normal = 2,
        Think = 3,
        Challenge = 4
    }

    public interface IEnvironmentPuzzle
    {
        string PuzzleId { get; }
        PuzzleState State { get; }

        void StartPuzzle();
        void ResetPuzzle();
        bool IsSolved();
    }
}
