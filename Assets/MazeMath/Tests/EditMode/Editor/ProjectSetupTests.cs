using System.IO;
using System.Linq;
using MazeMath.Editor;
using NUnit.Framework;
using UnityEditor;

namespace MazeMath.Tests.Editor
{
    public sealed class ProjectSetupTests
    {
        [Test] public void ConfigureProject_CreatesMissingLegacyScenesWithoutReplacingExistingOnes()
        {
            ProjectSetup.ConfigureProject();
            Assert.IsTrue(File.Exists(ProjectSetup.BootstrapScenePath));
            Assert.IsTrue(File.Exists(ProjectSetup.GameplayScenePath));
            byte[] bootstrap=File.ReadAllBytes(ProjectSetup.BootstrapScenePath);
            byte[] gameplay=File.ReadAllBytes(ProjectSetup.GameplayScenePath);
            byte[] adventure=File.ReadAllBytes(AdventureProjectTools.ScenePath);
            ProjectSetup.ConfigureProject();
            CollectionAssert.AreEqual(bootstrap,File.ReadAllBytes(ProjectSetup.BootstrapScenePath));
            CollectionAssert.AreEqual(gameplay,File.ReadAllBytes(ProjectSetup.GameplayScenePath));
            CollectionAssert.AreEqual(adventure,File.ReadAllBytes(AdventureProjectTools.ScenePath));
            Assert.IsTrue(EditorBuildSettings.scenes.Any(v=>v.path==ProjectSetup.BootstrapScenePath));
            Assert.IsTrue(EditorBuildSettings.scenes.Any(v=>v.path==ProjectSetup.GameplayScenePath));
        }
    }
}
