using System.Collections.Generic;
using System.IO;
using MazeMath.Content;
using MazeMath.Core;
using MazeMath.Demo;
using MazeMath.Input;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace MazeMath.Editor
{
    // Legacy demo setup is retained, but never regenerates existing scenes or authored content.
    public static class ProjectSetup
    {
        public const string BootstrapScenePath = "Assets/MazeMath/Scenes/Bootstrap.unity";
        public const string GameplayScenePath = "Assets/MazeMath/Scenes/Gameplay.unity";
        [MenuItem("MazeMath/Setup Project")]
        public static void ConfigureProject()
        {
            if(EditorApplication.isPlaying) { Debug.LogWarning("Stop Play Mode before project setup."); return; }
            Directory.CreateDirectory("Assets/MazeMath/Scenes");
            var catalog=AssetDatabase.LoadAssetAtPath<Chapter1ContentCatalog>(Chapter1ContentSetup.CatalogPath);
            if(catalog==null)
            {
                // A partial/invalid catalog must not cause the generator to overwrite surviving authored assets.
                if(Directory.Exists(Chapter1ContentSetup.Root) && Directory.GetFiles(Chapter1ContentSetup.Root,"*.asset",SearchOption.AllDirectories).Length>0)
                    throw new System.InvalidOperationException("Legacy catalog is missing or invalid. Existing content was preserved. Open Adventure.unity, or restore the legacy catalog from version control.");
                catalog=Chapter1ContentSetup.CreateOrUpdate();
            }
            CreateMissingScene(BootstrapScenePath,true,catalog);
            CreateMissingScene(GameplayScenePath,false,catalog);
            var entries=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach(var path in new[]{BootstrapScenePath,GameplayScenePath})
            {
                int index=entries.FindIndex(v=>v.path==path);
                if(index<0) entries.Add(new EditorBuildSettingsScene(path,true));
            }
            EditorBuildSettings.scenes=entries.ToArray();
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Debug.Log("MazeMath setup checked. Existing scenes and content were preserved. Open Adventure.unity for the playable chapter.");
        }
        private static void CreateMissingScene(string path,bool bootstrap,Chapter1ContentCatalog catalog)
        {
            if(File.Exists(path)) return;
            var previous=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            try
            {
                if(bootstrap)
                {
                    var go=new GameObject("GameBootstrap"); go.AddComponent<GameInputService>(); go.AddComponent<GameBootstrap>();
                }
                else
                {
                    var cameraObject=new GameObject("Main Camera",typeof(Camera)); cameraObject.tag="MainCamera";
                    var camera=cameraObject.GetComponent<Camera>(); camera.orthographic=true; camera.orthographicSize=5.4f;
                    cameraObject.transform.position=new Vector3(0,0,-10);
                    var events=new GameObject("EventSystem",typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
                    events.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
#else
                    events.AddComponent<StandaloneInputModule>();
#endif
                    new GameObject("GameplayRoot").AddComponent<Chapter1VerticalSliceController>().Catalog=catalog;
                }
                if(!EditorSceneManager.SaveScene(scene,path)) throw new IOException("Could not save missing scene: "+path);
            }
            finally
            {
                if(previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                EditorSceneManager.CloseScene(scene,true);
            }
        }
    }
}
