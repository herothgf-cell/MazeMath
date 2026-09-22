using System;
using MazeMath.Maze.Generation;
using MazeMath.Questions.Data;

namespace MazeMath.Questions.Generation.Arithmetic
{
    public sealed class MultiplicationGenerator : IQuestionGenerator
    {
        public QuestionInstance Generate(QuestionTemplateDefinition template, int seed)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (template.minOperand < 1 || template.maxOperand < template.minOperand)
                throw new InvalidOperationException("Multiplication template range is invalid.");

            var random = new DeterministicRandom(seed);
            var a = random.NextInt(template.minOperand, template.maxOperand + 1);
            var b = random.NextInt(template.minOperand, template.maxOperand + 1);

            while (a * b > template.maxAnswer && (a > template.minOperand || b > template.minOperand))
            {
                if (a >= b && a > template.minOperand) a--;
                else if (b > template.minOperand) b--;
            }

            if (a * b > template.maxAnswer)
                throw new InvalidOperationException("Multiplication template cannot satisfy maxAnswer.");

            return new QuestionInstance
            {
                InstanceId = template.templateId + ":" + seed,
                TemplateId = template.templateId,
                Seed = seed,
                Type = template.type,
                Prompt = $"{a} × {b} = ?",
                CorrectInteger = a * b,
                Operands = new[] { a, b },
                Difficulty = template.difficulty,
                PrimaryAxis = LearningAxis.Multiplication,
                MaxInputLength = template.maxInputLength
            };
        }
    }
}
