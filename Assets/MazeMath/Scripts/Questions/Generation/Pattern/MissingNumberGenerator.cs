using System;
using MazeMath.Maze.Generation;
using MazeMath.Questions.Data;

namespace MazeMath.Questions.Generation.Pattern
{
    public sealed class MissingNumberGenerator : IQuestionGenerator
    {
        public QuestionInstance Generate(QuestionTemplateDefinition template, int seed)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (template.minOperand < 0 || template.maxOperand < template.minOperand)
                throw new InvalidOperationException("Missing number template range is invalid.");

            var random = new DeterministicRandom(seed);
            var a = random.NextInt(template.minOperand, template.maxOperand + 1);
            var b = random.NextInt(template.minOperand, template.maxOperand + 1);
            var sum = a + b;

            while (sum > template.maxAnswer && b > template.minOperand)
            {
                b--;
                sum = a + b;
            }

            if (sum > template.maxAnswer)
                throw new InvalidOperationException("Missing number template cannot satisfy maxAnswer.");

            return new QuestionInstance
            {
                InstanceId = template.templateId + ":" + seed,
                TemplateId = template.templateId,
                Seed = seed,
                Type = QuestionType.MissingNumber,
                Prompt = $"{a} + □ = {sum}",
                CorrectInteger = b,
                Operands = new[] { a, sum, b },
                Difficulty = template.difficulty,
                PrimaryAxis = LearningAxis.NumberSense,
                MaxInputLength = template.maxInputLength
            };
        }
    }
}
