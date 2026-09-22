using System;
using System.Collections.Generic;
using MazeMath.Maze.Generation;
using MazeMath.Questions.Data;

namespace MazeMath.Questions.Generation
{
    public static class ChoiceDistractorBuilder
    {
        public static int[] Build(int correct, int choiceCount, int seed)
        {
            if (choiceCount < 2) throw new ArgumentOutOfRangeException(nameof(choiceCount));

            var values = new HashSet<int> { correct };
            var offsets = new[] { -1, 1, -2, 2, -10, 10, -5, 5 };
            var random = new DeterministicRandom(seed);

            for (var i = 0; i < offsets.Length && values.Count < choiceCount; i++)
            {
                var candidate = correct + offsets[(i + random.NextInt(0, offsets.Length)) % offsets.Length];
                if (candidate >= 0) values.Add(candidate);
            }

            while (values.Count < choiceCount)
            {
                var spread = Math.Max(4, Math.Abs(correct) + 5);
                var candidate = random.NextInt(0, spread * 2);
                values.Add(candidate);
            }

            var result = new int[values.Count];
            values.CopyTo(result);
            Shuffle(result, random);
            return result;
        }

        public static QuestionInstance AddChoices(QuestionInstance question, int choiceCount)
        {
            if (question == null) throw new ArgumentNullException(nameof(question));

            question.Choices = Build(question.CorrectInteger, choiceCount, question.Seed ^ 0x4A17);
            question.CorrectChoiceIndex = Array.IndexOf(question.Choices, question.CorrectInteger);
            question.Type = QuestionType.MultipleChoice;
            return question;
        }

        private static void Shuffle(int[] values, DeterministicRandom random)
        {
            for (var i = values.Length - 1; i > 0; i--)
            {
                var j = random.NextInt(0, i + 1);
                var temp = values[i];
                values[i] = values[j];
                values[j] = temp;
            }
        }
    }
}
