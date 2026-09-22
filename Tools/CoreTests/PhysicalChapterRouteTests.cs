using System;
using System.Collections.Generic;
using MazeMath.Adventure;
using MazeMath.Puzzles.Types;
using NUnit.Framework;

namespace MazeMath.CoreTests
{
    public sealed class PhysicalChapterRouteTests
    {
        [Test] public void WalkClimbCraftAndSolveTheWholeAuthoredRoute()
        {
            var s=AdventureState.NewRun(37); var m=new AdventureMotor(6,0);
            Walk(m,s,30); SolveMath(s,"math");
            Walk(m,s,18); Assert.IsTrue(AdventureRules.Craft(s,0));
            Walk(m,s,34); Assert.IsTrue(AdventureRules.OpenMiningGate(s));
            Walk(m,s,44);
            var weight=new WeightBridgePuzzle("bridge",8); weight.StartPuzzle();
            weight.PlaceWeight("a",3); weight.PlaceWeight("b",5); Assert.IsTrue(weight.IsSolved());
            AdventureRules.Complete(s,"bridge"); Walk(m,s,54); Climb(m,s,8);
            Walk(m,s,45);
            var laser=new LaserMirrorPuzzle("laser",12,7,new GridPosition(2,1),GridDirection.Up,new GridPosition(9,1),null,
                new Dictionary<GridPosition,MirrorOrientation>{{new GridPosition(2,4),MirrorOrientation.Backslash},{new GridPosition(9,4),MirrorOrientation.Slash}});
            laser.StartPuzzle(); laser.RotateMirror(new GridPosition(2,4)); laser.RotateMirror(new GridPosition(9,4));
            Assert.IsTrue(laser.IsSolved()); AdventureRules.Complete(s,"laser");
            Walk(m,s,18); CompletePlates(s,"sequence"); Walk(m,s,30); Climb(m,s,16);
            Walk(m,s,38); SolveMath(s,"boss.math"); Walk(m,s,44); CompletePlates(s,"boss.sequence"); Walk(m,s,54);
            var boss=new LaserMirrorPuzzle("boss.laser",7,7,new GridPosition(0,4),GridDirection.Right,new GridPosition(3,6),null,
                new Dictionary<GridPosition,MirrorOrientation>{{new GridPosition(3,4),MirrorOrientation.Backslash}});
            boss.StartPuzzle(); boss.RotateMirror(new GridPosition(3,4)); Assert.IsTrue(boss.IsSolved());
            AdventureRules.Complete(s,"boss.laser"); Walk(m,s,57); Assert.IsTrue(AdventureRules.Finish(s));
            s.x=m.X; s.y=m.Y; Assert.IsTrue(s.IsValid()); Assert.AreEqual(0,AdventureRules.Shields(s));
        }
        private static void Walk(AdventureMotor m,AdventureState s,float x)
        {
            for(int i=0;i<800 && Math.Abs(m.X-x)>.13f;i++) m.Tick(m.X<x?1:-1,0,false,.02f,s);
            Assert.That(m.X,Is.EqualTo(x).Within(.14f),"Cannot walk to "+x+" from floor "+m.Y);
            s.x=m.X; s.y=m.Y;
        }
        private static void Climb(AdventureMotor m,AdventureState s,float y)
        {
            for(int i=0;i<400 && Math.Abs(m.Y-y)>.01f;i++) m.Tick(0,m.Y<y?1:-1,false,.02f,s);
            Assert.That(m.Y,Is.EqualTo(y).Within(.02f)); s.x=m.X; s.y=m.Y;
        }
        private static void SolveMath(AdventureState s,string id)
        {
            var q=AdventureQuestions.Ensure(s,id); Assert.IsTrue(AdventureQuestions.Submit(s,q.answer.ToString()));
        }
        private static void CompletePlates(AdventureState s,string id)
        {
            var p=new SequencePlatePuzzle(id,new[]{2,4,6}); p.StartPuzzle(); p.Step(2);p.Step(4);p.Step(6);
            Assert.IsTrue(p.IsSolved()); AdventureRules.Complete(s,id);
        }
    }
}
