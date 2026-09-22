using System.Collections.Generic;
using MazeMath.Puzzles;
using MazeMath.Puzzles.Types;
using NUnit.Framework;

namespace MazeMath.Tests.Puzzles
{
    public sealed class AdvancedEnvironmentPuzzleTests
    {
        [Test]
        public void WeightBridge_SolvesAtExactTarget()
        {
            var puzzle = new WeightBridgePuzzle("bridge", 8);
            puzzle.StartPuzzle();

            puzzle.PlaceWeight("box3", 3);
            Assert.IsFalse(puzzle.IsSolved());

            puzzle.PlaceWeight("box5", 5);
            Assert.IsTrue(puzzle.IsSolved());
            Assert.AreEqual(8, puzzle.CurrentWeight);
        }

        [Test]
        public void WeightBridge_ResetClearsPlacedWeights()
        {
            var puzzle = new WeightBridgePuzzle("bridge", 8);
            puzzle.StartPuzzle();
            puzzle.PlaceWeight("box3", 3);
            puzzle.ResetPuzzle();

            Assert.AreEqual(0, puzzle.CurrentWeight);
            Assert.AreEqual(PuzzleState.Ready, puzzle.State);
        }

        [Test]
        public void Sokoban_PushBoxOntoGoal_Solves()
        {
            var puzzle = new SokobanPuzzle(
                "soko",
                4,
                3,
                new HashSet<GridPosition>(),
                new HashSet<GridPosition> { new GridPosition(2, 1) },
                new HashSet<GridPosition> { new GridPosition(3, 1) },
                new GridPosition(1, 1));

            puzzle.StartPuzzle();
            Assert.IsTrue(puzzle.Move(GridDirection.Right));

            Assert.IsTrue(puzzle.IsSolved());
            Assert.AreEqual(new GridPosition(2, 1), puzzle.Player);
        }

        [Test]
        public void Sokoban_CannotPushBoxIntoWall()
        {
            var puzzle = new SokobanPuzzle(
                "soko",
                4,
                3,
                new HashSet<GridPosition> { new GridPosition(3, 1) },
                new HashSet<GridPosition> { new GridPosition(2, 1) },
                new HashSet<GridPosition> { new GridPosition(1, 2) },
                new GridPosition(1, 1));

            puzzle.StartPuzzle();

            Assert.IsFalse(puzzle.Move(GridDirection.Right));
            Assert.AreEqual(new GridPosition(1, 1), puzzle.Player);
        }

        [Test]
        public void LaserMirror_RotationCanReachTarget()
        {
            var mirrors = new Dictionary<GridPosition, MirrorOrientation>
            {
                { new GridPosition(2, 1), MirrorOrientation.Slash }
            };

            var puzzle = new LaserMirrorPuzzle(
                "laser",
                5,
                5,
                new GridPosition(0, 1),
                GridDirection.Right,
                new GridPosition(2, 3),
                new HashSet<GridPosition>(),
                mirrors);

            puzzle.StartPuzzle();
            Assert.IsTrue(puzzle.IsSolved());
        }

        [Test]
        public void LaserMirror_WallBlocksBeam()
        {
            var puzzle = new LaserMirrorPuzzle(
                "laser",
                5,
                3,
                new GridPosition(0, 1),
                GridDirection.Right,
                new GridPosition(4, 1),
                new HashSet<GridPosition> { new GridPosition(2, 1) },
                new Dictionary<GridPosition, MirrorOrientation>());

            puzzle.StartPuzzle();
            Assert.IsFalse(puzzle.IsSolved());
        }
    }
}
