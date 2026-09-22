using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace MazeMath.Editor.Build
{
    public static class BuildAndroid
    {
        public const string OutputDirectory = "Build/Android";
        public const string OutputPath = OutputDirectory + "/MazeMath.apk";

        [MenuItem("MazeMath/Build/Android APK")]
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
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Android build failed: {report.summary.result}, errors={report.summary.totalErrors}");
            }
        }
    }
}
