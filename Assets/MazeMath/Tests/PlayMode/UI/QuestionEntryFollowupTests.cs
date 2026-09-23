using System.Collections;
using System.Collections.Generic;
using MazeMath.Adventure;
using MazeMath.Core.Save;
using MazeMath.Demo;
using MazeMath.Questions.Data;
using MazeMath.Questions.Runtime;
using MazeMath.UI.Question;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MazeMath.Tests.UI
{
    public sealed class QuestionEntryFollowupTests
    {
        private GameObject panelObject;
        private Scene temporaryScene;
        private float previousTimeScale;

        [SetUp] public void SetUp() { previousTimeScale = Time.timeScale; }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale = previousTimeScale;
            if (panelObject != null) Object.Destroy(panelObject);
            if (temporaryScene.IsValid() && temporaryScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(temporaryScene);
            yield return null;
        }

        [UnityTest] public IEnumerator NumericCorrectClosesEvenWhenTimeScaleIsZero()
        {
            var view = Panel();
            var session = new QuestionSession();
            int closed = 0;
            view.Closed += () => closed++;
            view.ShowQuestion(Question("numeric"), session);
            Time.timeScale = 0;
            Assert.AreEqual(QuestionSubmitResult.Correct, view.SubmitNumeric("23"));
            Assert.IsTrue(view.FeedbackPending);
            Assert.AreEqual(QuestionSubmitResult.AlreadyCompleted, view.SubmitNumeric("23"));
            yield return new WaitForSecondsRealtime(.8f);
            Assert.IsFalse(view.gameObject.activeSelf);
            Assert.AreEqual(1, closed);
        }

        [UnityTest] public IEnumerator MultipleChoiceWrongStaysOpenThenCorrectCloses()
        {
            var view = Panel();
            var q = Question("choice");
            q.Type = QuestionType.MultipleChoice;
            q.Choices = new[] { 21, 23, 25 };
            q.CorrectChoiceIndex = 1;
            view.ShowQuestion(q, new QuestionSession());
            Assert.AreEqual(QuestionSubmitResult.Incorrect, view.SubmitChoice(0));
            yield return new WaitForSecondsRealtime(.75f);
            Assert.IsTrue(view.gameObject.activeSelf);
            Assert.AreEqual(QuestionSubmitResult.Correct, view.SubmitChoice(1));
            yield return new WaitForSecondsRealtime(.8f);
            Assert.IsFalse(view.gameObject.activeSelf);
        }

        [UnityTest] public IEnumerator OldQuestionTimerDoesNotCloseANewQuestion()
        {
            var view = Panel();
            view.ShowQuestion(Question("first"), new QuestionSession());
            view.SubmitNumeric("23");
            view.ShowQuestion(Question("second"), new QuestionSession());
            yield return new WaitForSecondsRealtime(.8f);
            Assert.IsTrue(view.gameObject.activeSelf);
            Assert.IsFalse(view.FeedbackPending);
        }

        [UnityTest] public IEnumerator SharedSessionCompletionAlsoDismissesThePanel()
        {
            var view = Panel();
            var session = new QuestionSession();
            view.ShowQuestion(Question("external"), session);
            session.SubmitNumeric("23");
            yield return new WaitForSecondsRealtime(.8f);
            Assert.IsFalse(view.gameObject.activeSelf);
        }

        [UnityTest] public IEnumerator LegacyGameplayRetiresItsHudAndStartsOneCurrentAdventure()
        {
            temporaryScene = SceneManager.CreateScene("LegacyGameplayPolishTest");
            var host = new GameObject("GameplayRoot");
            SceneManager.MoveGameObjectToScene(host, temporaryScene);
            var legacy = host.AddComponent<Chapter1VerticalSliceController>();
            var oldCanvas = new GameObject("GameplayHUD", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(oldCanvas, temporaryScene);
            var safe = new GameObject("SafeArea", typeof(RectTransform));
            safe.transform.SetParent(oldCanvas.transform, false);
            new GameObject("ObjectiveAnchor", typeof(RectTransform)).transform.SetParent(safe.transform, false);
            var game = AdventureEntry.EnsureCurrent(temporaryScene);
            Assert.IsNotNull(game);
            game.Persistence = new MemoryStore();
            Assert.AreSame(game, AdventureEntry.EnsureCurrent(temporaryScene));
            Assert.IsFalse(legacy.enabled);
            Assert.IsFalse(oldCanvas.GetComponent<Canvas>().enabled);
            Assert.IsFalse(oldCanvas.GetComponent<GraphicRaycaster>().enabled);
            yield return null;
            Assert.IsNotNull(game.Hud);
        }

        private QuestionPanelView Panel()
        {
            panelObject = new GameObject("QuestionPanel", typeof(RectTransform));
            return panelObject.AddComponent<QuestionPanelView>();
        }
        private static QuestionInstance Question(string id)
        {
            return new QuestionInstance { InstanceId = id, Prompt = "16 + 7 = ?", Type = QuestionType.NumericInput,
                CorrectInteger = 23, MaxInputLength = 3 };
        }
        private sealed class MemoryStore : ISaveStore
        {
            private readonly Dictionary<string, string> values = new Dictionary<string, string>();
            public void Save(string key, string json) { values[key] = json; }
            public bool TryLoad(string key, out string json) { return values.TryGetValue(key, out json); }
            public void Delete(string key) { values.Remove(key); }
        }
    }
}
