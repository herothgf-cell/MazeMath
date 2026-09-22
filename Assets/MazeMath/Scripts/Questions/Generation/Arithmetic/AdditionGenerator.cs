using System;
using MazeMath.Maze.Generation;
using MazeMath.Questions.Data;

namespace MazeMath.Questions.Generation.Arithmetic
{
    public sealed class AdditionGenerator : IQuestionGenerator
    {
        public QuestionInstance Generate(QuestionTemplateDefinition template, int seed)
        {
            Validate(template);

            var random = new DeterministicRandom(seed);
            var firstMax = Math.Min(template.maxOperand, template.maxAnswer - template.minOperand);
            if (firstMax < template.minOperand)
            {
                throw new InvalidOperationException("Addition template cannot produce a valid question.");
            }

            var a = random.NextInt(template.minOperand, firstMax + 1);
            var secondMax = Math.Min(template.maxOperand, template.maxAnswer - a);
            var b = random.NextInt(template.minOperand, secondMax + 1);

            return new QuestionInstance
            {
                InstanceId = template.templateId + ":" + seed,
                TemplateId = template.templateId,
                Seed = seed,
                Type = template.type,
                Prompt = $"{a} + {b} = ?",
                CorrectInteger = a + b,
                Operands = new[] { a, b },
                Difficulty = template.difficulty,
                PrimaryAxis = LearningAxis.Addition,
                MaxInputLength = template.maxInputLength
            };
        }

        private static void Validate(QuestionTemplateDefinition template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (template.minOperand < 0 ||
                template.maxOperand < template.minOperand ||
                template.maxAnswer < template.minOperand * 2)
            {
                throw new InvalidOperationException("Addition template range is invalid.");
            }
        }
    }
}
