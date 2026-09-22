using MazeMath.Questions.Data;

namespace MazeMath.Questions.Generation
{
    public interface IQuestionGenerator
    {
        QuestionInstance Generate(QuestionTemplateDefinition template, int seed);
    }
}
