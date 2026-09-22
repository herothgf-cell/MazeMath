using System.Collections;
using MazeMath.UI.Question;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MazeMath.Tests.Questions
{
    public sealed class NumericKeypadViewTests
    {
        [UnityTest]
        public IEnumerator PressingDigitsBackspaceAndSubmit_UsesNumericBuffer()
        {
            var root = new GameObject("Keypad", typeof(RectTransform));
            var view = root.AddComponent<NumericKeypadView>();

            string submitted = null;
            view.Configure(3, value => submitted = value);

            view.PressDigit(2);
            view.PressDigit(3);
            Assert.AreEqual("23", view.CurrentText);

            view.Backspace();
            view.PressDigit(4);
            view.Submit();

            Assert.AreEqual("24", submitted);
            Assert.AreEqual("24", view.CurrentText);

            Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RuntimeBuild_CreatesTwelveInteractiveButtons()
        {
            var root = new GameObject("Keypad", typeof(RectTransform));
            var view = root.AddComponent<NumericKeypadView>();

            view.EnsureRuntimeUi();

            Assert.AreEqual(12, view.ButtonCount);

            Object.Destroy(root);
            yield return null;
        }
    }
}
