using System;
using System.Collections.Generic;
using MazeMath.Adventure;
using MazeMath.Core.Save;
using NUnit.Framework;

namespace MazeMath.Tests.Adventure
{
    public sealed class AdventureSaveTests
    {
        private sealed class MemoryStore : ISaveStore
        {
            public readonly Dictionary<string,string> Data = new Dictionary<string,string>();
            public void Save(string key,string json) { Data[key]=json; }
            public bool TryLoad(string key,out string json) { return Data.TryGetValue(key,out json); }
            public void Delete(string key) { Data.Remove(key); }
        }
        [Test] public void JsonUtilityRoundTripPreservesQuestionAndMaterials()
        {
            var store=new MemoryStore(); var save=new AdventureSave(store); var s=AdventureState.NewRun(17);
            AdventureRules.Complete(s,"math"); AdventureRules.Craft(s,0); AdventureQuestions.Ensure(s,"boss.math");
            save.Save(s); Assert.IsTrue(save.TryLoad(out var loaded,out var backup));
            Assert.IsFalse(backup); Assert.IsTrue(loaded.IsValid()); Assert.IsTrue(loaded.owned[0]);
            Assert.AreEqual(s.question.prompt,loaded.question.prompt); CollectionAssert.AreEqual(s.materials,loaded.materials);
        }
        [Test] public void BadPrimaryUsesGoodBackupWithoutDeletingEither()
        {
            var store=new MemoryStore(); var save=new AdventureSave(store); var s=AdventureState.NewRun(17);
            save.Save(s); s.x=18; save.Save(s); string backup=store.Data[AdventureSave.Key+".backup"];
            store.Data[AdventureSave.Key]="bad-json";
            Assert.IsTrue(save.TryLoad(out var loaded,out var recovered)); Assert.IsTrue(recovered); Assert.AreEqual(6,loaded.x);
            Assert.AreEqual("bad-json",store.Data[AdventureSave.Key]); Assert.AreEqual(backup,store.Data[AdventureSave.Key+".backup"]);
        }
        [Test] public void InvalidStateNeverOverwritesValidSave()
        {
            var store=new MemoryStore(); var save=new AdventureSave(store); var s=AdventureState.NewRun(17);
            save.Save(s); string before=store.Data[AdventureSave.Key]; s.materials=null;
            Assert.Throws<ArgumentException>(()=>save.Save(s)); Assert.AreEqual(before,store.Data[AdventureSave.Key]);
        }
    }
}
