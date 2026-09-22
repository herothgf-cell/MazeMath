using System;
using System.Globalization;
using MazeMath.Maze.Generation;
using MazeMath.Questions.Runtime;

namespace MazeMath.Adventure
{
    public static class AdventureQuestions
    {
        public static AdventureQuestion Ensure(AdventureState s, string source)
        {
            if (s.question != null && !s.question.solved && s.question.source == source) return s.question;
            s.question = Create(s.seed, source, s.questionIndex++); return s.question;
        }
        public static AdventureQuestion Create(int seed, string source, int index)
        {
            if (source != "math" && source != "boss.math") throw new ArgumentException("Unknown question source.");
            var r = new DeterministicRandom(unchecked(seed ^ MazeSeedService.StableHash(source) ^ index * 7919));
            int a, b, answer; string prompt;
            switch (r.NextInt(0, 5))
            {
                case 0: a = r.NextInt(12, 26); b = r.NextInt(4, 10); answer = a + b; prompt = $"{a} + {b} = ?"; break;
                case 1: a = r.NextInt(20, 36); b = r.NextInt(3, 10); answer = a - b; prompt = $"{a} - {b} = ?"; break;
                case 2: a = r.NextBool() ? 2 : 5; b = r.NextInt(2, 9); answer = a * b; prompt = $"{a} × {b} = ?"; break;
                case 3: b = r.NextBool() ? 2 : 5; answer = r.NextInt(2, 9); a = b * answer; prompt = $"{a} ÷ {b} = ?"; break;
                default: a = r.NextInt(10, 24); answer = r.NextInt(2, 10); prompt = $"{a} + □ = {a + answer}"; break;
            }
            var choices = new[]{answer, answer + 1, answer + 2, Math.Max(0, answer - 1)};
            for (int i = 3; i > 0; i--) { int j = r.NextInt(0, i + 1); int temp = choices[i]; choices[i] = choices[j]; choices[j] = temp; }
            return new AdventureQuestion { source = source, prompt = prompt, answer = answer, choices = choices, multipleChoice = r.NextInt(0, 3) == 0 };
        }
        public static bool Submit(AdventureState s, string input)
        {
            var q = s.question;
            if (q == null || q.solved) return false;
            var result = NumericAnswerValidator.Validate(input, q.answer, 3);
            if (result != NumericAnswerResult.Correct)
            {
                if (result == NumericAnswerResult.Incorrect) q.attempts = Math.Min(100000, q.attempts + 1);
                return false;
            }
            q.solved = true; AdventureRules.Complete(s, q.source); return true;
        }
        public static int[] PlateOrder(int seed)
        {
            var a = new[]{2,4,6}; var r = new DeterministicRandom(seed ^ 91817);
            for (int i = 2; i > 0; i--) { int j = r.NextInt(0, i + 1); int t = a[i]; a[i] = a[j]; a[j] = t; }
            return a;
        }
    }
}
