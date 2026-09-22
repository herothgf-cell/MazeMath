using System;
using MazeMath.Maze.Generation;
using MazeMath.Questions.Data;

namespace MazeMath.Questions.Generation.Arithmetic
{
    public sealed class SubtractionGenerator : IQuestionGenerator
    {
        public QuestionInstance Generate(QuestionTemplateDefinition template, int seed)
        {
            Validate(template);

            var random = new DeterministicRandom(seed);
            var first = random.NextInt(template.minOperand, template.maxOperand + 1);
            var second = random.NextInt(template.minOperand, template.maxOperand + 1);

            var minuend = Math.Max(first, second);
            var subtrahend = Math.Min(first, second);

            return new QuestionInstance
            {
                InstanceId = template.templateId + ":" + seed,
                TemplateId = template.templateId,
                Seed = seed,
                Type = template.type,
                Prompt = $"{minuend} - {subtrahend} = ?",
                CorrectInteger = minuend - subtrahend,
                Operands = new[] { minuend, subtrahend },
                Difficulty = template.difficulty,
                PrimaryAxis = LearningAxis.Subtraction,
                MaxInputLength = template.maxInputLength
            };
        }

        private static void Validate(QuestionTemplateDefinition template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (template.minOperand < 0 || template.maxOperand < template.minOperand)
            {
                throw new InvalidOperationException("Subtraction template range is invalid.");
            }
        }
    }
}
