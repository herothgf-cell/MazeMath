namespace MazeMath.Rewards
{
    public enum RewardGrantResult
    {
        Granted,
        Pending,
        Duplicate,
        Invalid
    }

    public interface IRewardService
    {
        RewardGrantResult Grant(string sourceId, LearningReward reward);
        int FlushPending();
        int PendingCount { get; }
    }
}
