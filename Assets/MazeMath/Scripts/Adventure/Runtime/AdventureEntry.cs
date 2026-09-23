using System.Collections.Generic;
using MazeMath.Demo;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MazeMath.Adventure
{
    /// <summary>
    /// Existing Bootstrap/Gameplay scenes launch the same current adventure as Play Adventure.
    /// Only runtime instances are affected: no scene files, authored assets or save keys are rewritten.
    /// </summary>
    public static class AdventureEntry
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InstallSceneHook()
        {
            // Also safe when Enter Play Mode has domain reload disabled.
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (LegacyDemos(scene).Count > 0) EnsureCurrent(scene);
        }

        public static AdventureGame EnsureCurrent(Scene scene)
        {
            if (!Application.isPlaying || !scene.IsValid() || !scene.isLoaded) return null;
            var demos = LegacyDemos(scene);
            // A load callback followed by Bootstrap.Start must never create two games.
            var existing = Object.FindObjectOfType<AdventureGame>();
            if (existing != null)
            {
                RetireDemoUi(scene, demos);
                return existing;
            }
            foreach (var root in scene.GetRootGameObjects())
            {
                var authored = root.GetComponentInChildren<AdventureGame>(true);
                if (authored != null) return authored; // Preserve a deliberately disabled authored root too.
            }
            if (!AdventureStartupPolicy.ShouldRedirect(scene.name, demos.Count > 0, false)) return null;

            RetireDemoUi(scene, demos);
            var host = new GameObject("MazeMath Adventure");
            host.SetActive(false);
            SceneManager.MoveGameObjectToScene(host, scene);
            var adventure = host.AddComponent<AdventureGame>();
            host.SetActive(true);
            Debug.Log("MazeMath: " + AdventureStartupPolicy.UiRevision + " launched from " + scene.name + ".", host);
            return adventure;
        }

        private static List<Chapter1VerticalSliceController> LegacyDemos(Scene scene)
        {
            var result = new List<Chapter1VerticalSliceController>();
            if (!scene.IsValid() || !scene.isLoaded) return result;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var demo in root.GetComponentsInChildren<Chapter1VerticalSliceController>(true))
                if (demo.isActiveAndEnabled) result.Add(demo);
            return result;
        }

        private static void RetireDemoUi(Scene scene, List<Chapter1VerticalSliceController> demos)
        {
            foreach (var demo in demos)
            {
                demo.enabled = false; // sceneLoaded happens before the demo's Start builds its old UI.
                var oldHud = demo.GetComponent<Chapter1DemoHud>();
                if (oldHud != null) oldHud.enabled = false;
            }
            foreach (var root in scene.GetRootGameObjects())
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                bool setupShell = canvas.name == "GameplayHUD" && canvas.transform.Find("SafeArea/ObjectiveAnchor") != null;
                bool generatedDemo = canvas.transform.Find("DemoControlPanel") != null;
                if (!setupShell && !generatedDemo) continue;
                canvas.enabled = false;
                var raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster != null) raycaster.enabled = false;
            }
        }
    }
}
