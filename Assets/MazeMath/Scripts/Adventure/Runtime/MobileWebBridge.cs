using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace MazeMath.Adventure
{
    /// <summary>Template-to-player bridge. Created only in a real WebGL player, never in native/Editor sessions.</summary>
    [Preserve]
    public sealed class MobileWebBridge : MonoBehaviour
    {
        private AdventureGame game;
        private AdventureHud configuredHud;
        private bool configured, touch, shouldSuspend;
        private int previousFrameRate;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (Object.FindObjectOfType<MobileWebBridge>() != null) return;
            var root = new GameObject("MazeMathMobileWebBridge");
            DontDestroyOnLoad(root);
            root.AddComponent<MobileWebBridge>();
#endif
        }

        private void Awake() { previousFrameRate = Application.targetFrameRate; }

        // Called once createUnityInstance resolves; browser UA/touch capabilities include desktop-mode iPad.
        [Preserve]
        public void ConfigureBrowser(string mode)
        {
            touch = mode == "touch";
            configured = true;
            if (touch) Application.targetFrameRate = 30;
        }

        [Preserve]
        public void SuspendBrowser(string unused)
        {
            shouldSuspend = true;
            ApplySuspension();
        }

        private void Update()
        {
            if (game == null) game = Object.FindObjectOfType<AdventureGame>();
            if (game == null || game.Hud == null) return;
            if (configured && configuredHud != game.Hud)
            {
                configuredHud = game.Hud;
                if (touch)
                {
                    // A smaller logical canvas keeps type and hit targets readable on phone-sized CSS screens.
                    var scaler = configuredHud.GetComponent<CanvasScaler>();
                    if (scaler != null) scaler.referenceResolution = new Vector2(900f, 500f);
                }
                configuredHud.SetTouchControls(touch);
            }
            ApplySuspension();
        }

        private void ApplySuspension()
        {
            if (!shouldSuspend || game == null || game.Hud == null) return;
            shouldSuspend = false;
            game.Controls.Clear();
            game.Commit();
            // Preserve an existing question/workshop modal. Never silently resume a game after a tab switch.
            if (game.Running && !game.Hud.Paused) game.Hud.Pause();
        }

        private void OnDestroy() { if (configured && touch) Application.targetFrameRate = previousFrameRate; }
    }
}
