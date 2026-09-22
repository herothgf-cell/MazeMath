using NUnit.Framework;

namespace MazeMath.Tests.EditMode
{
    public sealed class ProjectSmokeTests
    {
        [Test]
        public void TestAssembly_Loads()
        {
            Assert.That(typeof(ProjectSmokeTests).Assembly, Is.Not.Null);
        }
    }
}
