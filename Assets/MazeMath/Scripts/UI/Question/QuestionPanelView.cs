using System;
using MazeMath.Adventure;
using MazeMath.Questions.Data;
using MazeMath.Questions.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MazeMath.UI.Question
{
    /// <summary>Standalone/demo question card shares the current adventure's theme and dismissal lifecycle.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class QuestionPanelView : MonoBehaviour
    {
        private Text promptText, feedbackText;
        private NumericKeypadView keypad;
        private MultipleChoiceView choices;
        private QuestionSession session;
        private QuestionInstance shownQuestion;
        private AdventureTheme theme;
        private readonly QuestionFeedbackFlow flow = new QuestionFeedbackFlow();
        private bool built;
        private Vector2 lastParentSize;
        public bool FeedbackPending => flow.IsPending;
        public event Action Closed;

        public void EnsureRuntimeUi()
        {
            if (built) return;
            built = true;
            theme = new AdventureTheme();
            var card = theme.Box(transform, "QuestionCard", 0, 0, 1, 1);
            card.GetComponent<Image>().raycastTarget = true;
            theme.Label(card, theme.T("모모와 숫자 탐험", "Numbers with Momo"), 17, TextAnchor.MiddleLeft, .06f, .90f, .83f, .99f);
            theme.Button(card, "×", Hide, .88f, .91f, .98f, .99f);
            promptText = theme.Label(card, string.Empty, 28, TextAnchor.MiddleCenter, .06f, .74f, .94f, .90f);
            feedbackText = theme.Label(card, string.Empty, 16, TextAnchor.MiddleCenter, .06f, .63f, .94f, .74f);
            var keypadRoot = theme.Rect(card, "NumericKeypad", .18f, .04f, .82f, .63f);
            keypad = keypadRoot.gameObject.AddComponent<NumericKeypadView>();
            keypad.UseTheme(theme);
            var choiceRoot = theme.Rect(card, "MultipleChoices", .10f, .08f, .90f, .62f);
            choices = choiceRoot.gameObject.AddComponent<MultipleChoiceView>();
            choices.UseTheme(theme);
            Reflow();
        }

        public void ShowQuestion(QuestionInstance question, QuestionSession questionSession)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));
            if (questionSession == null) throw new ArgumentNullException(nameof(questionSession));
            EnsureRuntimeUi();
            DetachSession();
            flow.Cancel();
            gameObject.SetActive(true);
            shownQuestion = question;
            session = questionSession;
            session.Begin(question);
            session.Completed += OnSessionCompleted;
            flow.Begin();
            promptText.text = question.Prompt;
            feedbackText.text = string.Empty;
            var isChoice = question.Type == QuestionType.MultipleChoice;
            keypad.gameObject.SetActive(!isChoice);
            choices.gameObject.SetActive(isChoice);
            if (isChoice) choices.ShowChoices(question.Choices, OnChoiceSelected);
            else keypad.Configure(question.MaxInputLength > 0 ? question.MaxInputLength : 3, OnNumericSubmitted);
            choices.SetInteractable(true);
            keypad.SetInteractable(true);
            Reflow();
        }

        public QuestionSubmitResult SubmitNumeric(string text)
        {
            if (session == null || shownQuestion == null) return QuestionSubmitResult.NoQuestion;
            if (!flow.CanSubmit) return QuestionSubmitResult.AlreadyCompleted;
            var expected = shownQuestion;
            var result = session.SubmitNumeric(text);
            if (ReferenceEquals(expected, shownQuestion)) ApplyFeedback(result);
            return result;
        }
        public QuestionSubmitResult SubmitChoice(int index)
        {
            if (session == null || shownQuestion == null) return QuestionSubmitResult.NoQuestion;
            if (!flow.CanSubmit) return QuestionSubmitResult.AlreadyCompleted;
            var expected = shownQuestion;
            var result = session.SubmitChoice(index);
            if (ReferenceEquals(expected, shownQuestion)) ApplyFeedback(result);
            return result;
        }
        private void OnNumericSubmitted(string text) { SubmitNumeric(text); }
        private void OnChoiceSelected(int index) { SubmitChoice(index); }

        private void OnSessionCompleted(QuestionInstance question)
        {
            if (!isActiveAndEnabled || !ReferenceEquals(question, shownQuestion)) return;
            if (!flow.RecordAnswer(true, Time.unscaledTime)) return;
            feedbackText.text = theme.T("정답이에요! 잘했어요.", "Correct! Nicely done.");
            feedbackText.color = theme.Green;
            keypad.SetInteractable(false);
            choices.SetInteractable(false);
        }
        private void ApplyFeedback(QuestionSubmitResult result)
        {
            if (feedbackText == null || !isActiveAndEnabled) return;
            if (result == QuestionSubmitResult.Correct) OnSessionCompleted(shownQuestion);
            else if (result == QuestionSubmitResult.Incorrect || result == QuestionSubmitResult.Invalid)
            {
                feedbackText.text = result == QuestionSubmitResult.Incorrect
                    ? theme.T("괜찮아요. 한 번 더 생각해 볼까요?", "That's okay. Try again.")
                    : theme.T("숫자를 확인해 주세요.", "Please check the number.");
                feedbackText.color = theme.Gold;
            }
        }

        private void Update()
        {
            // Uses unscaled time; never depends on the caller advancing its gameplay clock.
            if (flow.ShouldClose(Time.unscaledTime)) { Hide(); return; }
            if (!built) return;
            var parent = transform.parent as RectTransform;
            if (parent != null && lastParentSize != parent.rect.size) Reflow();
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null || session == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame) { Hide(); return; }
            if (!flow.CanSubmit || shownQuestion.Type == QuestionType.MultipleChoice) return;
            var digits = new[] { keyboard.digit0Key, keyboard.digit1Key, keyboard.digit2Key, keyboard.digit3Key, keyboard.digit4Key, keyboard.digit5Key, keyboard.digit6Key, keyboard.digit7Key, keyboard.digit8Key, keyboard.digit9Key };
            var pads = new[] { keyboard.numpad0Key, keyboard.numpad1Key, keyboard.numpad2Key, keyboard.numpad3Key, keyboard.numpad4Key, keyboard.numpad5Key, keyboard.numpad6Key, keyboard.numpad7Key, keyboard.numpad8Key, keyboard.numpad9Key };
            for (int i = 0; i < 10; i++) if (digits[i].wasPressedThisFrame || pads[i].wasPressedThisFrame) keypad.PressDigit(i);
            if (keyboard.backspaceKey.wasPressedThisFrame) keypad.Backspace();
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) keypad.Submit();
#endif
        }
        private void Reflow()
        {
            var root = GetComponent<RectTransform>();
            var parent = root.parent as RectTransform;
            var available = parent != null ? parent.rect.size : new Vector2(560f, 600f);
            if (available.x < 1 || available.y < 1) available = new Vector2(560f, 600f);
            lastParentSize = available;
            root.anchorMin = root.anchorMax = new Vector2(.5f, .5f);
            root.pivot = new Vector2(.5f, .5f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = new Vector2(Mathf.Min(520f, Mathf.Max(1f, available.x - 24f)), Mathf.Min(560f, Mathf.Max(1f, available.y - 24f)));
            keypad?.Reflow();
        }
        public void Hide()
        {
            if (!gameObject.activeSelf) return;
            flow.Cancel();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            gameObject.SetActive(false);
            Closed?.Invoke();
        }
        private void DetachSession()
        {
            if (session != null) session.Completed -= OnSessionCompleted;
            session = null;
            shownQuestion = null;
        }
        private void OnDisable() { flow.Cancel(); DetachSession(); }
        private void OnDestroy() { DetachSession(); theme?.Dispose(); }
    }
}
