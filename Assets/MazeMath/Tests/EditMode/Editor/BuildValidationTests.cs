using MazeMath.Editor;
using MazeMath.Editor.Build;
using NUnit.Framework;

namespace MazeMath.Tests.Editor
{
    public sealed class BuildValidationTests
    {
        [Test]
        public void ConfiguredProject_PassesRequiredSceneValidation()
        {
            ProjectSetup.ConfigureProject();

            var result = BuildValidation.Validate();

            Assert.IsTrue(result.IsValid, string.Join("\n", result.Errors));
        }
    }
}
