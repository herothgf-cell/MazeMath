using System;
using System.Collections.Generic;
using System.Linq;
using MazeMath.Puzzles;

namespace MazeMath.Puzzles.Types
{
    public sealed class SokobanPuzzle : IEnvironmentPuzzle
    {
        private readonly int width;
        private readonly int height;
        private readonly HashSet<GridPosition> walls;
        private readonly HashSet<GridPosition> goals;
        private readonly HashSet<GridPosition> initialBoxes;
        private readonly GridPosition initialPlayer;
        private HashSet<GridPosition> boxes;

        public string PuzzleId { get; }
        public PuzzleState State { get; private set; } = PuzzleState.Ready;
        public GridPosition Player { get; private set; }
        public IReadOnlyCollection<GridPosition> Boxes => boxes;

        public SokobanPuzzle(
            string puzzleId,
            int width,
            int height,
            HashSet<GridPosition> walls,
            HashSet<GridPosition> boxes,
            HashSet<GridPosition> goals,
            GridPosition player)
        {
            if (string.IsNullOrWhiteSpace(puzzleId))
                throw new ArgumentException("Puzzle id is required.", nameof(puzzleId));
            if (width < 2 || height < 2)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (boxes == null || goals == null || boxes.Count == 0 || boxes.Count != goals.Count)
                throw new ArgumentException("Boxes and goals must have the same non-zero count.");

            PuzzleId = puzzleId;
            this.width = width;
            this.height = height;
            this.walls = walls != null ? new HashSet<GridPosition>(walls) : new HashSet<GridPosition>();
            initialBoxes = new HashSet<GridPosition>(boxes);
            this.goals = new HashSet<GridPosition>(goals);
            initialPlayer = player;
            ResetInternal();
        }

        public void StartPuzzle()
        {
            if (State != PuzzleState.Solved)
                State = PuzzleState.Active;
        }

        public bool Move(GridDirection direction)
        {
            if (State != PuzzleState.Active)
                return false;

            var next = Player.Step(direction);
            if (!IsInside(next) || walls.Contains(next))
                return false;

            if (boxes.Contains(next))
            {
                var boxNext = next.Step(direction);
                if (!IsInside(boxNext) || walls.Contains(boxNext) || boxes.Contains(boxNext))
                    return false;

                boxes.Remove(next);
                boxes.Add(boxNext);
            }

            Player = next;
            if (goals.All(goal => boxes.Contains(goal)))
                State = PuzzleState.Solved;
            return true;
        }

        public void ResetPuzzle()
        {
            ResetInternal();
            State = PuzzleState.Ready;
        }

        public bool IsSolved() => State == PuzzleState.Solved;

        private bool IsInside(GridPosition position)
        {
            return position.X >= 0 &&
                   position.X < width &&
                   position.Y >= 0 &&
                   position.Y < height;
        }

        private void ResetInternal()
        {
            Player = initialPlayer;
            boxes = new HashSet<GridPosition>(initialBoxes);
        }
    }
}
