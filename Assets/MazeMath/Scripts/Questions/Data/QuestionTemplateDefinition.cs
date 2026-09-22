using UnityEngine;

namespace MazeMath.Questions.Data
{
    [CreateAssetMenu(menuName = "MazeMath/Questions/Template")]
    public sealed class QuestionTemplateDefinition : ScriptableObject
    {
        public string templateId;
        public QuestionType type = QuestionType.NumericInput;
        public LearningAxis primaryAxis = LearningAxis.Addition;
        public DifficultyBand difficulty = DifficultyBand.Easy;
        public string generatorId;
        public int minOperand = 1;
        public int maxOperand = 10;
        public int maxAnswer = 20;
        public int maxInputLength = 2;
    }
}
