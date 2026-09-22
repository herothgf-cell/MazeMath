using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace MazeMath.Editor.Build
{
    public static class BuildWeb
    {
        public const string OutputDirectory = "Build/Web";

        [MenuItem("MazeMath/Build/WebGL")]
        public static void PerformBuild()
        {
            ProjectSetup.ConfigureProject();
            BuildValidation.ValidateOrThrow();

            Directory.CreateDirectory(OutputDirectory);

            var options = new BuildPlayerOptions
            {
                scenes = new[]
                {
                    ProjectSetup.BootstrapScenePath,
                    ProjectSetup.GameplayScenePath
                },
                locationPathName = OutputDirectory,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"WebGL build failed: {report.summary.result}, errors={report.summary.totalErrors}");
            }
        }
    }
}
