using UnityEditor;
namespace MazeMath.Editor.Build
{
    public static class BuildWeb
    {
        public const string OutputDirectory = "Build/Web";
        [MenuItem("MazeMath/Build/WebGL")]
        public static void PerformBuild() { AdventureProjectTools.Web(); }
    }
}
