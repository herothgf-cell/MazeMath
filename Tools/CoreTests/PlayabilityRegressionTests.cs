using System.Collections.Generic;
using MazeMath.Puzzles.Types;
using NUnit.Framework;

namespace MazeMath.CoreTests
{
    public sealed class PlayabilityRegressionTests
    {
        [Test]
        public void LaserCanStartImmediatelyAfterConstruction()
        {
            var puzzle = new LaserMirrorPuzzle("laser", 5, 5, new GridPosition(0, 1), GridDirection.Right,
                new GridPosition(2, 3), new HashSet<GridPosition>(),
                new Dictionary<GridPosition, MirrorOrientation> { { new GridPosition(2, 1), MirrorOrientation.Backslash } });
            Assert.DoesNotThrow(() => puzzle.StartPuzzle());
            Assert.IsFalse(puzzle.IsSolved());
            Assert.IsTrue(puzzle.RotateMirror(new GridPosition(2, 1)));
            Assert.IsTrue(puzzle.IsSolved());
        }

        [Test]
        public void LaserOwnsACopyOfCallerMirrors()
        {
            var mirrors = new Dictionary<GridPosition, MirrorOrientation> { { new GridPosition(2, 1), MirrorOrientation.Slash } };
            var puzzle = new LaserMirrorPuzzle("laser", 5, 5, new GridPosition(0, 1), GridDirection.Right,
                new GridPosition(2, 3), null, mirrors);
            mirrors.Clear();
            Assert.DoesNotThrow(() => puzzle.StartPuzzle());
            Assert.IsTrue(puzzle.IsSolved());
        }
    }
}
