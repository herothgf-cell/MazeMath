using System;
using System.Collections.Generic;

namespace MazeMath.Maze.Runtime
{
    [Serializable]
    public sealed class MazeRunState
    {
        public string chapterId;
        public int runSeed;
        public string currentNodeId;
        public int currentFloor;
        public List<string> visitedNodeIds = new List<string>();
        public List<string> seenNodeIds = new List<string>();
        public List<string> clearedNodeIds = new List<string>();
        public List<string> solvedGateIds = new List<string>();
        public string activeObjectiveId;
        public string playerSpawnId;
    }
}
