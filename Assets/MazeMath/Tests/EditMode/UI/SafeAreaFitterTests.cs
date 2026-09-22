using MazeMath.UI;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.UI
{
    public sealed class SafeAreaFitterTests
    {
        [TestCase(1080, 1920, 0, 80, 1080, 1760)]
        [TestCase(1920, 1080, 80, 0, 1760, 1080)]
        [TestCase(1024, 768, 0, 0, 1024, 768)]
        public void Apply_ConvertsPixelSafeAreaToAnchors(
            int screenWidth,
            int screenHeight,
            float x,
            float y,
            float width,
            float height)
        {
            var go = new GameObject("SafeArea", typeof(RectTransform));
            var fitter = go.AddComponent<SafeAreaFitter>();
            var rect = go.GetComponent<RectTransform>();

            fitter.Apply(
                new Rect(x, y, width, height),
                new Vector2Int(screenWidth, screenHeight));

            var expectedMin = new Vector2(x / screenWidth, y / screenHeight);
            var expectedMax = new Vector2(
                (x + width) / screenWidth,
                (y + height) / screenHeight);

            Assert.That(rect.anchorMin.x, Is.EqualTo(expectedMin.x).Within(0.0001f));
            Assert.That(rect.anchorMin.y, Is.EqualTo(expectedMin.y).Within(0.0001f));
            Assert.That(rect.anchorMax.x, Is.EqualTo(expectedMax.x).Within(0.0001f));
            Assert.That(rect.anchorMax.y, Is.EqualTo(expectedMax.y).Within(0.0001f));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Apply_WithInvalidScreenSize_UsesFullRect()
        {
            var go = new GameObject("SafeArea", typeof(RectTransform));
            var fitter = go.AddComponent<SafeAreaFitter>();
            var rect = go.GetComponent<RectTransform>();

            fitter.Apply(new Rect(0, 0, 100, 100), Vector2Int.zero);

            Assert.AreEqual(Vector2.zero, rect.anchorMin);
            Assert.AreEqual(Vector2.one, rect.anchorMax);

            Object.DestroyImmediate(go);
        }
    }
}
