using System;

namespace MazeMath.Puzzles.Types
{
    [Serializable]
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public int X { get; }
        public int Y { get; }

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(GridPosition other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        public override int GetHashCode() => unchecked((X * 397) ^ Y);
        public static bool operator ==(GridPosition left, GridPosition right) => left.Equals(right);
        public static bool operator !=(GridPosition left, GridPosition right) => !left.Equals(right);

        public GridPosition Step(GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.Up: return new GridPosition(X, Y + 1);
                case GridDirection.Down: return new GridPosition(X, Y - 1);
                case GridDirection.Left: return new GridPosition(X - 1, Y);
                case GridDirection.Right: return new GridPosition(X + 1, Y);
                default: return this;
            }
        }

        public override string ToString() => $"({X},{Y})";
    }

    public enum GridDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum MirrorOrientation
    {
        Slash,
        Backslash
    }
}
