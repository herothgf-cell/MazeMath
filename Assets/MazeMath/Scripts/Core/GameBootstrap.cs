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
            if (input != null)
            {
                services.Register<IGameInput>(input);
            }

            if (!services.TryGet<SaveService>(out _))
            {
                services.Register(new SaveService(new PlayerPrefsSaveStore()));
            }
        }

        private void Start()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (ShouldLoadGameplay(activeScene.name, SceneManager.sceneCountInBuildSettings))
            {
                SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
            }
        }

        public static bool ShouldLoadGameplay(
            string activeSceneName,
            int sceneCountInBuildSettings)
        {
            return activeSceneName == "Bootstrap" &&
                   sceneCountInBuildSettings > 1;
        }
    }
}
