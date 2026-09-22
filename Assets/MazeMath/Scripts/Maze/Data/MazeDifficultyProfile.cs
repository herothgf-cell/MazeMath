using UnityEngine;

namespace MazeMath.Maze.Data
{
    [CreateAssetMenu(menuName = "MazeMath/Maze/Difficulty Profile")]
    public sealed class MazeDifficultyProfile : ScriptableObject
    {
        public int minCriticalPathRooms = 6;
        public int maxCriticalPathRooms = 8;
        public int maxFloorTransitions = 1;
        public int maxSimultaneousObjectives = 1;
        public int hintDelaySeconds = 90;
        public int mapRevealRadius = 0;
        public bool allowOneWayGate;
        public bool allowHiddenRoom;
        public bool allowMultiFloorDependency;
    }
}
