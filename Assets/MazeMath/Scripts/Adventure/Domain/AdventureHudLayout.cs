using System;

namespace MazeMath.Adventure
{
    /// <summary>HUD dimensions in canvas units. Camera uses only the interval between the two rails.</summary>
    public sealed class AdventureHudLayout
    {
        public double Width { get; }
        public double Height { get; }
        public bool Compact { get; }
        public double HeaderHeight { get; }
        public double DockHeight { get; }
        public double WorldBottom => DockHeight / Height;
        public double WorldTop => 1.0 - HeaderHeight / Height;
        public double SlotSize { get; }
        public AdventureHudLayout(double width, double height, bool touch)
        {
            if (width <= 0 || height <= 0 || double.IsNaN(width) || double.IsNaN(height) || double.IsInfinity(width) || double.IsInfinity(height))
                throw new ArgumentOutOfRangeException(nameof(width));
            Width = width; Height = height; Compact = width < 700;
            HeaderHeight = Math.Min(Compact ? 76 : 66, height * 0.14);
            DockHeight = Math.Min(touch ? (Compact ? 148 : 98) : 78, height * 0.22);
            SlotSize = Math.Min(42, Math.Max(24, (width - 52) / 6));
        }
    }
}
