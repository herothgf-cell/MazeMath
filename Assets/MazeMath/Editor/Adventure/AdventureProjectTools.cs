using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MazeMath.Editor
{
    public static class AdventureProjectTools
    {
        public const string ScenePath = "Assets/MazeMath/Scenes/Adventure.unity";
        public const string EditorVersion = "2022.3.62f2";

        [MenuItem("MazeMath/Play Adventure")]
        public static void PlayAdventure()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ValidateScene();
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.isPlaying = true;
        }

        [MenuItem("MazeMath/Validate Adventure")]
        public static void ValidateScene()
        {
            if (!File.Exists(ScenePath)) throw new FileNotFoundException("Pull main: Adventure.unity is missing.", ScenePath);
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null) throw new InvalidOperationException("Adventure scene could not be imported.");
            if (Application.unityVersion != EditorVersion) throw new InvalidOperationException("Use Unity " + EditorVersion + "; found " + Application.unityVersion);
            Debug.Log("MazeMath Adventure scene is present; existing scenes were not changed.");
        }

        public static void Web()
        {
            ValidateScene(); EnsureModule(BuildTarget.WebGL);
            Directory.CreateDirectory("Build/Web");
            var old = PlayerSettings.WebGL.compressionFormat;
            try
            {
                // Portable local smoke build: no server compression headers needed.
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
                RunBuild(BuildTarget.WebGL, "Build/Web");
            }
            finally { PlayerSettings.WebGL.compressionFormat = old; }
        }

        public static void Android()
        {
            ValidateScene(); EnsureModule(BuildTarget.Android);
            Directory.CreateDirectory("Build/Android");
            var oldBundle = EditorUserBuildSettings.buildAppBundle;
            var oldSigning = PlayerSettings.Android.useCustomKeystore;
            var oldArch = PlayerSettings.Android.targetArchitectures;
            var oldMin = PlayerSettings.Android.minSdkVersion;
            var oldBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            string oldId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            var orientation = PlayerSettings.defaultInterfaceOrientation;
            try
            {
                EditorUserBuildSettings.buildAppBundle = false;
                PlayerSettings.Android.useCustomKeystore = false; // local debug APK, not a store release
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.herothgf.mazemath");
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
                RunBuild(BuildTarget.Android, "Build/Android/MazeMath.apk");
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = oldBundle;
                PlayerSettings.Android.useCustomKeystore = oldSigning;
                PlayerSettings.Android.targetArchitectures = oldArch;
                PlayerSettings.Android.minSdkVersion = oldMin;
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, oldBackend);
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, oldId);
                PlayerSettings.defaultInterfaceOrientation = orientation;
            }
        }

        private static void EnsureModule(BuildTarget target)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
                throw new InvalidOperationException("Install " + target + " Build Support for Unity " + EditorVersion + " in Unity Hub.");
        }
        private static void RunBuild(BuildTarget target, string output)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before building.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath }, locationPathName = output, target = target, options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException(target + " build failed: " + report.summary.result + ", errors=" + report.summary.totalErrors);
            Debug.Log(target + " Adventure build succeeded: " + output);
        }
    }
}
