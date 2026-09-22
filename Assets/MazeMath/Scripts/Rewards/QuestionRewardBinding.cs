using System;
using MazeMath.Questions.Data;
using MazeMath.Questions.Runtime;

namespace MazeMath.Rewards
{
    public sealed class QuestionRewardBinding : IDisposable
    {
        private readonly QuestionSession session;
        private readonly IRewardService rewardService;

        public QuestionRewardBinding(QuestionSession session, IRewardService rewardService)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.rewardService = rewardService ?? throw new ArgumentNullException(nameof(rewardService));
            session.Completed += OnCompleted;
        }

        public void Dispose()
        {
            session.Completed -= OnCompleted;
        }

        private void OnCompleted(QuestionInstance question)
        {
            rewardService.Grant(
                "question:" + question.InstanceId,
                QuestionRewardPolicy.For(question));
        }
    }
}
