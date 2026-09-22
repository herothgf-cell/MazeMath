using MazeMath.Questions.Runtime;
using NUnit.Framework;

namespace MazeMath.Tests.Questions
{
    public sealed class NumericAnswerValidatorTests
    {
        [Test]
        public void Validate_CorrectInteger_ReturnsCorrect()
        {
            var result = NumericAnswerValidator.Validate("23", 23, 3);

            Assert.AreEqual(NumericAnswerResult.Correct, result);
        }

        [Test]
        public void Validate_LeadingZeros_AreNormalized()
        {
            var result = NumericAnswerValidator.Validate("00023", 23, 5);

            Assert.AreEqual(NumericAnswerResult.Correct, result);
        }

        [Test]
        public void Validate_EmptyInput_IsRejected()
        {
            var result = NumericAnswerValidator.Validate("", 23, 3);

            Assert.AreEqual(NumericAnswerResult.Empty, result);
        }

        [Test]
        public void Validate_TooLongInput_IsRejected()
        {
            var result = NumericAnswerValidator.Validate("1234", 23, 3);

            Assert.AreEqual(NumericAnswerResult.TooLong, result);
        }

        [Test]
        public void Validate_NonNumericInput_IsRejected()
        {
            var result = NumericAnswerValidator.Validate("2a", 23, 3);

            Assert.AreEqual(NumericAnswerResult.Invalid, result);
        }
    }
}
