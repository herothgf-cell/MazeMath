using System;
using MazeMath.Questions.Data;

namespace MazeMath.Questions.Runtime
{
    public enum QuestionSubmitResult
    {
        Correct,
        Incorrect,
        Invalid,
        AlreadyCompleted,
        NoQuestion
    }

    public sealed class QuestionSession
    {
        public QuestionInstance Current { get; private set; }
        public bool IsCompleted { get; private set; }
        public int IncorrectAttempts { get; private set; }

        public event Action<QuestionInstance> Completed;

        public void Begin(QuestionInstance question)
        {
            Current = question ?? throw new ArgumentNullException(nameof(question));
            IsCompleted = false;
            IncorrectAttempts = 0;
        }

        public QuestionSubmitResult SubmitNumeric(string input)
        {
            if (Current == null) return QuestionSubmitResult.NoQuestion;
            if (IsCompleted) return QuestionSubmitResult.AlreadyCompleted;

            var maxLength = Current.MaxInputLength > 0 ? Current.MaxInputLength : 3;
            var result = NumericAnswerValidator.Validate(input, Current.CorrectInteger, maxLength);

            if (result == NumericAnswerResult.Correct)
            {
                Complete();
                return QuestionSubmitResult.Correct;
            }

            if (result == NumericAnswerResult.Incorrect)
            {
                IncorrectAttempts++;
                return QuestionSubmitResult.Incorrect;
            }

            return QuestionSubmitResult.Invalid;
        }

        public QuestionSubmitResult SubmitChoice(int choiceIndex)
        {
            if (Current == null) return QuestionSubmitResult.NoQuestion;
            if (IsCompleted) return QuestionSubmitResult.AlreadyCompleted;
            if (Current.Choices == null ||
                choiceIndex < 0 ||
                choiceIndex >= Current.Choices.Length)
            {
                return QuestionSubmitResult.Invalid;
            }

            if (choiceIndex == Current.CorrectChoiceIndex)
            {
                Complete();
                return QuestionSubmitResult.Correct;
            }

            IncorrectAttempts++;
            return QuestionSubmitResult.Incorrect;
        }

        private void Complete()
        {
            IsCompleted = true;
            Completed?.Invoke(Current);
        }
    }
}
