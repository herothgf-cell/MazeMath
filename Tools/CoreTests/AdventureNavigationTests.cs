using MazeMath.Adventure;
using NUnit.Framework;

namespace MazeMath.CoreTests
{
    public sealed class AdventureNavigationTests
    {
        [Test] public void GuideUsesLadderRouteInsteadOfPointingThroughFloor()
        {
            var s=AdventureState.NewRun(41); s.owned[0]=s.equipped[0]=true;
            AdventureRules.Complete(s,"math"); AdventureRules.OpenMiningGate(s); AdventureRules.Complete(s,"bridge");
            s.x=46; s.y=0; AdventureWorld.Objective(s,out var x,out var y,out var key);
            Assert.AreEqual("laser",key); Assert.Greater(x,s.x); Assert.AreEqual(0,y);
        }
        [Test] public void GuidePointsToUpperLadderAfterSequence()
        {
            var s=AdventureState.NewRun(41); s.owned[0]=s.equipped[0]=true;
            foreach(string id in new[]{"math","bridge","laser","sequence"}) AdventureRules.Complete(s,id);
            AdventureRules.OpenMiningGate(s); s.x=30; s.y=8;
            AdventureWorld.Objective(s,out var x,out var y,out var key);
            Assert.AreEqual("boss.math",key); Assert.AreEqual(30,x); Assert.AreEqual(16,y);
        }
        [Test] public void GraphRejectsShortcutBeforeSequenceUnlock()
        {
            var s=AdventureState.NewRun(1); var g=AdventureNavigator.Build(s);
            Assert.IsEmpty(g.FindPath("r1","r6")); s.flags.Add("sequence");
            Assert.IsNotEmpty(AdventureNavigator.Build(s).FindPath("r1","r6"));
        }
    }
}
