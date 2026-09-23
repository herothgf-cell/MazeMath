using MazeMath.Adventure;
using MazeMath.Core.Save;
using MazeMath.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MazeMath.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            var services = GameServices.Ensure();
            var input = GetComponent<GameInputService>();
            if (input != null) services.Register<IGameInput>(input);
            if (!services.TryGet<SaveService>(out _))
                services.Register(new SaveService(new PlayerPrefsSaveStore()));
        }

        private void Start()
        {
            if (Instance != this) return;
            var scene = SceneManager.GetActiveScene();
            if (scene.name == "Bootstrap") AdventureEntry.EnsureCurrent(scene);
        }

        // Retained for callers of the original Foundation contract; runtime no longer assumes index 1.
        public static bool ShouldLoadGameplay(string activeSceneName, int sceneCountInBuildSettings)
        {
            return activeSceneName == "Bootstrap" && sceneCountInBuildSettings > 1;
        }
    }
}
