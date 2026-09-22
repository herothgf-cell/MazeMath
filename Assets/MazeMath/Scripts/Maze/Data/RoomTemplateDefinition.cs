using System.Collections.Generic;
using UnityEngine;
using MazeMath.Maze;

namespace MazeMath.Maze.Data
{
    [CreateAssetMenu(menuName = "MazeMath/Maze/Room Template")]
    public sealed class RoomTemplateDefinition : ScriptableObject
    {
        public string templateId;
        public RoomType type;
        public GameObject prefab;
        public List<string> tags = new List<string>();
        public int weight = 1;
        public bool canBeCriticalPath = true;
        public bool canBeOptional = true;
        public int floorMin;
        public int floorMax = 2;

        public bool SupportsFloor(int floor)
        {
            return floor >= floorMin && floor <= floorMax;
        }
    }
}
