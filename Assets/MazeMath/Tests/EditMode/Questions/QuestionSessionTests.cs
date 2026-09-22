using MazeMath.Questions.Data;
using MazeMath.Questions.Runtime;
using NUnit.Framework;

namespace MazeMath.Tests.Questions
{
    public sealed class QuestionSessionTests
    {
        [Test]
        public void NumericCorrect_CompletesOnlyOnce()
        {
            var session = new QuestionSession();
            session.Begin(new QuestionInstance
            {
                InstanceId = "q1",
                Type = QuestionType.NumericInput,
                CorrectInteger = 23,
                MaxInputLength = 3
            });

            Assert.AreEqual(QuestionSubmitResult.Correct, session.SubmitNumeric("23"));
            Assert.IsTrue(session.IsCompleted);
            Assert.AreEqual(QuestionSubmitResult.AlreadyCompleted, session.SubmitNumeric("23"));
        }

        [Test]
        public void ChoiceWrongThenCorrect_TracksAttempts()
        {
            var session = new QuestionSession();
            session.Begin(new QuestionInstance
            {
                InstanceId = "q2",
                Type = QuestionType.MultipleChoice,
                Choices = new[] { 21, 23, 25 },
                CorrectInteger = 23,
                CorrectChoiceIndex = 1
            });

            Assert.AreEqual(QuestionSubmitResult.Incorrect, session.SubmitChoice(0));
            Assert.AreEqual(1, session.IncorrectAttempts);
            Assert.AreEqual(QuestionSubmitResult.Correct, session.SubmitChoice(1));
        }

        [Test]
        public void ChoiceOutsideRange_IsInvalid()
        {
            var session = new QuestionSession();
            session.Begin(new QuestionInstance
            {
                InstanceId = "q3",
                Type = QuestionType.MultipleChoice,
                Choices = new[] { 1, 2, 3 },
                CorrectChoiceIndex = 1
            });

            Assert.AreEqual(QuestionSubmitResult.Invalid, session.SubmitChoice(99));
        }
    }
}
