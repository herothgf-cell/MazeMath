using MazeMath.Questions.Data;
using MazeMath.Questions.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace MazeMath.UI.Question
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class QuestionPanelView : MonoBehaviour
    {
        private Text promptText;
        private Text feedbackText;
        private NumericKeypadView keypad;
        private MultipleChoiceView choices;
        private QuestionSession session;
        private bool built;

        public void EnsureRuntimeUi()
        {
            if (built) return;
            built = true;

            var rect = GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(720f, 820f);

            var image = GetComponent<Image>();
            if (image == null) image = gameObject.AddComponent<Image>();
            image.color = new Color(0.08f, 0.09f, 0.11f, 0.97f);

            promptText = CreateText("Prompt", 42, TextAnchor.MiddleCenter);
            SetRect(promptText.rectTransform, new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.96f));

            feedbackText = CreateText("Feedback", 26, TextAnchor.MiddleCenter);
            SetRect(feedbackText.rectTransform, new Vector2(0.08f, 0.69f), new Vector2(0.92f, 0.78f));

            var keypadObject = new GameObject("NumericKeypad", typeof(RectTransform));
            keypadObject.transform.SetParent(transform, false);
            var keypadRect = keypadObject.GetComponent<RectTransform>();
            keypadRect.anchorMin = new Vector2(0.20f, 0.04f);
            keypadRect.anchorMax = new Vector2(0.80f, 0.66f);
            keypadRect.offsetMin = Vector2.zero;
            keypadRect.offsetMax = Vector2.zero;
            keypad = keypadObject.AddComponent<NumericKeypadView>();

            var choicesObject = new GameObject("MultipleChoices", typeof(RectTransform));
            choicesObject.transform.SetParent(transform, false);
            var choicesRect = choicesObject.GetComponent<RectTransform>();
            choicesRect.anchorMin = new Vector2(0.10f, 0.12f);
            choicesRect.anchorMax = new Vector2(0.90f, 0.65f);
            choicesRect.offsetMin = Vector2.zero;
            choicesRect.offsetMax = Vector2.zero;
            choices = choicesObject.AddComponent<MultipleChoiceView>();
        }

        public void ShowQuestion(QuestionInstance question, QuestionSession questionSession)
        {
            EnsureRuntimeUi();
            session = questionSession;
            session.Begin(question);

            promptText.text = question.Prompt;
            feedbackText.text = string.Empty;

            var isChoice = question.Type == QuestionType.MultipleChoice;
            keypad.gameObject.SetActive(!isChoice);
            choices.gameObject.SetActive(isChoice);

            if (isChoice)
            {
                choices.ShowChoices(question.Choices, OnChoiceSelected);
            }
            else
            {
                keypad.Configure(
                    question.MaxInputLength > 0 ? question.MaxInputLength : 3,
                    OnNumericSubmitted);
            }
        }

        private void OnNumericSubmitted(string text)
        {
            ApplyFeedback(session.SubmitNumeric(text));
        }

        private void OnChoiceSelected(int index)
        {
            ApplyFeedback(session.SubmitChoice(index));
        }

        private void ApplyFeedback(QuestionSubmitResult result)
        {
            switch (result)
            {
                case QuestionSubmitResult.Correct:
                    feedbackText.text = "정답이에요!";
                    feedbackText.color = new Color(0.45f, 1f, 0.50f);
                    break;
                case QuestionSubmitResult.Incorrect:
                    feedbackText.text = "한 번 더 생각해 볼까요?";
                    feedbackText.color = new Color(1f, 0.82f, 0.30f);
                    break;
                case QuestionSubmitResult.Invalid:
                    feedbackText.text = "숫자를 확인해 주세요.";
                    feedbackText.color = new Color(1f, 0.70f, 0.30f);
                    break;
            }
        }

        private Text CreateText(string name, int size, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(transform, false);
            var text = go.GetComponent<Text>();
            text.font = MazeMath.UI.RuntimeFontProvider.Get();
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
