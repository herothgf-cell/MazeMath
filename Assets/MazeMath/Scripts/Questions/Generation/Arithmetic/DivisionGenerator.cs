using System;
using MazeMath.Maze.Generation;
using MazeMath.Questions.Data;

namespace MazeMath.Questions.Generation.Arithmetic
{
    public sealed class DivisionGenerator : IQuestionGenerator
    {
        public QuestionInstance Generate(QuestionTemplateDefinition template, int seed)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (template.minOperand < 1 || template.maxOperand < template.minOperand)
                throw new InvalidOperationException("Division template range is invalid.");

            var random = new DeterministicRandom(seed);
            var divisor = random.NextInt(template.minOperand, template.maxOperand + 1);
            var quotientMax = Math.Max(1, Math.Min(template.maxOperand, template.maxAnswer / divisor));
            var quotientMin = Math.Min(template.minOperand, quotientMax);
            var quotient = random.NextInt(quotientMin, quotientMax + 1);
            var dividend = divisor * quotient;

            return new QuestionInstance
            {
                InstanceId = template.templateId + ":" + seed,
                TemplateId = template.templateId,
                Seed = seed,
                Type = template.type,
                Prompt = $"{dividend} ÷ {divisor} = ?",
                CorrectInteger = quotient,
                Operands = new[] { dividend, divisor },
                Difficulty = template.difficulty,
                PrimaryAxis = LearningAxis.Division,
                MaxInputLength = template.maxInputLength
            };
        }
    }
}
