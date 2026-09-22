using UnityEngine;

namespace MazeMath.Maze.Data
{
    public enum GateType
    {
        Key,
        Switch,
        Question,
        EnvironmentPuzzle,
        Equipment,
        BossShield
    }

    [CreateAssetMenu(menuName = "MazeMath/Maze/Gate")]
    public sealed class GateDefinition : ScriptableObject
    {
        public string gateId;
        public GateType type;
        public string requirementId;
        public bool persistentAfterSolved = true;
        public string openFeedbackKey;
    }
}
