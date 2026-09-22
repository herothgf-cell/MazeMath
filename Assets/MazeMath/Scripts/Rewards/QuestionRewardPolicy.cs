using System.Collections.Generic;
using MazeMath.Questions.Data;

namespace MazeMath.Rewards
{
    public static class QuestionRewardPolicy
    {
        public static LearningReward For(QuestionInstance question)
        {
            var xp = 5;
            if (question != null)
            {
                switch (question.Difficulty)
                {
                    case DifficultyBand.Normal: xp = 8; break;
                    case DifficultyBand.Think: xp = 12; break;
                    case DifficultyBand.Challenge: xp = 16; break;
                }
            }

            return new LearningReward(
                xp,
                new List<ItemReward> { new ItemReward("material.scrap", 1) });
        }
    }
}
