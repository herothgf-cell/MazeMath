using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace MazeMath.Editor.Build
{
    public sealed class BuildValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new List<string>();
    }

    public static class BuildValidation
    {
        public static BuildValidationResult Validate()
        {
            var result = new BuildValidationResult();

            ValidateScene(ProjectSetup.BootstrapScenePath, result);
            ValidateScene(ProjectSetup.GameplayScenePath, result);

            var enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (!enabledScenes.Contains(ProjectSetup.BootstrapScenePath))
            {
                result.Errors.Add("Bootstrap scene is not enabled in Build Settings.");
            }

            if (!enabledScenes.Contains(ProjectSetup.GameplayScenePath))
            {
                result.Errors.Add("Gameplay scene is not enabled in Build Settings.");
            }

            return result;
        }

        public static void ValidateOrThrow()
        {
            var result = Validate();
            if (!result.IsValid)
            {
                throw new System.InvalidOperationException(
                    "MazeMath build validation failed:\n" + string.Join("\n", result.Errors));
            }
        }

        private static void ValidateScene(string path, BuildValidationResult result)
        {
            if (!File.Exists(path))
            {
                result.Errors.Add("Required scene is missing: " + path);
            }
        }
    }
}
