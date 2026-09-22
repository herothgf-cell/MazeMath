using System;
using MazeMath.Puzzles;

namespace MazeMath.Puzzles.Types
{
    public sealed class SequencePlatePuzzle : IEnvironmentPuzzle
    {
        private readonly int[] sequence;

        public string PuzzleId { get; }
        public PuzzleState State { get; private set; } = PuzzleState.Ready;
        public int ProgressIndex { get; private set; }

        public SequencePlatePuzzle(string puzzleId, int[] sequence)
        {
            if (string.IsNullOrWhiteSpace(puzzleId))
            {
                throw new ArgumentException("Puzzle id must not be empty.", nameof(puzzleId));
            }

            if (sequence == null || sequence.Length == 0)
            {
                throw new ArgumentException("Sequence must contain at least one step.", nameof(sequence));
            }

            PuzzleId = puzzleId;
            this.sequence = (int[])sequence.Clone();
        }

        public void StartPuzzle()
        {
            if (State != PuzzleState.Solved)
            {
                State = PuzzleState.Active;
            }
        }

        public bool Step(int value)
        {
            if (State != PuzzleState.Active)
            {
                return false;
            }

            if (value != sequence[ProgressIndex])
            {
                ProgressIndex = 0;
                return false;
            }

            ProgressIndex++;
            if (ProgressIndex >= sequence.Length)
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
