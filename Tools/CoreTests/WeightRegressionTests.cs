using MazeMath.Puzzles.Types;
using NUnit.Framework;

namespace MazeMath.CoreTests
{
    public sealed class WeightRegressionTests
    {
        [Test] public void RemovingExtraCrateCanCompleteTheScale()
        {
            var p = new WeightBridgePuzzle("scale", 8);
            p.StartPuzzle();
            p.PlaceWeight("extra", 2); p.PlaceWeight("a", 3); p.PlaceWeight("b", 5);
            Assert.IsFalse(p.IsSolved()); Assert.AreEqual(10, p.CurrentWeight);
            Assert.IsTrue(p.RemoveWeight("extra"));
            Assert.AreEqual(8, p.CurrentWeight); Assert.IsTrue(p.IsSolved());
        }
        [Test] public void RemovingRequiredCrateReopensTheScale()
        {
            var p = new WeightBridgePuzzle("scale", 8); p.StartPuzzle();
            p.PlaceWeight("a", 3); p.PlaceWeight("b", 5); Assert.IsTrue(p.IsSolved());
            p.RemoveWeight("b"); Assert.IsFalse(p.IsSolved());
            p.PlaceWeight("b", 5); Assert.IsTrue(p.IsSolved());
        }
    }
}
