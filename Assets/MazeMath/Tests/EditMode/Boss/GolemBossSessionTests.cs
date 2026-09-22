using MazeMath.Boss;
using NUnit.Framework;

namespace MazeMath.Tests.Boss
{
    public sealed class GolemBossSessionTests
    {
        [Test]
        public void UniquePhases_RemoveOneShieldEach()
        {
            var boss = new GolemBossSession(3);

            Assert.AreEqual(3, boss.ShieldsRemaining);
            Assert.IsTrue(boss.CompletePhase("math"));
            Assert.AreEqual(2, boss.ShieldsRemaining);
            Assert.IsTrue(boss.CompletePhase("puzzle-a"));
            Assert.AreEqual(1, boss.ShieldsRemaining);
            Assert.IsTrue(boss.CompletePhase("puzzle-b"));
            Assert.AreEqual(0, boss.ShieldsRemaining);
            Assert.IsTrue(boss.CanFinish);
        }

        [Test]
        public void DuplicatePhase_DoesNotRemoveExtraShield()
        {
            var boss = new GolemBossSession(3);

            Assert.IsTrue(boss.CompletePhase("math"));
            Assert.IsFalse(boss.CompletePhase("math"));
            Assert.AreEqual(2, boss.ShieldsRemaining);
        }

        [Test]
        public void Finish_RequiresAllShieldsRemoved()
        {
            var boss = new GolemBossSession(3);

            Assert.IsFalse(boss.Finish());

            boss.CompletePhase("a");
            boss.CompletePhase("b");
            boss.CompletePhase("c");

            Assert.IsTrue(boss.Finish());
            Assert.IsTrue(boss.IsCleared);
        }
    }
}
