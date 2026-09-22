using MazeMath.Core.Save;
using MazeMath.Input;
using UnityEngine;

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
    }
}
