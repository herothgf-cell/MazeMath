using System.Collections;
using System.Collections.Generic;
using MazeMath.Adventure;
using MazeMath.Core.Save;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MazeMath.Tests.Adventure
{
    public sealed class AdventurePlayTests
    {
        private AdventureGame game;
        private sealed class MemoryStore : ISaveStore
        {
            private readonly Dictionary<string,string> values=new Dictionary<string,string>();
            public void Save(string key,string json) { values[key]=json; }
            public bool TryLoad(string key,out string json) { return values.TryGetValue(key,out json); }
            public void Delete(string key) { values.Remove(key); }
        }
        [UnitySetUp] public IEnumerator Setup()
        {
            game=new GameObject("Adventure Test").AddComponent<AdventureGame>();
            game.Persistence=new MemoryStore(); yield return null;
            game.SendMessage("OnApplicationFocus",true);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(game!=null) Object.Destroy(game.gameObject); yield return null;
        }
        [UnityTest] public IEnumerator TitleNewGameMovementAndPause()
        {
            Assert.IsTrue(game.Hud.Paused); Assert.IsFalse(game.Running);
            game.BeginNew(); Assert.IsTrue(game.Running); Assert.IsFalse(game.Hud.Paused);
            float x=game.Motor.X; game.Controls.Hold(1,1);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.Greater(game.Motor.X,x); game.Hud.Pause(); x=game.Motor.X;
            yield return new WaitForFixedUpdate(); Assert.AreEqual(x,game.Motor.X); Assert.AreEqual(0,game.Controls.Horizontal);
        }
        [UnityTest] public IEnumerator WorkshopAndQuestionArePlayableWithoutSceneRegeneration()
        {
            game.BeginNew(); game.Motor.Place(18,0); AdventureRules.Complete(game.State,"math");
            Assert.IsTrue(game.Craft(0)); Assert.IsTrue(game.State.equipped[0]);
            var q=AdventureQuestions.Ensure(game.State,"boss.math"); game.Hud.Question();
            Assert.IsTrue(game.Answer(q.answer.ToString())); Assert.IsTrue(game.State.Has("boss.math"));
            game.Hud.Close(); game.Commit(); game.Title(); Assert.IsTrue(game.CanContinue);
            game.Continue(); Assert.IsTrue(game.State.owned[0]); Assert.IsTrue(game.State.Has("boss.math"));
            yield return null;
        }
    }
}
