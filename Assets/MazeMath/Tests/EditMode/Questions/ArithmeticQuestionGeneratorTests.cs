using MazeMath.Questions.Data;
using MazeMath.Questions.Generation.Arithmetic;
using NUnit.Framework;
using UnityEngine;

namespace MazeMath.Tests.Questions
{
    public sealed class ArithmeticQuestionGeneratorTests
    {
        [Test]
        public void Addition_SameSeedProducesSameQuestion()
        {
            var template = CreateTemplate(QuestionType.NumericInput);
            template.minOperand = 1;
            template.maxOperand = 20;
            template.maxAnswer = 30;

            var generator = new AdditionGenerator();

            var first = generator.Generate(template, 123);
            var second = generator.Generate(template, 123);

            Assert.AreEqual(first.Prompt, second.Prompt);
            Assert.AreEqual(first.CorrectInteger, second.CorrectInteger);
            CollectionAssert.AreEqual(first.Operands, second.Operands);
        }

        [Test]
        public void Addition_DifferentSeedsProduceVariety()
        {
            var template = CreateTemplate(QuestionType.NumericInput);
            template.minOperand = 1;
            template.maxOperand = 20;
            template.maxAnswer = 30;

            var generator = new AdditionGenerator();
            var signatures = new System.Collections.Generic.HashSet<string>();

            for (var seed = 0; seed < 30; seed++)
            {
                var question = generator.Generate(template, seed);
                signatures.Add(string.Join(",", question.Operands));
            }

            Assert.That(signatures.Count, Is.GreaterThan(5));
        }

        [Test]
        public void Subtraction_NeverProducesNegativeAnswer()
        {
            var template = CreateTemplate(QuestionType.NumericInput);
            template.minOperand = 1;
            template.maxOperand = 30;
            template.maxAnswer = 30;

            var generator = new SubtractionGenerator();

            for (var seed = 0; seed < 1000; seed++)
            {
                var question = generator.Generate(template, seed);
                Assert.That(question.CorrectInteger, Is.GreaterThanOrEqualTo(0));
            }
        }

        private static QuestionTemplateDefinition CreateTemplate(QuestionType type)
        {
            var template = ScriptableObject.CreateInstance<QuestionTemplateDefinition>();
            template.templateId = "test";
            template.type = type;
            template.primaryAxis = LearningAxis.Addition;
            template.difficulty = DifficultyBand.Easy;
            template.maxInputLength = 3;
            return template;
        }
    }
}
