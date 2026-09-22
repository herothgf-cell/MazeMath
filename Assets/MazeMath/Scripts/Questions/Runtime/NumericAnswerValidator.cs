namespace MazeMath.Questions.Runtime
{
    public enum NumericAnswerResult
    {
        Correct,
        Incorrect,
        Empty,
        Invalid,
        TooLong
    }

    public static class NumericAnswerValidator
    {
        public static NumericAnswerResult Validate(
            string input,
            int expectedAnswer,
            int maxLength)
        {
            if (string.IsNullOrEmpty(input))
            {
                return NumericAnswerResult.Empty;
            }

            if (maxLength <= 0 || input.Length > maxLength)
            {
                return NumericAnswerResult.TooLong;
            }

            if (!int.TryParse(input, out var value))
            {
                return NumericAnswerResult.Invalid;
            }

            return value == expectedAnswer
                ? NumericAnswerResult.Correct
                : NumericAnswerResult.Incorrect;
        }
    }
}
