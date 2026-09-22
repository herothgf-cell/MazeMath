using System.Linq;
using MazeMath.Questions.Data;
using MazeMath.Questions.Generation;
using MazeMath.Questions.Generation.Arithmetic;
using MazeMath.Questions.Generation.Pattern;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Questions
{
    public sealed class ExtendedQuestionGeneratorTests
    {
        [Test]
        public void Multiplication_GeneratesValidProduct()
        {
            var template = CreateTemplate(QuestionType.NumericInput);
            template.minOperand = 2;
            template.maxOperand = 10;
            template.maxAnswer = 100;

            var question = new MultiplicationGenerator().Generate(template, 44);

            Assert.AreEqual(question.Operands[0] * question.Operands[1], question.CorrectInteger);
            Assert.AreEqual(LearningAxis.Multiplication, question.PrimaryAxis);
        }

        [Test]
        public void Division_AlwaysHasNoRemainder()
        {
            var template = CreateTemplate(QuestionType.NumericInput);
            template.minOperand = 2;
            template.maxOperand = 10;
            template.maxAnswer = 100;

            var generator = new DivisionGenerator();

            for (var seed = 0; seed < 1000; seed++)
            {
                var question = generator.Generate(template, seed);
                Assert.AreEqual(0, question.Operands[0] % question.Operands[1]);
                Assert.AreEqual(question.Operands[0] / question.Operands[1], question.CorrectInteger);
            }
        }

        [Test]
        public void MissingNumber_AnswerCompletesEquation()
        {
            var template = CreateTemplate(QuestionType.MissingNumber);
            template.minOperand = 2;
            template.maxOperand = 15;
            template.maxAnswer = 30;

            var question = new MissingNumberGenerator().Generate(template, 91);

            Assert.AreEqual(
                question.Operands[2],
                question.CorrectInteger);
            Assert.IsTrue(question.Prompt.Contains("□"));
        }

        [Test]
        public void Distractors_AreUniqueAndContainCorrectAnswerOnce()
        {
            var choices = ChoiceDistractorBuilder.Build(23, 4, 77);

            Assert.AreEqual(4, choices.Length);
            Assert.AreEqual(4, choices.Distinct().Count());
            Assert.AreEqual(1, choices.Count(value => value == 23));
        }

        private static QuestionTemplateDefinition CreateTemplate(QuestionType type)
        {
            var template = ScriptableObject.CreateInstance<QuestionTemplateDefinition>();
            template.templateId = "extended-test";
            template.type = type;
            template.primaryAxis = LearningAxis.Addition;
            template.difficulty = DifficultyBand.Normal;
            template.maxInputLength = 3;
            return template;
        }
    }
}
