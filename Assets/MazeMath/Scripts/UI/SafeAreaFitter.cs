using UnityEngine;

namespace MazeMath.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform target;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            target = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            ApplyCurrent();
        }

        private void Update()
        {
            var currentSize = new Vector2Int(Screen.width, Screen.height);
            if (Screen.safeArea != lastSafeArea || currentSize != lastScreenSize)
            {
                ApplyCurrent();
            }
        }

        public void Apply(Rect safeArea, Vector2Int screenSize)
        {
            if (target == null)
            {
                target = GetComponent<RectTransform>();
            }

            if (screenSize.x <= 0 || screenSize.y <= 0)
            {
                target.anchorMin = Vector2.zero;
                target.anchorMax = Vector2.one;
                target.offsetMin = Vector2.zero;
                target.offsetMax = Vector2.zero;
                return;
            }

            var min = safeArea.position;
            var max = safeArea.position + safeArea.size;

            min.x /= screenSize.x;
            min.y /= screenSize.y;
            max.x /= screenSize.x;
            max.y /= screenSize.y;

            target.anchorMin = min;
            target.anchorMax = max;
            target.offsetMin = Vector2.zero;
            target.offsetMax = Vector2.zero;
        }

        private void ApplyCurrent()
        {
            lastSafeArea = Screen.safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            Apply(lastSafeArea, lastScreenSize);
        }
    }
}
