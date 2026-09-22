using System;
using System.Linq;
using System.Text.Json;
using MazeMath.Adventure;
using NUnit.Framework;

namespace MazeMath.CoreTests
{
    public sealed class AdventureTests
    {
        [Test] public void NewRunStartsAtEntranceWithOnlyStarterMaterials()
        {
            var s = AdventureState.NewRun(18);
            Assert.AreEqual(6, s.x); Assert.AreEqual(0, s.y);
            Assert.AreEqual(1, s.materials[0]); Assert.AreEqual(1, s.materials[1]);
            Assert.IsFalse(s.owned.Any(v => v)); Assert.IsFalse(s.flags.Contains("clear"));
        }
        [Test] public void StarterMaterialsCannotBeSpentOnOptionalTool()
        {
            var s = AdventureState.NewRun(1); s.materials[2] = 10; s.materials[3] = 10;
            Assert.IsFalse(AdventureRules.Craft(s, 4)); Assert.AreEqual(1, s.materials[0]);
        }
        [Test] public void FirstQuestionFundsRequiredToolAndRewardIsOnce()
        {
            var s = AdventureState.NewRun(2);
            Assert.IsTrue(AdventureRules.Complete(s, "math"));
            Assert.IsFalse(AdventureRules.Complete(s, "math"));
            Assert.AreEqual(3, s.materials[0]);
            Assert.IsTrue(AdventureRules.Craft(s, 0)); Assert.IsTrue(s.equipped[0]);
            int n = s.materials[0]; Assert.IsFalse(AdventureRules.Craft(s, 0)); Assert.AreEqual(n, s.materials[0]);
        }
        [Test] public void WrongPatternNeverConsumesMaterials()
        {
            var s = RichState(); var copy = (int[])s.materials.Clone();
            Assert.IsFalse(AdventureRules.Craft(s, 4, new int[9])); CollectionAssert.AreEqual(copy, s.materials);
            Assert.IsTrue(AdventureRules.Craft(s, 4, AdventureRules.Pattern(4)));
        }
        [Test] public void PatternIsADefensiveCopy()
        {
            var p = AdventureRules.Pattern(4); p[0] = 99; Assert.AreNotEqual(99, AdventureRules.Pattern(4)[0]);
        }
        [Test] public void EnchantsArePaidOnceAndRequireEquippedToolForEffect()
        {
            var s = RichState(); AdventureRules.Craft(s, 4); int xp = s.xp;
            Assert.IsTrue(AdventureRules.Enchant(s, 4, 0)); Assert.AreEqual(xp - 10, s.xp);
            Assert.IsTrue(AdventureRules.Enchant(s, 4, 0)); Assert.AreEqual(xp - 10, s.xp);
            Assert.IsTrue(AdventureRules.HasEnchant(s, 4, 0));
            s.equipped[4] = false; Assert.IsFalse(AdventureRules.HasEnchant(s, 4, 0));
        }
        [Test] public void FullMaterialsPreserveOverflowAndFlushAfterCraft()
        {
            var s = RichState(); s.materials[0] = 99;
            AdventureRules.Claim(s, "cache-a", 8, new []{5,0,0,0,0});
            Assert.AreEqual(99, s.materials[0]); Assert.AreEqual(5, s.pending[0]);
            Assert.IsFalse(AdventureRules.Claim(s, "cache-a", 8, new []{5,0,0,0,0}));
            AdventureRules.Craft(s, 1); Assert.AreEqual(99, s.materials[0]); Assert.AreEqual(3, s.pending[0]);
        }
        [Test] public void GateLatchedOpenAfterToolUseSurvivesUnequip()
        {
            var s = RichState(); Assert.IsTrue(AdventureRules.OpenMiningGate(s));
            s.equipped[0] = false; Assert.IsTrue(AdventureWorld.MiningOpen(s));
        }
        [Test] public void BossRequiresThreeDifferentPhasesAndFinalInteraction()
        {
            var s = RichState(); Assert.AreEqual(3, AdventureRules.Shields(s));
            Assert.IsFalse(AdventureRules.Finish(s));
            AdventureRules.Complete(s, "boss.math"); AdventureRules.Complete(s, "boss.math");
            Assert.AreEqual(2, AdventureRules.Shields(s));
            AdventureRules.Complete(s, "boss.sequence"); AdventureRules.Complete(s, "boss.laser");
            Assert.IsTrue(AdventureRules.Finish(s)); Assert.IsFalse(AdventureRules.Finish(s));
        }
        [Test] public void QuestionsReproduceForSeedAndStayWithinChildFriendlyScope()
        {
            for (int i = 0; i < 1000; i++)
            {
                var a = AdventureQuestions.Create(i, "math", 0); var b = AdventureQuestions.Create(i, "math", 0);
                Assert.AreEqual(a.prompt, b.prompt); Assert.AreEqual(a.answer, b.answer);
                Assert.That(a.answer, Is.InRange(0, 99));
                Assert.AreEqual(1, a.choices.Count(v => v == a.answer)); Assert.AreEqual(4, a.choices.Distinct().Count());
            }
        }
        [Test] public void IncorrectAnswerKeepsQuestionAndDoesNotConsumeHealth()
        {
            var s = AdventureState.NewRun(41); var q = AdventureQuestions.Ensure(s, "math"); int hp = s.health;
            Assert.IsFalse(AdventureQuestions.Submit(s, (q.answer + 1).ToString()));
            Assert.AreSame(q, s.question); Assert.AreEqual(hp, s.health); Assert.AreEqual(1, q.attempts);
        }
        [Test] public void QuestionRewardUsesCapturedSourceNotPlayerLocation()
        {
            var s = AdventureState.NewRun(41); var q = AdventureQuestions.Ensure(s, "math");
            s.x = 42; s.y = 16; Assert.IsTrue(AdventureQuestions.Submit(s, q.answer.ToString()));
            Assert.IsTrue(s.Has("math")); Assert.IsFalse(s.Has("boss.math"));
            Assert.IsFalse(AdventureQuestions.Submit(s, q.answer.ToString()));
        }
        [Test] public void SaveRoundTripKeepsUnfinishedQuestionAndAllProgress()
        {
            var s = RichState(); AdventureRules.Craft(s, 4); AdventureRules.Enchant(s, 4, 1);
            AdventureRules.Complete(s, "bridge"); s.visited[9] = true; AdventureQuestions.Ensure(s, "boss.math");
            var options = new JsonSerializerOptions { IncludeFields = true };
            var loaded = JsonSerializer.Deserialize<AdventureState>(JsonSerializer.Serialize(s, options), options);
            Assert.IsTrue(loaded.IsValid()); Assert.AreEqual(s.question.prompt, loaded.question.prompt);
            Assert.AreEqual(s.question.answer, loaded.question.answer); CollectionAssert.AreEqual(s.materials, loaded.materials);
            CollectionAssert.AreEqual(s.enchants, loaded.enchants); Assert.IsTrue(loaded.Has("bridge"));
            Assert.IsTrue(loaded.visited[9]);
        }
        [Test] public void CorruptShapeAndImpossibleStateAreRejected()
        {
            var s = AdventureState.NewRun(1); s.materials = null; Assert.IsFalse(s.IsValid());
            s = AdventureState.NewRun(1); s.x = float.NaN; Assert.IsFalse(s.IsValid());
            s = AdventureState.NewRun(1); s.equipped[2] = true; Assert.IsFalse(s.IsValid());
            s = AdventureState.NewRun(1); s.enchants[2] = 0; Assert.IsFalse(s.IsValid());
        }
        [Test] public void WalkingIsPhysicalAndCannotCrossMiningGate()
        {
            var s = AdventureState.NewRun(1); var m = new AdventureMotor(34, 0);
            for (int i = 0; i < 100; i++) m.Tick(1, 0, false, .02f, s);
            Assert.LessOrEqual(m.X + AdventureMotor.HalfWidth, 35.6f + .01f);
            Assert.Greater(m.X, 34);
        }
        [Test] public void JumpCannotBypassFullHeightLockedGate()
        {
            var s = AdventureState.NewRun(1); var m = new AdventureMotor(34, 0);
            for (int i = 0; i < 150; i++) m.Tick(1, 0, i % 35 == 0, .02f, s);
            Assert.Less(m.X, 36);
        }
        [Test] public void OpeningGateAllowsTheSameMotorToPass()
        {
            var s = RichState(); AdventureRules.OpenMiningGate(s); var m = new AdventureMotor(34, 0);
            for (int i = 0; i < 50; i++) m.Tick(1, 0, false, .02f, s);
            Assert.Greater(m.X, 38);
        }
        [Test] public void LadderIsRequiredForFloorsAndCanBeTraversedBothWays()
        {
            var s = RichState(); s.flags.Add("bridge"); var m = new AdventureMotor(54, 0);
            for (int i = 0; i < 130; i++) m.Tick(0, 1, false, .02f, s);
            Assert.That(m.Y, Is.EqualTo(8).Within(.05));
            for (int i = 0; i < 130; i++) m.Tick(0, -1, false, .02f, s);
            Assert.That(m.Y, Is.EqualTo(0).Within(.05));
        }
        [Test] public void LockedUpperLadderCannotSkipSequencePuzzle()
        {
            var s = AdventureState.NewRun(1); var m = new AdventureMotor(30, 8);
            for (int i = 0; i < 120; i++) m.Tick(0, 1, false, .02f, s);
            Assert.That(m.Y, Is.EqualTo(8).Within(.05));
            s.flags.Add("sequence");
            for (int i = 0; i < 120; i++) m.Tick(0, 1, false, .02f, s);
            Assert.That(m.Y, Is.EqualTo(16).Within(.05));
        }
        [Test] public void MotorRejectsInvalidDeltaAndNeverDoubleJumps()
        {
            var s = AdventureState.NewRun(1); var m = new AdventureMotor(6, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => m.Tick(0, 0, false, float.NaN, s));
            m.Tick(0, 0, true, .02f, s); float vy = m.VelocityY;
            m.Tick(0, 0, true, .02f, s); Assert.Less(m.VelocityY, vy);
        }
        [Test] public void ResumeCorruptionDoesNotMutateValidBackup()
        {
            var s = AdventureState.NewRun(1); var copy = s.Copy(); copy.flags.Add("bridge"); copy.materials[0] = 40;
            Assert.IsFalse(s.Has("bridge")); Assert.AreEqual(1, s.materials[0]);
        }
        [Test] public void MultiTouchReleaseDoesNotCancelOtherHeldDirection()
        {
            var c = new AdventureControls(); c.Hold(12, 1); c.Hold(13, 2); c.Release(13);
            Assert.AreEqual(1, c.Horizontal); Assert.AreEqual(0, c.Vertical);
            c.Jump(); Assert.IsTrue(c.ConsumeJump()); Assert.IsFalse(c.ConsumeJump());
            c.Clear(); Assert.AreEqual(0, c.Horizontal);
        }
        [Test] public void AirborneResumeCannotCreateAnExtraJump()
        {
            var s = AdventureState.NewRun(1); var m = new AdventureMotor(6, 3);
            m.Tick(0, 0, true, .02f, s); Assert.LessOrEqual(m.VelocityY, 0);
        }
        [Test] public void SoftLandingDoesNotBlockBossPulseDamage()
        {
            var s = RichState(); AdventureRules.Craft(s, 2); AdventureRules.Enchant(s, 2, 0);
            AdventureRules.Recover(s); Assert.AreEqual(5, s.health);
            AdventureRules.Recover(s, false); Assert.AreEqual(4, s.health);
        }
        [Test] public void EdgePositionsRemainSaveable()
        {
            var s = AdventureState.NewRun(1); var m = new AdventureMotor(1, 0);
            for(int i=0;i<100;i++) m.Tick(-1,0,false,.02f,s);
            s.x=m.X; s.y=m.Y; Assert.IsTrue(s.IsValid());
        }
        private static AdventureState RichState()
        {
            var s = AdventureState.NewRun(41); s.materials = new[]{30,30,30,30,3}; s.xp = 100;
            AdventureRules.Craft(s, 0); return s;
        }
    }
}
