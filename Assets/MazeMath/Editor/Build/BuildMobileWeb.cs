using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MazeMath.Editor.Build
{
    /// <summary>Experimental mobile-browser profile. Does not upgrade the pinned Editor or replace desktop/APK profiles.</summary>
    public static class BuildMobileWeb
    {
        public const string OutputDirectory = "Build/MobileWeb";
        public const string TemplateDirectory = "Assets/WebGLTemplates/MazeMathMobile";

        [MenuItem("MazeMath/Build/Mobile Web (Experimental)")]
        public static void PerformBuild()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before building.");
            AdventureProjectTools.ValidateScene();
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new InvalidOperationException("Install WebGL Build Support for Unity 2022.3.62f2 in Unity Hub.");
            if (!File.Exists(TemplateDirectory + "/index.html"))
                throw new FileNotFoundException("Pull main: MazeMathMobile template is missing.");

            var oldTemplate = PlayerSettings.WebGL.template;
            var oldCompression = PlayerSettings.WebGL.compressionFormat;
            var oldFallback = PlayerSettings.WebGL.decompressionFallback;
            var oldThreads = PlayerSettings.WebGL.threadsSupport;
            var oldHash = PlayerSettings.WebGL.nameFilesAsHashes;
            var oldCache = PlayerSettings.WebGL.dataCaching;
            var oldInitialMemory = PlayerSettings.WebGL.initialMemorySize;
            var oldWidth = PlayerSettings.defaultWebScreenWidth;
            var oldHeight = PlayerSettings.defaultWebScreenHeight;
            string staging = "Build/.MobileWeb-" + Guid.NewGuid().ToString("N");
            Directory.CreateDirectory(staging);
            try
            {
                PlayerSettings.WebGL.template = "PROJECT:MazeMathMobile";
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = true;
                PlayerSettings.WebGL.threadsSupport = false;
                PlayerSettings.WebGL.nameFilesAsHashes = true;
                PlayerSettings.WebGL.dataCaching = true;
                PlayerSettings.WebGL.initialMemorySize = 128;
                PlayerSettings.defaultWebScreenWidth = 960;
                PlayerSettings.defaultWebScreenHeight = 540;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { AdventureProjectTools.ScenePath },
                    locationPathName = staging,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                });
                if (report == null || report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("Mobile Web build failed. See the Unity Console/build log.");
                ValidateOutput(staging);
                File.WriteAllText(Path.Combine(staging, ".nojekyll"), string.Empty);
                File.WriteAllText(Path.Combine(staging, "build-info.json"),
                    "{\"editor\":\"2022.3.62f2\",\"profile\":\"mobile-web-experimental\",\"deviceVerified\":false}");
                // Retain the last successful output until the replacement has built successfully.
                string backup = OutputDirectory + "-backup-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
                bool hadPrevious = Directory.Exists(OutputDirectory);
                if (hadPrevious) Directory.Move(OutputDirectory, backup);
                try { Directory.Move(staging, OutputDirectory); }
                catch { if (hadPrevious && !Directory.Exists(OutputDirectory)) Directory.Move(backup, OutputDirectory); throw; }
                Debug.Log("Mobile Web build files created in " + Path.GetFullPath(OutputDirectory) +
                    ". Device compatibility is not yet verified. Serve over HTTP/HTTPS, not file://.");
            }
            finally
            {
                PlayerSettings.WebGL.template = oldTemplate;
                PlayerSettings.WebGL.compressionFormat = oldCompression;
                PlayerSettings.WebGL.decompressionFallback = oldFallback;
                PlayerSettings.WebGL.threadsSupport = oldThreads;
                PlayerSettings.WebGL.nameFilesAsHashes = oldHash;
                PlayerSettings.WebGL.dataCaching = oldCache;
                PlayerSettings.WebGL.initialMemorySize = oldInitialMemory;
                PlayerSettings.defaultWebScreenWidth = oldWidth;
                PlayerSettings.defaultWebScreenHeight = oldHeight;
                // Failed output stays in its uniquely named staging folder for diagnosis.
            }
        }

        public static void ValidateOutput(string folder)
        {
            string html = Path.Combine(folder, "index.html"), build = Path.Combine(folder, "Build");
            if (!File.Exists(html) || !Directory.Exists(build)) throw new InvalidOperationException("Missing WebGL output files.");
            if (File.ReadAllText(html).Contains("{{{")) throw new InvalidOperationException("Unresolved Unity template token.");
            foreach (string pattern in new[] { "*.loader.js", "*.wasm*", "*.data*", "*.framework.js*" })
                if (Directory.GetFiles(build, pattern).Length == 0) throw new InvalidOperationException("Missing output: " + pattern);
            foreach (string file in new[] { "mobile-web.js", "mobile-web.css" })
                if (!File.Exists(Path.Combine(folder, file))) throw new InvalidOperationException("Missing template asset: " + file);
        }
    }
}
