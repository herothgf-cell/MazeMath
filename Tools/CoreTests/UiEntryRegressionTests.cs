using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using NUnit.Framework;
using MazeMath.Adventure;

namespace MazeMath.CoreTests
{
    public sealed class UiEntryRegressionTests
    {
        [TestCase("Bootstrap", false, false, true)]
        [TestCase("Gameplay", true, false, true)]
        [TestCase("CustomLegacyScene", true, false, true)]
        [TestCase("Gameplay", true, true, false)]
        [TestCase("Adventure", false, true, false)]
        [TestCase("OtherGame", false, false, false)]
        public void OnlyOldMazeMathEntryPointsRedirect(string scene, bool legacy, bool current, bool expected)
        {
            var type = typeof(AdventureState).Assembly.GetType("MazeMath.Adventure.AdventureStartupPolicy");
            Assert.That(type, Is.Not.Null, "A shared startup policy must cover old entry scenes, not just the new menu.");
            var method = type.GetMethod("ShouldRedirect", BindingFlags.Public | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            Assert.That(method.Invoke(null, new object[] { scene, legacy, current }), Is.EqualTo(expected));
        }

        [Test]
        public void BootstrapUsesTheCurrentExperienceInsteadOfAnUnstableBuildIndex()
        {
            var source = Read("Assets/MazeMath/Scripts/Core/GameBootstrap.cs");
            var start = Method(source, "Start");
            StringAssert.Contains("AdventureEntry.EnsureCurrent", start);
            StringAssert.DoesNotContain("LoadSceneAsync(1", start);
        }

        [Test]
        public void StandaloneQuestionPanelAlsoUsesTheSharedSuccessTimer()
        {
            var source = Read("Assets/MazeMath/Scripts/UI/Question/QuestionPanelView.cs");
            StringAssert.Contains("QuestionFeedbackFlow", source);
            StringAssert.Contains("session.Completed +=", source);
            StringAssert.Contains("ShouldClose", source);
            StringAssert.Contains("gameObject.SetActive(false)", source);
        }

        [Test]
        public void AnswerDismissalRunsEvenWhenWorldPresentationCannotBeUpdated()
        {
            var source = Read("Assets/MazeMath/Scripts/Adventure/Runtime/AdventureHud.cs");
            var update = Method(source, "Update");
            StringAssert.Contains("ShouldClose", update);
            StringAssert.DoesNotContain("presentation.Update", update);
        }

        [Test]
        public void LegacyQuestionViewsDoNotCreateOversizedFixedCanvases()
        {
            var source = Read("Assets/MazeMath/Scripts/UI/Question/QuestionPanelView.cs");
            StringAssert.DoesNotContain("new Vector2(720f, 820f)", source);
            StringAssert.Contains("AdventureTheme", source);
            var keypad = Read("Assets/MazeMath/Scripts/UI/Question/NumericKeypadView.cs");
            StringAssert.DoesNotContain("new Vector2(118f, 88f)", keypad);
            StringAssert.Contains("Reflow", keypad);
        }

        private static string Read(string path)
        {
            return File.ReadAllText(Path.Combine(SourceSyntaxTests.RepositoryRoot(), path));
        }
        private static string Method(string source, string name)
        {
            var syntax = CSharpSyntaxTree.ParseText(source).GetRoot();
            var method = syntax.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.ValueText == name);
            Assert.That(method, Is.Not.Null, "Missing runtime method " + name);
            return method.ToString();
        }
    }
}
