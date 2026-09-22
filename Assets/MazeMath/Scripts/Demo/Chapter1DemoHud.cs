using System;
using MazeMath.Crafting;
using MazeMath.Questions.Data;
using MazeMath.Questions.Runtime;
using MazeMath.UI.Crafting;
using MazeMath.UI.Question;
using MazeMath.UI.Puzzles;
using UnityEngine;
using UnityEngine.UI;

namespace MazeMath.Demo
{
    public sealed class Chapter1DemoHud : MonoBehaviour
    {
        private Text statusText;
        private Text messageText;
        private QuestionPanelView questionPanel;
        private PatternCraftingView patternPanel;
        private EnvironmentPuzzleDemoView puzzlePanel;
        private bool built;

        public event Action PreviousRequested;
        public event Action NextRequested;
        public event Action QuestionRequested;
        public event Action CraftMiningRequested;
        public event Action PatternSensorRequested;
        public event Action EnchantSensorRequested;
        public event Action PuzzleRequested;

        public void EnsureBuilt()
        {
            if (built) return;
            built = true;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            }

            if (canvas == null)
            {
                var canvasObject = new GameObject(
                    "Chapter1Canvas",
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster));
                canvasObject.transform.SetParent(transform, false);
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            var panel = new GameObject("DemoControlPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0.25f, 1f);
            panelRect.offsetMin = new Vector2(12f, 12f);
            panelRect.offsetMax = new Vector2(-12f, -12f);
            panel.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.08f, 0.92f);

            statusText = CreateText(panel.transform, "Status", 20, TextAnchor.UpperLeft);
            SetRect(statusText.rectTransform, 0.06f, 0.62f, 0.94f, 0.96f);

            messageText = CreateText(panel.transform, "Message", 24, TextAnchor.MiddleCenter);
            SetRect(messageText.rectTransform, 0.06f, 0.50f, 0.94f, 0.62f);
            messageText.color = new Color(1f, 0.86f, 0.34f);

            var buttonArea = new GameObject(
                "Buttons",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup));
            buttonArea.transform.SetParent(panel.transform, false);
            var buttonRect = buttonArea.GetComponent<RectTransform>();
            SetRect(buttonRect, 0.06f, 0.04f, 0.94f, 0.48f);

            var layout = buttonArea.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            AddButton(buttonArea.transform, "← 이전 방", () => PreviousRequested?.Invoke());
            AddButton(buttonArea.transform, "다음 방 →", () => NextRequested?.Invoke());
            AddButton(buttonArea.transform, "수학 문제", () => QuestionRequested?.Invoke());
            AddButton(buttonArea.transform, "환경 퍼즐", () => PuzzleRequested?.Invoke());
            AddButton(buttonArea.transform, "Mining Arm 제작", () => CraftMiningRequested?.Invoke());
            AddButton(buttonArea.transform, "3×3 센서 제작", () => PatternSensorRequested?.Invoke());
            AddButton(buttonArea.transform, "센서 인챈트", () => EnchantSensorRequested?.Invoke());

            var questionObject = new GameObject("QuestionPanel", typeof(RectTransform));
            questionObject.transform.SetParent(canvas.transform, false);
            var questionRect = questionObject.GetComponent<RectTransform>();
            questionRect.anchorMin = new Vector2(0.36f, 0.08f);
            questionRect.anchorMax = new Vector2(0.78f, 0.92f);
            questionRect.offsetMin = Vector2.zero;
            questionRect.offsetMax = Vector2.zero;
            questionPanel = questionObject.AddComponent<QuestionPanelView>();
            questionPanel.EnsureRuntimeUi();
            questionObject.SetActive(false);

            var patternObject = new GameObject("PatternPanel", typeof(RectTransform));
            patternObject.transform.SetParent(canvas.transform, false);
            var patternRect = patternObject.GetComponent<RectTransform>();
            patternRect.anchorMin = new Vector2(0.40f, 0.14f);
            patternRect.anchorMax = new Vector2(0.72f, 0.86f);
            patternRect.offsetMin = Vector2.zero;
            patternRect.offsetMax = Vector2.zero;
            patternPanel = patternObject.AddComponent<PatternCraftingView>();
            patternPanel.EnsureRuntimeUi();
            patternObject.SetActive(false);

            var puzzleObject = new GameObject("EnvironmentPuzzlePanel", typeof(RectTransform));
            puzzleObject.transform.SetParent(canvas.transform, false);
            var puzzleRect = puzzleObject.GetComponent<RectTransform>();
            puzzleRect.anchorMin = new Vector2(0.38f, 0.12f);
            puzzleRect.anchorMax = new Vector2(0.74f, 0.88f);
            puzzleRect.offsetMin = Vector2.zero;
            puzzleRect.offsetMax = Vector2.zero;
            puzzlePanel = puzzleObject.AddComponent<EnvironmentPuzzleDemoView>();
            puzzlePanel.EnsureRuntimeUi();
            puzzleObject.SetActive(false);
        }

        public void SetStatus(string text)
        {
            EnsureBuilt();
            statusText.text = text ?? string.Empty;
        }

        public void SetMessage(string text)
        {
            EnsureBuilt();
            messageText.text = text ?? string.Empty;
        }

        public void ShowQuestion(QuestionInstance question, QuestionSession session)
        {
            EnsureBuilt();
            patternPanel.gameObject.SetActive(false);
            puzzlePanel.gameObject.SetActive(false);
            questionPanel.gameObject.SetActive(true);
            questionPanel.ShowQuestion(question, session);
        }

        public void HideQuestion()
        {
            if (questionPanel != null)
                questionPanel.gameObject.SetActive(false);
        }

        public void ShowPatternCrafting(
            ICraftingService service,
            string recipeId,
            string[] availableItems,
            Action<CraftResult> result)
        {
            EnsureBuilt();
            questionPanel.gameObject.SetActive(false);
            puzzlePanel.gameObject.SetActive(false);
            patternPanel.gameObject.SetActive(true);
            patternPanel.Configure(service, recipeId, availableItems, result);
        }

        public void HidePatternCrafting()
        {
            if (patternPanel != null)
                patternPanel.gameObject.SetActive(false);
        }

        public void ShowEnvironmentPuzzles(Action<string> onSolved)
        {
            EnsureBuilt();
            questionPanel.gameObject.SetActive(false);
            patternPanel.gameObject.SetActive(false);
            puzzlePanel.gameObject.SetActive(true);
            puzzlePanel.Configure(onSolved);
        }

        public void HideEnvironmentPuzzles()
        {
            if (puzzlePanel != null)
                puzzlePanel.gameObject.SetActive(false);
        }

        public void HideAllOverlays()
        {
            HideQuestion();
            HidePatternCrafting();
            HideEnvironmentPuzzles();
        }

        private static Text CreateText(
            Transform parent,
            string name,
            int fontSize,
            TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void AddButton(
            Transform parent,
            string label,
            UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(0.22f, 0.25f, 0.28f);
            go.GetComponent<LayoutElement>().preferredHeight = 52f;

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(action);

            var text = CreateText(go.transform, "Label", 20, TextAnchor.MiddleCenter);
            text.text = label;
            var rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(
            RectTransform rect,
            float minX,
            float minY,
            float maxX,
            float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
