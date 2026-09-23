using System.Collections;
using System.Collections.Generic;
using MazeMath.Adventure;
using MazeMath.Core.Save;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MazeMath.Tests.UI
{
    public sealed class AdventureHudPolishTests
    {
        private GameObject root;
        private AdventureGame game;
        private sealed class MemoryStore : ISaveStore
        {
            private readonly Dictionary<string,string> data=new Dictionary<string,string>();
            public void Save(string key,string json) { data[key]=json; }
            public bool TryLoad(string key,out string json) => data.TryGetValue(key,out json);
            public void Delete(string key) { data.Remove(key); }
        }
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            root=new GameObject("HUD integration test"); game=root.AddComponent<AdventureGame>(); game.Persistence=new MemoryStore();
            yield return null; game.BeginNew(); yield return null;
        }
        [UnityTearDown]
        public IEnumerator TearDown() { if(root!=null) Object.Destroy(root); yield return null; }
        private void OpenQuestion()
        {
            AdventureQuestions.Ensure(game.State,"math"); game.Hud.Question();
        }
        [UnityTest]
        public IEnumerator CorrectAnswerClosesActualModalAndCannotGrantTwice()
        {
            OpenQuestion(); string answer=game.State.question.answer.ToString();
            game.Hud.SubmitAnswer(answer); int xp=game.State.xp;
            game.Hud.SubmitAnswer(answer);
            Assert.That(game.Hud.QuestionFeedbackPending,Is.True);
            Assert.That(game.Hud.Paused,Is.True);
            Assert.That(game.State.xp,Is.EqualTo(xp));
            yield return new WaitForSecondsRealtime(.8f);
            Assert.That(game.Hud.IsQuestionOpen,Is.False);
            Assert.That(game.Hud.Paused,Is.False);
            Assert.That(game.State.question.solved,Is.True);
        }
        [UnityTest]
        public IEnumerator WrongAnswerKeepsRetryUiOpen()
        {
            OpenQuestion(); game.Hud.SubmitAnswer((game.State.question.answer+1).ToString());
            yield return new WaitForSecondsRealtime(.8f);
            Assert.That(game.Hud.IsQuestionOpen,Is.True);
            Assert.That(game.Hud.QuestionFeedbackPending,Is.False);
        }
        [UnityTest]
        public IEnumerator OldSuccessTimerCannotDismissNewBag()
        {
            OpenQuestion(); game.Hud.SubmitAnswer(game.State.question.answer.ToString());
            game.Hud.Close(); game.Hud.Bag();
            yield return new WaitForSecondsRealtime(.8f);
            Assert.That(game.Hud.Paused,Is.True);
            Assert.That(game.Hud.IsQuestionOpen,Is.False);
        }
        [UnityTest]
        public IEnumerator ExplorationButtonsDoNotOverlapCameraViewport()
        {
            game.Hud.SetTouchControls(true); yield return null; Canvas.ForceUpdateCanvases();
            var v=game.Hud.PlayfieldViewport;
            var world=new Rect(v.x*Screen.width,v.y*Screen.height,v.width*Screen.width,v.height*Screen.height);
            world.xMin+=1; world.yMin+=1; world.xMax-=1; world.yMax-=1;
            var corners=new Vector3[4];
            foreach(var b in game.Hud.GetComponentsInChildren<Button>())
            {
                if(!b.gameObject.activeInHierarchy) continue;
                b.GetComponent<RectTransform>().GetWorldCorners(corners);
                var a=RectTransformUtility.WorldToScreenPoint(null,corners[0]);
                var z=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
                var bounds=Rect.MinMaxRect(a.x,a.y,z.x,z.y);
                Assert.That(bounds.Overlaps(world),Is.False,"UI overlaps world: "+b.name);
            }
        }
        [UnityTest]
        public IEnumerator DialogTypographyHasAMaximumAndArtIsSmooth()
        {
            OpenQuestion(); yield return null;
            foreach(var label in game.Hud.GetComponentsInChildren<Text>()) Assert.That(label.fontSize,Is.LessThanOrEqualTo(28),label.name);
            var art=new AdventureArt();
            try { Assert.That(art.Get("robot").texture.filterMode,Is.EqualTo(FilterMode.Bilinear)); Assert.That(art.Get("robot").texture.width,Is.GreaterThanOrEqualTo(96)); }
            finally { art.Dispose(); }
        }
    }
}
