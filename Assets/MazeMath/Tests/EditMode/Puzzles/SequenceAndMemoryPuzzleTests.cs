using MazeMath.Puzzles;
using MazeMath.Puzzles.Types;
using NUnit.Framework;

namespace MazeMath.Tests.Puzzles
{
    public sealed class SequenceAndMemoryPuzzleTests
    {
        [Test]
        public void SequencePlate_WrongStepResetsProgress()
        {
            var puzzle = new SequencePlatePuzzle("plates", new[] { 2, 4, 6 });
            puzzle.StartPuzzle();

            Assert.IsTrue(puzzle.Step(2));
            Assert.AreEqual(1, puzzle.ProgressIndex);

            Assert.IsFalse(puzzle.Step(6));
            Assert.AreEqual(0, puzzle.ProgressIndex);
            Assert.IsFalse(puzzle.IsSolved());
        }

        [Test]
        public void SequencePlate_CorrectSequenceSolves()
        {
            var puzzle = new SequencePlatePuzzle("plates", new[] { 2, 4, 6 });
            puzzle.StartPuzzle();

            puzzle.Step(2);
            puzzle.Step(4);
            puzzle.Step(6);

            Assert.IsTrue(puzzle.IsSolved());
            Assert.AreEqual(PuzzleState.Solved, puzzle.State);
        }

        [TestCase(PuzzleDifficulty.Easy, 3)]
        [TestCase(PuzzleDifficulty.Normal, 4)]
        public void MemoryPath_GeneratesExpectedLength(
            PuzzleDifficulty difficulty,
            int expectedLength)
        {
            var puzzle = MemoryPathPuzzle.Create("memory", difficulty, 123);

            Assert.AreEqual(expectedLength, puzzle.Path.Count);
        }

        [Test]
        public void MemoryPath_ThinkLengthIsFiveOrSix()
        {
            for (var seed = 0; seed < 20; seed++)
            {
                var puzzle = MemoryPathPuzzle.Create("memory", PuzzleDifficulty.Think, seed);
                Assert.That(puzzle.Path.Count, Is.InRange(5, 6));
            }
        }
    }
}
