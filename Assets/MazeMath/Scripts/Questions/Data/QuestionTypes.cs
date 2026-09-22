namespace MazeMath.Questions.Data
{
    public enum QuestionType
    {
        MultipleChoice,
        NumericInput,
        StepInput,
        MissingNumber,
        RulePattern
    }

    public enum LearningAxis
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        NumberSense,
        Sequence,
        RuleInference,
        WorkingMemory,
        SpatialRotation,
        SpatialPlanning,
        VisualSearch,
        MultiStepPlanning
    }

    public enum DifficultyBand
    {
        Easy = 1,
        Normal = 2,
        Think = 3,
        Challenge = 4
    }
}
