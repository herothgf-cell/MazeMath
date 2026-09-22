using UnityEditor;
namespace MazeMath.Editor.Build
{
    public static class BuildAndroid
    {
        public const string OutputDirectory = "Build/Android";
        public const string OutputPath = OutputDirectory + "/MazeMath.apk";
        [MenuItem("MazeMath/Build/Android APK")]
        public static void PerformBuild() { AdventureProjectTools.Android(); }
    }
}
