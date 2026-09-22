using System;
using System.Collections.Generic;
using MazeMath.Maze.Generation;

namespace MazeMath.Puzzles.Types
{
    public enum PathStep
    {
        Up,
        Down,
        Left,
        Right
    }

    public sealed class MemoryPathPuzzle : IEnvironmentPuzzle
    {
        private readonly PathStep[] path;

        public string PuzzleId { get; }
        public PuzzleState State { get; private set; } = PuzzleState.Ready;
        public int ProgressIndex { get; private set; }
        public IReadOnlyList<PathStep> Path => path;

        private MemoryPathPuzzle(string puzzleId, PathStep[] path)
        {
            PuzzleId = puzzleId;
            this.path = path;
        }

        public static MemoryPathPuzzle Create(
            string puzzleId,
            PuzzleDifficulty difficulty,
            int seed)
        {
            if (string.IsNullOrWhiteSpace(puzzleId))
            {
                throw new ArgumentException("Puzzle id must not be empty.", nameof(puzzleId));
            }

            var random = new DeterministicRandom(seed);
            var length = difficulty switch
            {
                PuzzleDifficulty.Easy => 3,
                PuzzleDifficulty.Normal => 4,
                PuzzleDifficulty.Think => random.NextInt(5, 7),
                PuzzleDifficulty.Challenge => 6,
                _ => 3
            };

            var generated = new PathStep[length];
            for (var i = 0; i < generated.Length; i++)
            {
                generated[i] = (PathStep)random.NextInt(0, 4);
            }

            return new MemoryPathPuzzle(puzzleId, generated);
        }

        public void StartPuzzle()
        {
            if (State != PuzzleState.Solved)
            {
                State = PuzzleState.Active;
            }
        }

        public bool Step(PathStep step)
        {
            if (State != PuzzleState.Active)
            {
                return false;
            }

            if (step != path[ProgressIndex])
            {
                ProgressIndex = 0;
                return false;
            }

            ProgressIndex++;
            if (ProgressIndex >= path.Length)
            {
                State = PuzzleState.Solved;
            }

            return true;
        }

        public void ResetPuzzle()
        {
            ProgressIndex = 0;
            State = PuzzleState.Ready;
        }

        public bool IsSolved()
        {
            return State == PuzzleState.Solved;
        }
    }
}
