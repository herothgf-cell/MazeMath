using System.IO;
using MazeMath.Editor;
using NUnit.Framework;
using UnityEditor;

namespace MazeMath.Tests.Editor
{
    public sealed class ProjectSetupTests
    {
        [Test]
        public void ConfigureProject_CreatesRequiredScenesAndBuildSettings()
        {
            ProjectSetup.ConfigureProject();

            Assert.IsTrue(File.Exists(ProjectSetup.BootstrapScenePath));
            Assert.IsTrue(File.Exists(ProjectSetup.GameplayScenePath));

            var scenes = EditorBuildSettings.scenes;
            Assert.AreEqual(2, scenes.Length);
            Assert.AreEqual(ProjectSetup.BootstrapScenePath, scenes[0].path);
            Assert.AreEqual(ProjectSetup.GameplayScenePath, scenes[1].path);
            Assert.IsTrue(scenes[0].enabled);
            Assert.IsTrue(scenes[1].enabled);
        }
    }
}
