using System;

namespace MazeMath.Questions.Data
{
    [Serializable]
    public sealed class QuestionInstance
    {
        public string InstanceId { get; set; }
        public string TemplateId { get; set; }
        public int Seed { get; set; }
        public QuestionType Type { get; set; }
        public string Prompt { get; set; }
        public int CorrectInteger { get; set; }
        public int[] Operands { get; set; }
        public DifficultyBand Difficulty { get; set; }
        public LearningAxis PrimaryAxis { get; set; }
        public int MaxInputLength { get; set; }
    }
}
