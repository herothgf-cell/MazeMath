using System;
using System.Collections.Generic;
using MazeMath.Puzzles;

namespace MazeMath.Puzzles.Types
{
    public sealed class LaserMirrorPuzzle : IEnvironmentPuzzle
    {
        private readonly int width;
        private readonly int height;
        private readonly GridPosition source;
        private readonly GridDirection sourceDirection;
        private readonly GridPosition target;
        private readonly HashSet<GridPosition> walls;
        private readonly Dictionary<GridPosition, MirrorOrientation> initialMirrors;
        private Dictionary<GridPosition, MirrorOrientation> mirrors;

        public string PuzzleId { get; }
        public PuzzleState State { get; private set; } = PuzzleState.Ready;

        public LaserMirrorPuzzle(
            string puzzleId,
            int width,
            int height,
            GridPosition source,
            GridDirection sourceDirection,
            GridPosition target,
            HashSet<GridPosition> walls,
            Dictionary<GridPosition, MirrorOrientation> mirrors)
        {
            if (string.IsNullOrWhiteSpace(puzzleId))
                throw new ArgumentException("Puzzle id is required.", nameof(puzzleId));
            if (width < 2 || height < 2)
                throw new ArgumentOutOfRangeException(nameof(width));

            PuzzleId = puzzleId;
            this.width = width;
            this.height = height;
            this.source = source;
            this.sourceDirection = sourceDirection;
            this.target = target;
            this.walls = walls != null ? new HashSet<GridPosition>(walls) : new HashSet<GridPosition>();
            initialMirrors = mirrors != null
                ? new Dictionary<GridPosition, MirrorOrientation>(mirrors)
                : new Dictionary<GridPosition, MirrorOrientation>();
            this.mirrors = new Dictionary<GridPosition, MirrorOrientation>(initialMirrors);
        }

        public void StartPuzzle()
        {
            State = TraceHitsTarget() ? PuzzleState.Solved : PuzzleState.Active;
        }

        public bool RotateMirror(GridPosition position)
        {
            if (!mirrors.TryGetValue(position, out var current))
                return false;

            mirrors[position] = current == MirrorOrientation.Slash
                ? MirrorOrientation.Backslash
                : MirrorOrientation.Slash;

            State = TraceHitsTarget() ? PuzzleState.Solved : PuzzleState.Active;
            return true;
        }

        public void ResetPuzzle()
        {
            mirrors = new Dictionary<GridPosition, MirrorOrientation>(initialMirrors);
            State = PuzzleState.Ready;
        }

        public bool IsSolved()
        {
            if (State == PuzzleState.Ready)
                return false;

            return TraceHitsTarget();
        }

        public bool TraceHitsTarget()
        {
            var position = source;
            var direction = sourceDirection;
            var visited = new HashSet<string>();
            var maxSteps = width * height * 4;

            for (var step = 0; step < maxSteps; step++)
            {
                position = position.Step(direction);
                if (!IsInside(position) || walls.Contains(position))
                    return false;

                if (position == target)
                    return true;

                var stateKey = position.X + ":" + position.Y + ":" + (int)direction;
                if (!visited.Add(stateKey))
                    return false;

                if (mirrors.TryGetValue(position, out var mirror))
                    direction = Reflect(direction, mirror);
            }

            return false;
        }

        private bool IsInside(GridPosition position)
        {
            return position.X >= 0 &&
                   position.X < width &&
                   position.Y >= 0 &&
                   position.Y < height;
        }

        private static GridDirection Reflect(
            GridDirection direction,
            MirrorOrientation mirror)
        {
            if (mirror == MirrorOrientation.Slash)
            {
                switch (direction)
                {
                    case GridDirection.Right: return GridDirection.Up;
                    case GridDirection.Up: return GridDirection.Right;
                    case GridDirection.Left: return GridDirection.Down;
                    case GridDirection.Down: return GridDirection.Left;
                }
            }

            switch (direction)
            {
                case GridDirection.Right: return GridDirection.Down;
                case GridDirection.Down: return GridDirection.Right;
                case GridDirection.Left: return GridDirection.Up;
                case GridDirection.Up: return GridDirection.Left;
                default: return direction;
            }
        }
    }
}
