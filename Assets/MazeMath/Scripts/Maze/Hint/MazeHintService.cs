using System;
using MazeMath.Maze;

namespace MazeMath.Maze.Hint
{
    public enum MazeHintLevel
    {
        None = 0,
        Observe = 1,
        Floor = 2,
        Direction = 3,
        FullAssist = 4
    }

    public sealed class MazeHintService
    {
        private readonly double noProgressThresholdSeconds;
        private double lastProgressTime;
        private bool stuckEventLatched;

        public MazeHintLevel CurrentLevel { get; private set; }

        public MazeHintService(double noProgressThresholdSeconds)
        {
            if (noProgressThresholdSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(noProgressThresholdSeconds));
            }

            this.noProgressThresholdSeconds = noProgressThresholdSeconds;
        }

        public void RegisterProgress(double nowSeconds)
        {
            lastProgressTime = nowSeconds;
            CurrentLevel = MazeHintLevel.None;
            stuckEventLatched = false;
        }

        public MazeHintLevel Evaluate(
            double nowSeconds,
            int repeatedVisitedRoomEntries,
            int consecutiveDeadEnds)
        {
            var timedOut = nowSeconds - lastProgressTime >= noProgressThresholdSeconds;
            var repeatedRooms = repeatedVisitedRoomEntries >= 3;
            var repeatedDeadEnds = consecutiveDeadEnds >= 2;
            var stuck = timedOut || repeatedRooms || repeatedDeadEnds;

            if (stuck && !stuckEventLatched && CurrentLevel == MazeHintLevel.None)
            {
                CurrentLevel = MazeHintLevel.Observe;
                stuckEventLatched = true;
            }

            if (!stuck)
            {
                stuckEventLatched = false;
            }

            return CurrentLevel;
        }

        public MazeHintLevel RequestStrongerHint()
        {
            if (CurrentLevel == MazeHintLevel.None)
            {
                CurrentLevel = MazeHintLevel.Observe;
                return CurrentLevel;
            }

            if (CurrentLevel < MazeHintLevel.FullAssist)
            {
                CurrentLevel++;
            }

            return CurrentLevel;
        }

        public string GetNextNode(
            MazeGraph graph,
            string currentNodeId,
            string targetNodeId)
        {
            if (graph == null)
            {
                throw new ArgumentNullException(nameof(graph));
            }

            var path = graph.FindPath(currentNodeId, targetNodeId);
            return path.Count >= 2 ? path[1] : null;
        }
    }
}
