using System.IO;
using MazeMath.Core;
using MazeMath.Input;
using MazeMath.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MazeMath.Editor
{
    public static class ProjectSetup
    {
        public const string BootstrapScenePath = "Assets/MazeMath/Scenes/Bootstrap.unity";
        public const string GameplayScenePath = "Assets/MazeMath/Scenes/Gameplay.unity";

        [MenuItem("MazeMath/Setup Project")]
        public static void ConfigureProject()
        {
            Directory.CreateDirectory("Assets/MazeMath/Scenes");
            Directory.CreateDirectory("Assets/MazeMath/Prefabs");
            Directory.CreateDirectory("Assets/MazeMath/ScriptableObjects");

            CreateBootstrapScene();
            CreateGameplayScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootstrapScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true)
            };

            ConfigurePlayerSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrapObject = new GameObject("GameBootstrap");
            bootstrapObject.AddComponent<GameBootstrap>();
            bootstrapObject.AddComponent<GameInputService>();

            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        private static void CreateGameplayScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateMainCamera();
            CreateHud();
            CreateEventSystem();

            var gameplayRoot = new GameObject("GameplayRoot");
            gameplayRoot.transform.position = Vector3.zero;

            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void CreateMainCamera()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";

            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.10f, 0.12f, 1f);

            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void CreateHud()
        {
            var canvasObject = new GameObject(
                "GameplayHUD",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var safeAreaObject = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
            safeAreaObject.transform.SetParent(canvasObject.transform, false);

            var safeArea = safeAreaObject.GetComponent<RectTransform>();
            safeArea.anchorMin = Vector2.zero;
            safeArea.anchorMax = Vector2.one;
            safeArea.offsetMin = Vector2.zero;
            safeArea.offsetMax = Vector2.zero;

            CreateAnchor("ObjectiveAnchor", safeArea, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f));
            CreateAnchor("MiniMapAnchor", safeArea, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f));
            CreateAnchor("HotbarAnchor", safeArea, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f));
            CreateAnchor("MobileMoveAnchor", safeArea, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f));
            CreateAnchor("MobileActionAnchor", safeArea, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 24f));
        }

        private static void CreateAnchor(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition)
        {
            var objectWithRect = new GameObject(name, typeof(RectTransform));
            objectWithRect.transform.SetParent(parent, false);

            var rect = objectWithRect.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(
                Mathf.Approximately(anchorMin.x, 1f) ? 1f : anchorMin.x,
                Mathf.Approximately(anchorMin.y, 1f) ? 1f : anchorMin.y);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(360f, 180f);
        }

        private static void CreateEventSystem()
        {
            new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "MazeMath";
            PlayerSettings.productName = "MazeMath";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.herothgf.mazemath");

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
        }
    }
}
