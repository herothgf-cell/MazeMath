using System.Collections;
using MazeMath.UI.Puzzles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MazeMath.Tests.Puzzles
{
    public sealed class EnvironmentPuzzleDemoViewTests
    {
        [UnityTest]
        public IEnumerator WeightMode_ThreePlusFiveSolves()
        {
            var root = new GameObject("PuzzleView", typeof(RectTransform));
            var view = root.AddComponent<EnvironmentPuzzleDemoView>();

            string solvedId = null;
            view.Configure(id => solvedId = id);
            view.SelectWeightPuzzle();

            view.AddWeight3();
            Assert.IsFalse(view.IsActiveSolved);

            view.AddWeight5();
            Assert.IsTrue(view.IsActiveSolved);
            Assert.AreEqual("demo.weight", solvedId);

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SokobanMode_CanSolveFixedBoard()
        {
            var root = new GameObject("PuzzleView", typeof(RectTransform));
            var view = root.AddComponent<EnvironmentPuzzleDemoView>();

            string solvedId = null;
            view.Configure(id => solvedId = id);
            view.SelectSokobanPuzzle();

            view.MoveRight();

            Assert.IsTrue(view.IsActiveSolved);
            Assert.AreEqual("demo.sokoban", solvedId);

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LaserMode_RotationSolvesFixedBoard()
        {
            var root = new GameObject("PuzzleView", typeof(RectTransform));
            var view = root.AddComponent<EnvironmentPuzzleDemoView>();

            string solvedId = null;
            view.Configure(id => solvedId = id);
            view.SelectLaserPuzzle();

            Assert.IsFalse(view.IsActiveSolved);
            view.RotateLaserMirror();

            Assert.IsTrue(view.IsActiveSolved);
            Assert.AreEqual("demo.laser", solvedId);

            Object.Destroy(root);
            yield return null;
        }
    }
}
